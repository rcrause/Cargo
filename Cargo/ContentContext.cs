using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// A basic implementation for <see cref="IContentContext"/>.
    /// </summary>
    public class ContentContext : IContentContext
    {
        /// <summary>
        /// When <c>true</c>, editing is enabled and tokenized content tags will be rendered.
        /// </summary>
        public virtual bool EditingEnabled { get; set; }

        /// <summary>
        /// The locale for the content. Not used at current.
        /// </summary>
        public virtual string Locale { get; set; }

        /// <summary>
        /// The location for the content.
        /// </summary>
        public virtual string Location { get; set; }
    }
}
