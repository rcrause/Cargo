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

        private DbSet<ContentItem> ContentItems { get { return _dataContext.Set<ContentItem>(); } }

        /// <summary>
        /// Create a new <see cref="EntityFrameworkCargoDataSource"/> given an instance of <see cref="DbContext"/>. 
        /// </summary>
        /// <param name="dataContext">
        /// The <see cref="DbContext"/> to use. The <see cref="DbContext"/>
        /// must have been prepared by calling <see cref="CargoEntityFrameworkExtensions.MapCargoContent(DbModelBuilder, string, string)"/>.
        /// </param>
        public EntityFrameworkCargoDataSource(DbContext dataContext, bool ownsContext = true)
        {
            _dataContext = dataContext;
            _ownsContext = ownsContext;
        }

        /// <inheritdoc />
        public override ContentItem Get(string location, string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (location == null) throw new ArgumentNullException(nameof(location));

            ValidateLocation(location);
            ValidateKey(key);

            return AddIds(ContentItems.Find(location, key));
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
            if (location == null) throw new ArgumentNullException(nameof(location));

            ValidateLocation(location);

            return ContentItems
                .Select(AddIds)
                .Where(x => x.Location == location)
                .ToList();
        }

        /// <inheritdoc />
        public override ICollection<string> GetAllLocations()
        {
            return ContentItems
                .Select(x => x.Location)
                .Distinct()
                .ToList();
        }

        /// <inheritdoc />
        public override ContentItem GetById(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            ValidateId(id);

            string location, key;
            ParseId(id, out location, out key);
            ValidateLocation(location);
            ValidateKey(key);

            return AddIds(ContentItems.Find(location, key));
        }

        /// <inheritdoc />
        public override void Remove(IEnumerable<string> contentItemIds)
        {
            foreach(var item in GetMultipleById(contentItemIds))
            {
                if(item != null)
                {
                    ContentItems.Remove(item);
                }
            }

            _dataContext.SaveChanges();
        }

        /// <inheritdoc />
        public override void SetByIdInternal(IEnumerable<KeyValuePair<string, string>> idContentPairs)
        {
            foreach(var item in idContentPairs)
            {
                string id = item.Key;
                string location, key;
                ParseId(id, out location, out key);

                var contentItem = AddIds(ContentItems.Find(location, key));
                if(contentItem != null)
                {
                    contentItem.Content = item.Value;
                }
                else
                {
                    ContentItems.Add(new ContentItem
                    {
                        Id = id,
                        Location = location,
                        Key = key,
                        Content = item.Value
                    });
                }
            }

            _dataContext.SaveChanges();
        }

        /// <inheritdoc />
        public override void SetInternal(IEnumerable<ContentItem> contentItems)
        {
            foreach (var item in contentItems)
            {
                ValidateKey(item.Key);
                ValidateLocation(item.Location);

                string id = GetId(item.Location, item.Key);
                ValidateId(id);

                var contentItem = AddIds(ContentItems.Find(item.Location, item.Key));
                if (contentItem != null)
                {
                    contentItem.Content = item.Content;
                }
                else
                {
                    ContentItems.Add(new ContentItem
                    {
                        Id = id,
                        Location = item.Location,
                        Key = item.Key,
                        Content = item.Content
                    });
                }
            }

            _dataContext.SaveChanges();
        }

        /// <inheritdoc />
        protected override ContentItem CreateInternal(string location, string key, string content)
        {
            ValidateKey(key);
            ValidateLocation(location);

            string id = GetId(location, key);
            ValidateId(id);

            var contentItem = AddIds(ContentItems.Find(location, key));
            if (contentItem != null)
            {
                contentItem.Content = content;
            }
            else
            {
                contentItem = ContentItems.Add(new ContentItem
                {
                    Id = id,
                    Location = location,
                    Key = key,
                    Content = content
                });
            }

            _dataContext.SaveChanges();

            return contentItem;
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
                string location, key;
                ParseId(id, out location, out key);
                ValidateLocation(location);
                ValidateKey(key);

                yield return ContentItems
                    .AsNoTracking()
                    .Select(AddIds)
                    .Single(x => x.Location == location && x.Key == key);
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
