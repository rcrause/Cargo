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
    public interface ICargoDataSource : INotifyCollectionChanged
    {
        /// <summary>
        /// Returns the <see cref="ContentItem"/> with a matching <see cref="ContentItem.Id"/>, or <c>null</c> if a match is not found.
        /// </summary>
        /// <param name="id">the value of the <see cref="ContentItem.Id"/> for the <see cref="ContentItem"/> to be returned.</param>
        ContentItem GetById(string id);

        /// <summary>
        /// Returns all <see cref="ContentItem"/>s for a location (i.e. page), or an empty <see cref="IEnumerable{ContentItem}"/> if
        /// none exist.
        /// </summary>
        /// <param name="location">The name of the location for which to return <see cref="ContentItem"/>s.
        /// </param>
        IEnumerable<ContentItem> GetContent(string location);

        /// <summary>
        /// Returns all <see cref="ContentItem"/>s not bound to any location.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ContentItem> GetGlobalContent();

        /// <summary>
        /// Returns all <see cref="ContentItem"/>s with matching <see cref="ContentItem.Key"/>s, or an empty <see cref="IEnumerable{ContentItem}"/> if
        /// none exist.
        /// </summary>
        /// <param name="location">The name of the location for which to return <see cref="ContentItem"/>s.</param>
        /// <param name="key">the value of the <see cref="ContentItem.Key"/> for the <see cref="ContentItem"/>s to be returned.</param>
        IEnumerable<ContentItem> GetByKey(string location, string key);

        /// <summary>
        /// Adds or updates <see cref="ContentItem"/>s in the underlying data source. If the <see cref="ContentItem"/> is new
        /// and is added to the database, it should fill in the <see cref="ContentItem.Id"/> property.
        /// </summary>
        /// <param name="contentItems">The <see cref="ContentItem"/>s to update or add in the database.</param>
        void Set(IEnumerable<ContentItem> contentItems);

        /// <summary>
        /// Removes <see cref="ContentItem"/>s from the underlying data source.
        /// </summary>
        /// <param name="contentItemIds">The Id's <see cref="ContentItem"/>s to remove from the database.</param>
        void Remove(IEnumerable<string> contentItemIds);
    }
}
