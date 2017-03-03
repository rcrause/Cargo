using Nito.AsyncEx;
using Stores;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// Allows using an Entity Framework data context as a data source for Cargo. The
    /// relevant <see cref="DbContext"/> instance needs to have the needed entities mapped
    /// by calling <see cref="CargoEntityFrameworkExtensions.MapCargoContent(DbModelBuilder, string, string)"/>.
    /// </summary>
    public class EntityFrameworkCargoDataSource : CargoDataSourceBase
    {
        private string _rxId = @"^(.*)\/(.+)";
        private DbContext _dataContext;
        private bool _ownsContext;
        private IStore _cache;

        private DbSet<ContentItem> ContentItems { get { return _dataContext.Set<ContentItem>(); } }

        /// <summary>
        /// Create a new <see cref="EntityFrameworkCargoDataSource"/> given an instance of <see cref="DbContext"/>. 
        /// </summary>
        /// <param name="dataContext">
        /// The <see cref="DbContext"/> to use. The <see cref="DbContext"/>
        /// must have been prepared by calling <see cref="CargoEntityFrameworkExtensions.MapCargoContent(DbModelBuilder, string, string)"/>.
        /// </param>
        /// <param name="cache">Provide a store as a caching mechanism to prevent strain on the DB</param>
        /// <param name="ownsContext">If set to <c>true</c> the context will be disposed when this instance is disposed.</param>
        public EntityFrameworkCargoDataSource(DbContext dataContext, IStore cache, bool ownsContext = true)
        {
            _dataContext = dataContext;
            _ownsContext = ownsContext;
            _cache = cache;
        }

        /// <inheritdoc />
        public override ContentItem Get(string location, string key)
        {
            ValidateLocation(location);
            ValidateKey(key);

            CacheItem<ContentItem> r = AsyncContext.Run(() => _cache.TryItemRestoreAsync<ContentItem>(GetId(location, key)));

            if (r.Success)
            {
                return r.Item;
            }
            else
            {
                ContentItem contentItem = AddIds(ContentItems.Find(location, key));
                AsyncContext.Run(() => _cache.ItemSaveAsync(contentItem.Id, contentItem, TimeSpan.MaxValue));

                return contentItem;
            }
        }

        /// <inheritdoc />
        public override ICollection<ContentItem> GetAllContent()
        {
            return ContentItems
                .Select(AddIds)
                .ToList();
        }

        /// <inheritdoc />
        public override ICollection<ContentItem> GetAllContentForLocation(string location)
        {
            ValidateLocation(location);
            string redisKey = $"loc:{location}";

            CacheItem<ICollection<ContentItem>> r = AsyncContext.Run(() => _cache.TryItemRestoreAsync<ICollection<ContentItem>>(redisKey));
            if (r.Success)
            {
                return r.Item;
            }
            else
            {
                var locationItems = ContentItems
                                    .Select(AddIds)
                                    .Where(x => x.Location == location && x.OriginalContent != null)
                                    .ToList();

                AsyncContext.Run(() => _cache.ItemSaveAsync(redisKey, locationItems, TimeSpan.MaxValue));

                return locationItems;
            }
        }

        /// <inheritdoc />
        public override ICollection<string> GetAllLocations()
        {
            string redisKey = $"all:loc";

            CacheItem<ICollection<string>> r = AsyncContext.Run(() => _cache.TryItemRestoreAsync<ICollection<string>>(redisKey));
            if (r.Success)
            {
                return r.Item;
            }
            else
            {
                var locationItems = ContentItems
                                    .Select(x => x.Location)
                                    .Distinct()
                                    .ToList();

                AsyncContext.Run(() => _cache.ItemSaveAsync(redisKey, locationItems, TimeSpan.MaxValue));

                return locationItems;
            }
        }

        /// <inheritdoc />
        public override ContentItem GetById(string id)
        {
            ValidateId(id);

            CacheItem<ContentItem> r = AsyncContext.Run(() => _cache.TryItemRestoreAsync<ContentItem>(id));

            if (r.Success)
            {
                return r.Item;
            }
            else
            {
                string location, key;
                ParseId(id, out location, out key);
                ValidateLocation(location);
                ValidateKey(key);
                
                ContentItem contentItem = AddIds(ContentItems.Find(location, key));
                AsyncContext.Run(() => _cache.ItemSaveAsync(contentItem.Id, contentItem, TimeSpan.MaxValue));

                return contentItem;
            }
        }

        /// <inheritdoc />
        public override void Remove(IEnumerable<string> contentItemIds)
        {
            foreach(var item in GetMultipleById(contentItemIds))
            {
                if(item != null)
                {
                    AsyncContext.Run(() => _cache.ItemDeleteAsync(item.Id));
                    ContentItems.Remove(item);
                }
            }

            _dataContext.SaveChanges();
        }

        /// <inheritdoc />
        public override ContentItem GetOrCreate(string location, string key, string defaultContent)
        {
            ContentItem item = null;

            ValidateKey(key);
            ValidateLocation(location);
            defaultContent = defaultContent ?? "";

            //Try to find in cache
            CacheItem<ContentItem> r = AsyncContext.Run(() => _cache.TryItemRestoreAsync<ContentItem>(GetId(location, key)));
            if (r.Success)
            {
                item = r.Item;
            }

            //If not in cache try DB
            if (item == null)
            {
                item = ContentItems.Find(location, key);
            }
            
            //If item does not exist add it
            if(item == null)
            {
                item = ContentItems.Add(new ContentItem
                {
                    Content = defaultContent,
                    Key = key,
                    Location = location,
                    Id = GetId(location, key),
                    OriginalContent = defaultContent
                });

                AsyncContext.Run(() => _cache.ItemSaveAsync(item.Id, item, TimeSpan.MaxValue));
                _dataContext.SaveChanges();
            }

            if(item.OriginalContent != defaultContent)
            {
                item.OriginalContent = defaultContent;
                AsyncContext.Run(() => _cache.ItemSaveAsync(item.Id, item, TimeSpan.MaxValue));
                _dataContext.SaveChanges();
            }

            return item;
        }

        /// <inheritdoc />
        public override void Set(IEnumerable<ContentItem> contentItems)
        {
            string redisAllKey = $"all:loc";
            AsyncContext.Run(() => _cache.ItemDeleteAsync(redisAllKey));

            bool anyset = false;
            foreach(var item in contentItems)
            {
                ValidateKey(item.Key);
                ValidateLocation(item.Location);

                string redisKey = $"loc:{item.Location}";
                AsyncContext.Run(() => _cache.ItemDeleteAsync(redisKey));


                var contentItem = ContentItems.Find(item.Location, item.Key);
                if (contentItem == null)
                {
                    contentItem = ContentItems.Add(new ContentItem
                    {
                        Content = item.Content,
                        Key = item.Key,
                        Location = item.Location,
                        Id = GetId(item.Location, item.Key),
                        OriginalContent = item.OriginalContent
                    });

                    AsyncContext.Run(() => _cache.ItemSaveAsync(contentItem.Id, contentItem, TimeSpan.MaxValue));
                    anyset = true;
                }
                else
                {
                    if (contentItem.Content != item.Content)
                    {
                        contentItem.Content = item.Content;
                        contentItem.OriginalContent = item.OriginalContent;

                        AsyncContext.Run(() => _cache.ItemSaveAsync(contentItem.Id, contentItem, TimeSpan.MaxValue));
                        anyset = true;
                    }
                }
            }

            if(anyset)
            {
                _dataContext.SaveChanges();
            }
        }

        /// <inheritdoc />
        public override void SetById(IEnumerable<KeyValuePair<string, string>> idContentPairs)
        {
            List<string> editedNonExistingLocations = new List<string>();
            List<string> editedExistingLocations = new List<string>();

            foreach (var item in idContentPairs)
            {
                ValidateId(item.Key);

                string id = item.Key;
                string location, key;
                ParseId(id, out location, out key);
                ValidateLocation(location);
                ValidateKey(key);

                //Delete the location if edited and existing
                string redisKey = $"loc:{location}";
                if (AsyncContext.Run(() => _cache.TryItemRestoreAsync<ICollection<ContentItem>>(redisKey)).Success)
                {
                    editedExistingLocations.Add(location);
                }
                else
                {
                    editedNonExistingLocations.Add(location);
                }

                var contentItem = AddIds(ContentItems.Find(location, key));
                if (contentItem != null)
                {
                    contentItem.Content = item.Value;
                    AsyncContext.Run(() => _cache.ItemSaveAsync(contentItem.Id, contentItem, TimeSpan.MaxValue));
                }
            }

            //Delete All Locations if a location was added
            if (editedNonExistingLocations.Count > 0)
            {
                string redisKey = $"all:loc";
                AsyncContext.Run(() => _cache.ItemDeleteAsync(redisKey));
            }

            //Delete locations edited. These caches no longer valid
            foreach (var location in editedExistingLocations)
            {
                string redisKey = $"loc:{location}";
                AsyncContext.Run(() => _cache.ItemDeleteAsync(redisKey));
            }

            _dataContext.SaveChanges();
        }

        private static ContentItem AddIds(ContentItem ci)
        {
            if (ci != null) ci.Id = GetId(ci.Location, ci.Key);

            return ci;
        }

        private IEnumerable<ContentItem> GetMultipleById(IEnumerable<string> contentItemIds)
        {
            foreach (var id in contentItemIds)
            {
                ValidateId(id);

                string location, key;
                ParseId(id, out location, out key);
                ValidateLocation(location);
                ValidateKey(key);

                ContentItem contentItem = null;

                //Try to find in cache otherwise DB
                CacheItem<ContentItem> r = AsyncContext.Run(() => _cache.TryItemRestoreAsync<ContentItem>(GetId(location, key)));
                if (r.Success)
                {
                    contentItem = r.Item;
                }
                else
                {
                    contentItem = ContentItems.Find(location, key);
                }

                if (contentItem != null) yield return contentItem;
            }
        }

        private static string GetId(string location, string key)
        {
            return $"{location}/{key}";
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

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(_dataContext != null && _ownsContext)
                {
                    _dataContext.Dispose();
                    _dataContext = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
