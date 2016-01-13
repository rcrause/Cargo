﻿using Cargo.Newtonsoft;
using Cargo.Newtonsoft.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// An implementation of <see cref="ICargoDataSource"/> that reads from and writes to a normal JSON file.
    /// </summary>
    public class FileDataSource : IDisposable
    {
        private string _filename;
        JObject _items;
        private ReaderWriterLockSlim _rwl = new ReaderWriterLockSlim();
        private object _fileWriteLock = new object();
        private JsonSerializer _serializer = new JsonSerializer();
        private FileSystemWatcher _fsw;
        private DateTime _lastModifiedTime;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public string Filename
        {
            get { return _filename; }
            set
            {
                if (Filename == null) throw new ArgumentNullException("value");
                SetFileName(value);
            }
        }

        public FileDataSource(string filename)
        {
            SetFilenameInternal(filename);
            Reload(force: true, @lock: false, notify: false);
        }

        private void SetFileName(string filename)
        {
            lock (_fileWriteLock)
            {
                _rwl.EnterWriteLock();
                try
                {
                    if (filename != _filename)
                    {
                        SetFilenameInternal(filename);
                        Reload(force: true, @lock: false);
                    }
                }
                finally
                {
                    _rwl.ExitWriteLock();
                }
            }
        }

        private void SetFilenameInternal(string filename)
        {
            if (File.Exists(filename))
            {
                var fileAttributes = File.GetAttributes(filename);
                if ((fileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    throw new ArgumentException("filename cannot be a directory", "filename");
                }
            }
            else
            {
                //touch the file
                File.Create(filename).Close();
            }

            FileInfo fi = new FileInfo(filename);

            string directoryName = fi.DirectoryName;
            string onlyFileName = fi.Name;
            _lastModifiedTime = fi.LastWriteTimeUtc;

            //disable the old watcher
            if (_fsw != null) _fsw.EnableRaisingEvents = false;
            try
            {
                //create a new watcher disabled
                var FileWatcher = new FileSystemWatcher(directoryName, onlyFileName);
                FileWatcher.EnableRaisingEvents = false;
                try
                {
                    FileWatcher.Created += FileWatcher_Modified;
                    FileWatcher.Deleted += FileWatcher_Modified;
                    FileWatcher.Changed += FileWatcher_Modified;
                }
                catch
                {
                    FileWatcher.Dispose();
                    throw;
                }

                //replace the watcher
                if (_fsw != null) _fsw.Dispose();
                _fsw = FileWatcher;
            }
            finally
            {
                if (_fsw != null) _fsw.EnableRaisingEvents = true;
            }

            _filename = fi.FullName;
        }

        void FileWatcher_Modified(object sender, FileSystemEventArgs e)
        {
            Reload(false);
        }

        private void Reload(bool force, bool @lock = true, bool notify = true)
        {
            if (@lock) Monitor.Enter(_fileWriteLock);
            try
            {
                FileInfo fi = new FileInfo(_filename);

                if (!force)
                {
                    if (fi.Exists && fi.LastWriteTimeUtc <= _lastModifiedTime)
                    {
                        return;
                    }
                }

                JObject oldItems = _items;
                JObject newItems;
                bool changed = false;

                newItems = ReadFile(_filename);
                if (newItems != null)
                {
                    if (@lock) _rwl.EnterUpgradeableReadLock();
                    try
                    {
                        if (oldItems == null || !JToken.DeepEquals(newItems, _items))
                        {
                            if (@lock) _rwl.EnterWriteLock();
                            try
                            {
                                _items = newItems;
                            }
                            finally
                            {
                                if (@lock) _rwl.ExitWriteLock();
                            }
                            changed = true;
                        }
                    }
                    finally
                    {
                        if (@lock) _rwl.ExitUpgradeableReadLock();
                    }
                }

                if (changed && notify)
                {
                    NotifyChangedThorough(oldItems, newItems);
                }
            }
            finally
            {
                if (@lock) Monitor.Exit(_fileWriteLock);
            }
        }

        private void NotifyChangedThorough(JObject oldItems, JObject newItems)
        {
            if (oldItems == null) oldItems = new JObject();
            if (newItems == null) newItems = new JObject();

            var addedProps = newItems.Properties().Where(x => oldItems.Property(x.Name) == null).Select(x => new KeyValuePair<string, object>(x.Name, MakeValueIntoObject(x))).ToArray();
            var removedProps = oldItems.Properties().Where(x => newItems.Property(x.Name) == null).Select(x => new KeyValuePair<string, object>(x.Name, MakeValueIntoObject(x))).ToArray();
            var changedProps = newItems.Properties()
                .Join(oldItems.Properties(), x => x.Name, x => x.Name, (n, o) => new { n = n, o = o })
                .Where(p => !JToken.DeepEquals(p.n.Value, p.o.Value))
                .Select(p => new
                {
                    n = new KeyValuePair<string, object>(p.n.Name, MakeValueIntoObject(p.n)),
                    o = new KeyValuePair<string, object>(p.o.Name, MakeValueIntoObject(p.o))
                })
                .ToArray();

            if (addedProps.Length != 0) OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addedProps));
            if (removedProps.Length != 0) OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedProps));
            if (changedProps.Length != 0) OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, changedProps.Select(z => z.n).ToArray(), changedProps.Select(z => z.o).ToArray()));
        }

        private void Persist()
        {
            lock (_fileWriteLock)
            {
                _fsw.EnableRaisingEvents = false;
                WriteFile(_items, _filename);
                _fsw.EnableRaisingEvents = true;
            }
        }

        private JObject ReadFile(string _filename)
        {
            if (!File.Exists(_filename)) return null;

            using (var sr = File.OpenText(_filename))
            {
                using (JsonReader rdr = new JsonTextReader(sr))
                {
                    return _serializer.Deserialize<JObject>(rdr);
                }
            }
        }

        private void WriteFile(JObject toWrite, string filename)
        {
            using (var sw = File.CreateText(filename))
            {
                using (JsonWriter jsonWriter = new JsonTextWriter(sw))
                {
                    _serializer.Serialize(jsonWriter, toWrite);
                }
            }
        }

        public T GetOrSetDefault<T>(string key, T defaultValue)
        {
            bool mustWrite = false;
            T result;

            JToken tv = JToken.FromObject(defaultValue, _serializer);

            _rwl.EnterUpgradeableReadLock();
            try
            {
                var prop = _items != null ? _items.Property(key) : null;
                if (prop != null)
                {
                    result = prop.Value.ToObject<T>(_serializer);
                }
                else
                {
                    _rwl.EnterWriteLock();
                    try
                    {
                        mustWrite = SetInternal(key, tv);
                    }
                    finally
                    {
                        _rwl.ExitWriteLock();
                    }

                    result = defaultValue;
                }
            }
            finally
            {
                _rwl.ExitUpgradeableReadLock();
            }

            if (mustWrite)
            {
                Persist();
                NotifyChanged(null, new JProperty(key, tv));
            }

            return result;
        }

        private void NotifyChanged(JProperty oldItem, JProperty newItem)
        {
            Debug.Assert(oldItem != null || newItem != null);

            if (oldItem == null && newItem != null) OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<string, object>(newItem.Name, MakeValueIntoObject(newItem))));
            else if (oldItem != null && newItem == null) OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new KeyValuePair<string, object>(oldItem.Name, MakeValueIntoObject(oldItem))));
            else OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new KeyValuePair<string, object>(newItem.Name, MakeValueIntoObject(newItem)), new KeyValuePair<string, object>(oldItem.Name, MakeValueIntoObject(oldItem))));
        }

        public void Set<T>(string key, T value)
        {
            bool mustWrite;
            JProperty oldItem;
            JToken jvalue =  value == null ? JValue.CreateNull() : JToken.FromObject(value, _serializer);

            _rwl.EnterUpgradeableReadLock();
            try
            {
                if (_items != null)
                {
                    var oldprop = _items.Property(key);
                    if (oldprop != null) oldItem = new JProperty(oldprop);
                    else oldItem = null;
                }
                else oldItem = null;

                _rwl.EnterWriteLock();
                try
                {
                    mustWrite = SetInternal(key, jvalue);
                }
                finally
                {
                    _rwl.ExitWriteLock();
                }
            }
            finally
            {
                _rwl.ExitUpgradeableReadLock();
            }


            if (mustWrite)
            {
                Persist();
                NotifyChanged(oldItem, value == null ? null : new JProperty(key, jvalue));
            }
        }

        private bool SetInternal(string key, JToken objectToken)
        {
            if (_items != null)
            {
                var prop = _items.Property(key);
                if (prop != null)
                {
                    if (!JToken.DeepEquals(prop.Value, objectToken))
                    {
                        prop.Value = objectToken;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    _items.Add(key, objectToken);
                    return true;
                }
            }
            else
            {
                _items = new JObject();
                _items.Add(key, objectToken);
                return true;
            }
        }

        public T Get<T>(string key)
        {
            _rwl.EnterReadLock();
            try
            {
                if (_items != null)
                {
                    var prop = _items.Property(key);
                    if (prop != null) return prop.Value.ToObject<T>(_serializer);
                    else return default(T);
                }
                else
                {
                    return default(T);
                }
            }
            finally
            {
                _rwl.ExitReadLock();
            }
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            JProperty removed;
            if (_items != null)
            {
                _rwl.EnterUpgradeableReadLock();
                try
                {
                    removed = _items.Property(key);

                    if (removed != null)
                    {
                        _rwl.EnterWriteLock();
                        try
                        {
                            _items.Remove(key);
                        }
                        finally
                        {
                            _rwl.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    _rwl.ExitUpgradeableReadLock();
                }

                NotifyChanged(removed, null);
            }
        }

        public int Count
        {
            get
            {
                if (_items != null) return _items.Count;
                else return 0;
            }
        }

        public bool ContainsKey(string key)
        {
            if (_items != null)
            {
                _rwl.EnterReadLock();
                try
                {
                    if (_items != null)
                    {
                        return _items.Property(key) != null;
                    }
                }
                finally
                {
                    _rwl.ExitReadLock();
                }
            }

            return false;
        }

        public ICollection<string> Keys
        {
            get
            {
                _rwl.EnterReadLock();
                try
                {
                    if (_items != null)
                    {
                        return _items.Properties().Select(x => x.Name).ToArray();
                    }
                    else
                    {
                        return new string[0];
                    }
                }
                finally
                {
                    _rwl.ExitReadLock();
                }
            }
        }

        private object MakeValueIntoObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return ((JObject)token).Properties().ToDictionary(x => x.Name, x => MakeValueIntoObject(x.Value));
                case JTokenType.Array:
                    return ((JArray)token).Select(x => MakeValueIntoObject(x)).ToArray();
                case JTokenType.Integer:
                    return token.ToObject<int>(_serializer);
                case JTokenType.Float:
                    return token.ToObject<double>(_serializer);
                case JTokenType.String:
                    return token.ToObject<string>(_serializer);
                case JTokenType.Boolean:
                    return token.ToObject<bool>(_serializer);
                case JTokenType.Date:
                    return token.ToObject<DateTime>(_serializer);
                case JTokenType.Guid:
                    return token.ToObject<Guid>(_serializer);
                case JTokenType.Uri:
                    return token.ToObject<Uri>(_serializer);
                case JTokenType.TimeSpan:
                    return token.ToObject<TimeSpan>(_serializer);

                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;

                case JTokenType.Bytes:
                    return token.ToObject<byte[]>(_serializer);
                case JTokenType.Property:
                    return MakeValueIntoObject(((JProperty)token).Value);

                case JTokenType.None:
                case JTokenType.Raw:
                case JTokenType.Constructor:
                case JTokenType.Comment:
                default:
                    throw new NotSupportedException();
            }
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null) CollectionChanged(sender, e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (_fsw != null)
            {
                _fsw.Dispose();
                _fsw = null;
            }
        }

        ~FileDataSource()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
