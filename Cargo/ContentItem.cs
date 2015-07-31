using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// An item of content.
    /// </summary>
    [Serializable]
    public class ContentItem
    {
        /// <summary>
        /// The identifier for this <see cref="ContentItem"/>. May be provided by the datasource and should be unique for
        /// each <see cref="ContentItem"/>, even those with the same <see cref="Key"/>.
        /// </summary>
        public virtual object Id { get; set; }

        /// <summary>
        /// The locality (i.e. page) of this content item.
        /// </summary>
        public virtual string Locality { get; set; }

        /// <summary>
        /// The key for this data item. The key identifies each individual <see cref="ContentItem"/>, but there may be multiple content items
        /// with the same key differentiated by the <see cref="Condition"/>. The <see cref="ContentItem"/> chosen will be the one with the most specific matching
        /// <see cref="Condition"/>.
        /// </summary>
        public virtual string Key { get; set; }

        /// <summary>
        /// The <see cref="Condition"/> on which this <see cref="ContentItem"/> will be selected. The <see cref="ContentItem"/> chosen will be the
        /// one with the most specific matching <see cref="Condition"/>.
        /// </summary>
        public virtual string Condition { get; set; }

        /// <summary>
        /// The type of the content
        /// </summary>
        public virtual ContentType ContentType { get; set; }

        /// <summary>
        /// The content for this <see cref="ContentItem"/>.
        /// </summary>
        public virtual string Content { get; set; }
    }
}
