using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public abstract class CargoDataSourceBase : ICargoDataSource
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Returns the <see cref="ContentItem"/> with a matching <see cref="ContentItem.Id"/>, or <c>null</c> if a match is not found.
        /// </summary>
        /// <param name="id">the value of the <see cref="ContentItem.Id"/> for the <see cref="ContentItem"/> to be returned.</param>
        protected abstract ContentItem CreateInternal(string location, string key, string content);
        
        public abstract ContentItem GetById(string id);
        public abstract ContentItem Get(string location, string key);
        public abstract ICollection<ContentItem> GetAllContent();
        public abstract ICollection<ContentItem> GetAllContentForLocation(string location);
        public abstract ICollection<string> GetAllLocations();
        public abstract void Remove(IEnumerable<string> contentItemIds);
        public abstract void Set(IEnumerable<ContentItem> contentItems);

        protected virtual void ValidateId(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(id)) throw new ArgumentException($"{nameof(id)} cannot be empty", nameof(id));
            if (id.Length > 200) throw new ArgumentException($"{nameof(id)} must be less than 200 characters long", nameof(id));
            if (id.Contains('~')) throw new ArgumentException($"{nameof(id)} cannot contain a ~ character", nameof(id));
            if (id.Contains('`')) throw new ArgumentException($"{nameof(id)} cannot contain a ` character", nameof(id));
        }

        protected virtual void ValidateKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException($"{nameof(key)} cannot be empty", nameof(key));
            if (key.Length > 200) throw new ArgumentException($"{nameof(key)} must be less than 200 characters long", nameof(key));
            if (key.Contains('~')) throw new ArgumentException($"{nameof(key)} cannot contain a ~ character", nameof(key));
            if (key.Contains('/')) throw new ArgumentException($"{nameof(key)} cannot contain a / character", nameof(key));
            if (key.Contains('\\')) throw new ArgumentException($"{nameof(key)} cannot contain a \\ character", nameof(key));
            if (key.Contains('`')) throw new ArgumentException($"{nameof(key)} cannot contain a ` character", nameof(key));
        }

        protected virtual void ValidateLocation(string location)
        {
            if (location == null) throw new ArgumentNullException(nameof(location));
            if (string.IsNullOrEmpty(location)) throw new ArgumentException($"{nameof(location)} cannot be empty", nameof(location));
            if (location.Length > 200) throw new ArgumentException($"{nameof(location)} must be less than 200 characters long", nameof(location));
            if (location.Contains('~')) throw new ArgumentException($"{nameof(location)} cannot contain a ~ character", nameof(location));
            if (location.Contains('`')) throw new ArgumentException($"{nameof(location)} cannot contain a ` character", nameof(location));
        }
        
        public ContentItem GetOrCreate(string location, string key, string defaultContent)
        {
            if (defaultContent == null) throw new ArgumentNullException(nameof(defaultContent));

            ValidateLocation(location);
            ValidateKey(key);
            var existing = Get(location, key);
            if (existing != null) return existing;

            var contentItem = CreateInternal(location, key, defaultContent);
            ValidateId(contentItem.Id);

            return contentItem;
        }


        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null) CollectionChanged(sender, e);
        }
    }
}
