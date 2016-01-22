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
    /// <summary>
    /// Provides an implementation of <see cref="ICargoDataSource"/>
    /// </summary>
    public class CargoFileDataSource : CargoDataSourceBase
    {
        private bool _disposed;
        private FileDataSource _fds;
        private bool _inWebApplication;
        private string _appDataPath;
        private string _rxId = @"^(.*)\/(.+)";

        /// <summary>
        /// Creates a new <see cref="CargoFileDataSource"/>, using the "cargo.json" as
        /// filename
        /// </summary>
        public CargoFileDataSource()
            : this("cargo.json")
        {
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override ICollection<ContentItem> GetAllContent()
        {
            return GetAllContentInternal()
            .ToList()
            .AsReadOnly();
        }

        /// <inheritdoc />
        public override ICollection<ContentItem> GetAllContentForLocation(string location)
        {
            return GetAllContentInternal()
                .Where(x => x.Location == location)
                .ToList()
                .AsReadOnly();
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override void Remove(IEnumerable<string> contentItemIds)
        { 
            foreach(var id in contentItemIds)
            {
                _fds.Remove(id);
            }
        }

        /// <inheritdoc />
        public override ContentItem GetOrCreate(string location, string key, string defaultContent)
        {
            ValidateLocation(location);
            ValidateKey(key);
            var id = GetId(location, key);

            var contentItem = _fds.Get<ContentItemMinimal>(id);

            if (contentItem == null)
            {
                _fds.Set(id, new ContentItemMinimal
                {
                    content = defaultContent
                });

                return new ContentItem
                {
                    Id = id,
                    Content = defaultContent,
                    Key = key,
                    Location = location
                };
            }
            else
            {
                return new ContentItem
                {
                    Id = id,
                    Content = contentItem.content,
                    Key = key,
                    Location = location
                };
            }
        }

        /// <inheritdoc />
        public override void Set(IEnumerable<ContentItem> contentItems)
        {
            foreach (var item in contentItems)
            {
                ValidateKey(item.Key);
                ValidateLocation(item.Location);
                var id = GetId(item.Location, item.Key);

                var contentItem = _fds.Get<ContentItemMinimal>(id);
                if (contentItem == null)
                {
                    _fds.Set(id, new ContentItemMinimal
                    {
                        content = item.Content
                    });
                }
                else
                {
                    if (contentItem.content != item.Content)
                    {
                        contentItem.content = item.Content;
                        _fds.Set(id, contentItem);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void SetById(IEnumerable<KeyValuePair<string, string>> idContentPairs)
        {
            foreach (var item in idContentPairs)
            {
                var id = item.Key;
                ValidateId(id);

                string location, key;
                ParseId(id, out location, out key);
                ValidateKey(key);
                ValidateLocation(location);

                var contentItem = _fds.Get<ContentItemMinimal>(id);
                if (contentItem == null)
                {
                    _fds.Set(id, new ContentItemMinimal
                    {
                        content = item.Value
                    });
                }
                else
                {
                    if (contentItem.content != item.Value)
                    {
                        contentItem.content = item.Value;
                        _fds.Set(id, contentItem);
                    }
                }
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

        /// <summary>
        /// Creates a new <see cref="CargoFileDataSource"/>, using the given filename for a file.
        /// </summary>
        /// <param name="filename">The file to use.</param>
        /// <remarks>
        /// If the filename is not rooted (i.e. is a relative path), and this class is being instantiated
        /// in an appdomain in which a System.Web based web application is running, the file will be created
        /// relative to the App_Data folder.
        /// </remarks>
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

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
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

            base.Dispose(disposing);
        }

        private class ContentItemMinimal
        {
            public string content { get; set; }
        }
    }
}
