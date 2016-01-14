using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cargo
{
    public class CargoFileDataSource : CargoDataSourceBase, IDisposable
    {
        private bool _disposed;
        private FileDataSource _fds;
        private bool _inWebApplication;
        private string _appDataPath;
        private string _rxId = @"^(.*)\/(.+)";

        public CargoFileDataSource()
            : this("cargo.json")
        {
        }

        protected override ContentItem CreateInternal(string location, string key, string content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (location == null) throw new ArgumentNullException(nameof(location));
            if (key == null) throw new ArgumentNullException(nameof(key));

            ValidateLocation(location);
            ValidateKey(key);

            var id = GetId(location, key);
            ValidateId(id);

            _fds.Set(id, new ContentItemMinimal { content = content });

            return new ContentItem
            {
                Content = content,
                Id = id,
                Key = key,
                Location = location
            };
        }

        public override ContentItem GetById(string id)
        {
            string content = _fds.Get<ContentItemMinimal>(id)?.content;
            if (content == null) return null;
            string location;
            string key;
            ParseId(id, out location, out key);

            return new ContentItem
            {
                Content = content,
                Id = id,
                Key = key,
                Location = location
            };
        }

        public override ContentItem Get(string location, string key)
        {
            ValidateLocation(location);
            ValidateKey(key);

            var id = GetId(location, key);

            string content = _fds.Get<ContentItemMinimal>(id)?.content;
            if (content == null) return null;
            return new ContentItem
            {
                Content = content,
                Id = id,
                Key = key,
                Location = location
            };
        }

        public override ICollection<ContentItem> GetAllContent()
        {
            return GetAllContentInternal()
            .ToList()
            .AsReadOnly();
        }

        public override ICollection<ContentItem> GetAllContentForLocation(string location)
        {
            return GetAllContentInternal()
                .Where(x => x.Location == location)
                .ToList()
                .AsReadOnly();
        }

        public override ICollection<string> GetAllLocations()
        {
            return _fds.Keys.Select(id =>
            {
                string location;
                string key;
                ParseId(id, out location, out key);

                return location;
            }).Distinct()
            .ToList()
            .AsReadOnly();
        }

        public override void Remove(IEnumerable<string> contentItemIds)
        { 
            foreach(var id in contentItemIds)
            {
                _fds.Remove(id);
            }
        }

        public override void SetInternal(IEnumerable<ContentItem> contentItems)
        {
            foreach(var item in contentItems)
            {
                ValidateLocation(item.Location);
                ValidateKey(item.Key);

                var id = GetId(item.Location, item.Key);
                ValidateId(id);

                _fds.Set(id, new ContentItemMinimal { content = item.Content });
            }
        }

        public override void SetByIdInternal(IEnumerable<KeyValuePair<string, string>> idContentPairs)
        {
            foreach (var item in idContentPairs)
            {
                ValidateId(item.Key);

                _fds.Set(item.Key, new ContentItemMinimal { content = item.Value });
            }
        }

        private static string GetId(string location, string key)
        {
            return $"{location}/{key}";
        }

        private IEnumerable<ContentItem> GetAllContentInternal()
        {
            return _fds.Keys.Select(id =>
            {
                string content = _fds.Get<ContentItemMinimal>(id)?.content;
                if (content == null) return null;

                string location;
                string key;
                ParseId(id, out location, out key);

                return new ContentItem
                {
                    Id = id,
                    Content = content,
                    Key = key,
                    Location = location
                };
            }).Where(x => x != null);
        }

        private void ParseId(string id, out string location, out string key)
        {
            ValidateId(id);

            //note the greedy match of the first group
            var m = Regex.Match(id, _rxId);
            location = m.Groups[1].Value;
            if (location == "") location = null;
            key = m.Groups[2].Value;
        }

        protected override void ValidateId(string id)
        {
            base.ValidateId(id);

            if (!Regex.IsMatch(id, _rxId)) throw new ArgumentException($"The id must be in the format {_rxId}");
        }

        public CargoFileDataSource(string filename)
        {
            FigureOutIfInWebApplication();

            if (!Path.IsPathRooted(filename))
            {
                if (_inWebApplication && !string.IsNullOrEmpty(_appDataPath))
                {
                    filename = Path.Combine(_appDataPath, filename);
                }
            }

            _fds = new FileDataSource(filename);
        }

        private void FigureOutIfInWebApplication()
        {
            try
            {
                var systemweb = Assembly.Load("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                string appDomainAppId = systemweb.GetType("System.Web.HttpRuntime")
                    .GetProperty("AppDomainAppId", BindingFlags.Static | BindingFlags.Public)
                    .GetGetMethod()
                    .Invoke(null, new object[0]) as string;
                _inWebApplication = appDomainAppId != null;

                if (_inWebApplication)
                {
                    _appDataPath = systemweb.GetType("System.Web.Hosting.HostingEnvironment")
                        .GetMethod("MapPath", BindingFlags.Static | BindingFlags.Public)
                        .Invoke(null, new object[] { "~/App_Data" }) as string;
                }
            }
            catch { }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_fds != null)
                    {
                        _fds.Dispose();
                        _fds = null;
                    }
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private class ContentItemMinimal
        {
            public string content { get; set; }
        }
    }
}
