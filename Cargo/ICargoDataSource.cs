using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// Provides persistence for content items. Optionally also provides notification of items modified in the underlying data source.
    /// </summary>
    public interface ICargoDataSource : IDisposable
    {
        /// <summary>
        /// Returns the <see cref="ContentItem"/> with a matching <see cref="ContentItem.Id"/>, or <c>null</c> if a match is not found.
        /// </summary>
        /// <param name="id">the value of the <see cref="ContentItem.Id"/> for the <see cref="ContentItem"/> to be returned.</param>
        ContentItem GetById(string id);

        /// <summary>
        /// Returns the <see cref="ContentItem"/> with a matching <see cref="ContentItem.Location"/> and <see cref="ContentItem.Key"/>, or <c>null</c> if a match is not found.
        /// </summary>
        /// <param name="id">the value of the <see cref="ContentItem.Id"/> for the <see cref="ContentItem"/> to be returned.</param>
        ContentItem Get(string location, string key);

        /// <summary>
        /// Creates a new content item
        /// </summary>
        /// <param name="location">The location for the <see cref="ContentItem"/>.</param>
        /// <param name="key">The key of the <see cref="ContentItem"/>.</param>
        /// <param name="defaultContent">The default content for the <see cref="ContentItem"/>.</param>
        ContentItem GetOrCreate(string location, string key, string defaultContent);

        /// <summary>
        /// Returns all <see cref="ContentItem"/>s for a location (i.e. page), or an empty <see cref="ICollection{ContentItem}"/> if
        /// none exist.
        /// </summary>
        /// <param name="location">The name of the location for which to return <see cref="ContentItem"/>s. If this is null it will
        /// return global content (content not bound to any specific location.
        /// </param>
        ICollection<ContentItem> GetAllContentForLocation(string location);

        /// <summary>
        /// Returns all <see cref="ContentItem"/>s in the data source.
        /// </summary>
        ICollection<ContentItem> GetAllContent();

        /// <summary>
        /// Returns all the various locations in the data source. The method may return locations that don't have any content items.
        /// </summary>
        ICollection<string> GetAllLocations();

        /// <summary>
        /// Adds or updates <see cref="ContentItem"/>s in the underlying data source based on <see cref="ContentItem.Key"/>
        /// and <see cref="ContentItem.Location"/>.
        /// </summary>
        /// <param name="contentItems">The <see cref="ContentItem"/>s to update or add in the database.</param>
        void Set(IEnumerable<ContentItem> contentItems);

        /// <summary>
        /// Adds or updates <see cref="ContentItem"/>s in the underlying data source, using id/content pairs. The
        /// content items must already exist or this method will throw.
        /// </summary>
        /// <param name="contentItems">The <see cref="ContentItem"/>s to update or add in the database.</param>
        void SetById(IEnumerable<KeyValuePair<string, string>> idContentPairs);

        /// <summary>
        /// Removes <see cref="ContentItem"/>s from the underlying data source.
        /// </summary>
        /// <param name="contentItemIds">The Id's <see cref="ContentItem"/>s to remove from the database.</param>
        void Remove(IEnumerable<string> contentItemIds);
    }
}
