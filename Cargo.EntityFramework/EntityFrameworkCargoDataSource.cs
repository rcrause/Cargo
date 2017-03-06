using Nito.AsyncEx;
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

        private const string REDIS_ALL_LOCATION_KEY = "all:loc";

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

        #region Cache Helper functions

        //Get and setting single key in cache
        private CacheItem<ContentItem> GetCacheContentItem(string location, string key)
        {
            return AsyncContext.Run(() => _cache.TryItemRestoreAsync<ContentItem>(GetId(location, key)));
        }

        private CacheItem<ContentItem> GetCacheContentItem(string id)
        {
            return AsyncContext.Run(() => _cache.TryItemRestoreAsync<ContentItem>(id));
        }

        private void SetCacheContentItem(ContentItem item)
        {
            AsyncContext.Run(() => _cache.ItemSaveAsync(item.Id, item, TimeSpan.MaxValue));
        }
        
        private void RemoveCacheContentItem(ContentItem item)
        {
            AsyncContext.Run(() => _cache.ItemDeleteAsync(item.Id));
        }

        //Get and set cache for a location
        private CacheItem<ICollection<ContentItem>> GetCacheLocationItem(string location)
        {
            string redisKey = $"loc:{location}";
            return AsyncContext.Run(() => _cache.TryItemRestoreAsync<ICollection<ContentItem>>(redisKey));
        }

        private void SetCacheLocationItem(string location, List<ContentItem> locationItems)
        {
            string redisKey = $"loc:{location}";
            AsyncContext.Run(() => _cache.ItemSaveAsync(redisKey, locationItems, TimeSpan.MaxValue));
        }

        private void RemoveCacheLocationItem(string location)
        {
            string redisKey = $"loc:{location}";
            AsyncContext.Run(() => _cache.ItemDeleteAsync(redisKey));
        }


        //Get and set locations list
        private CacheItem<ICollection<string>> GetCacheAllLocations()
        {
            string redisKey = REDIS_ALL_LOCATION_KEY;
            return AsyncContext.Run(() => _cache.TryItemRestoreAsync<ICollection<string>>(redisKey));
        }

        private void SetCacheAllLocations(List<string> locations)
        {
            string redisKey = REDIS_ALL_LOCATION_KEY;
            AsyncContext.Run(() => _cache.ItemSaveAsync(redisKey, locations, TimeSpan.MaxValue));
        }

        private void RemoveCacheAllLocations()
        {
            string redisKey = REDIS_ALL_LOCATION_KEY;
            AsyncContext.Run(() => _cache.ItemDeleteAsync(redisKey));
        }


        #endregion

        /// <inheritdoc />
        public override ContentItem Get(string location, string key)
        {
            ValidateLocation(location);
            ValidateKey(key);

            CacheItem<ContentItem> r = GetCacheContentItem(location, key);
            if (r.Success)
            {
                return r.Item;
            }
            else
            {
                ContentItem contentItem = AddIds(ContentItems.Find(location, key));
                SetCacheContentItem(contentItem);

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

            CacheItem<ICollection<ContentItem>> r = GetCacheLocationItem(location);
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
                SetCacheLocationItem(location, locationItems);

                return locationItems;
            }
        }

        /// <inheritdoc />
        public override ICollection<string> GetAllLocations()
        {
            CacheItem<ICollection<string>> r = GetCacheAllLocations();
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

                SetCacheAllLocations(locationItems);

                return locationItems;
            }
        }

        /// <inheritdoc />
        public override ContentItem GetById(string id)
        {
            ValidateId(id);

            CacheItem<ContentItem> r = GetCacheContentItem(id);
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
                SetCacheContentItem(contentItem);

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
                    RemoveCacheContentItem(item);
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
            CacheItem<ContentItem> r = GetCacheContentItem(location, key);
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
                
                SetCacheContentItem(item);
                _dataContext.SaveChanges();
            }

            if(item.OriginalContent != defaultContent)
            {
                item.OriginalContent = defaultContent;

                SetCacheContentItem(item);
                _dataContext.SaveChanges();
            }

            return item;
        }

        /// <inheritdoc />
        public override void Set(IEnumerable<ContentItem> contentItems)
        {
            RemoveCacheAllLocations();

            bool anyset = false;
            foreach(var item in contentItems)
            {
                ValidateKey(item.Key);
                ValidateLocation(item.Location);

                //All cache in the particular location will be invalid as well as the item
                RemoveCacheLocationItem(item.Location); 
                RemoveCacheContentItem(item);


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

                    SetCacheContentItem(contentItem);
                    anyset = true;
                }
                else
                {
                    if (contentItem.Content != item.Content)
                    {
                        contentItem.Content = item.Content;
                        contentItem.OriginalContent = item.OriginalContent;

                        SetCacheContentItem(contentItem);
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
            RemoveCacheAllLocations();

            foreach (var item in idContentPairs)
            {
                ValidateId(item.Key);

                string id = item.Key;
                string location, key;
                ParseId(id, out location, out key);
                ValidateLocation(location);
                ValidateKey(key);

                //All cache in the particular location will be invalid
                RemoveCacheLocationItem(location);
                

                var contentItem = AddIds(ContentItems.Find(location, key));
                if (contentItem != null)
                {
                    contentItem.Content = item.Value;
                    SetCacheContentItem(contentItem);
                }
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
                CacheItem<ContentItem> r = GetCacheContentItem(location, key);
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
