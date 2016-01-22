using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// Provides a an abstract base for <see cref="ICargoDataSource"/>.
    /// </summary>
    public abstract class CargoDataSourceBase : ICargoDataSource
    {
        /// <inheritdoc />
        public abstract ContentItem GetById(string id);
        /// <inheritdoc />
        public abstract ContentItem Get(string location, string key);
        /// <inheritdoc />
        public abstract ICollection<ContentItem> GetAllContent();
        /// <inheritdoc />
        public abstract ICollection<ContentItem> GetAllContentForLocation(string location);
        /// <inheritdoc />
        public abstract ICollection<string> GetAllLocations();
        /// <inheritdoc />
        public abstract ContentItem GetOrCreate(string location, string key, string defaultContent);
        /// <inheritdoc />
        public abstract void Set(IEnumerable<ContentItem> contentItems);
        /// <inheritdoc />
        public abstract void SetById(IEnumerable<KeyValuePair<string, string>> idContentPairs);
        /// <inheritdoc />
        public abstract void Remove(IEnumerable<string> contentItemIds);


        /// <summary>
        /// Validates an id based on certain criteria. An id must be less
        /// than 200 characters, not null or empty, and not contain ~ or ` characters. This 
        /// method will throw if the id is invalid.
        /// </summary>
        /// <param name="id">The id to validate</param>
        protected virtual void ValidateId(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(id)) throw new ArgumentException($"{nameof(id)} cannot be empty", nameof(id));
            if (id.Length > 200) throw new ArgumentException($"{nameof(id)} must be less than 200 characters long", nameof(id));
            if (id.Contains('~')) throw new ArgumentException($"{nameof(id)} cannot contain a ~ character", nameof(id));
            if (id.Contains('`')) throw new ArgumentException($"{nameof(id)} cannot contain a ` character", nameof(id));
        }

        /// <summary>
        /// Validates an key based on certain criteria. An key must be less
        /// than 200 characters, not null or empty, and not contain ~ or ` or / or \ characters. This 
        /// method will throw if the key is invalid.
        /// </summary>
        /// <param name="key">The key to validate</param>
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

        /// <summary>
        /// Validates an location based on certain criteria. An location must be less
        /// than 200 characters, not null or empty, and not contain ~ or ` characters. This 
        /// method will throw if the location is invalid.
        /// </summary>
        /// <param name="location">The location to validate</param>
        protected virtual void ValidateLocation(string location)
        {
            if (location == null) throw new ArgumentNullException(nameof(location));
            if (string.IsNullOrEmpty(location)) throw new ArgumentException($"{nameof(location)} cannot be empty", nameof(location));
            if (location.Length > 200) throw new ArgumentException($"{nameof(location)} must be less than 200 characters long", nameof(location));
            if (location.Contains('~')) throw new ArgumentException($"{nameof(location)} cannot contain a ~ character", nameof(location));
            if (location.Contains('`')) throw new ArgumentException($"{nameof(location)} cannot contain a ` character", nameof(location));
        }

        /// <summary>
        /// Dispose this instance. The <paramref name="disposing"/> parameter specifies whether or not
        /// this method is being called from a disposing method or a finalizer.
        /// </summary>
        /// <param name="disposing"><c>true</c> when called from <see cref="Dispose()"/> and <c>false</c> when called from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

    }
}
