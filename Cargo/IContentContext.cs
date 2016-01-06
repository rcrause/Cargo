using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// The context for content in Cargo. Content is managed on a pre-context basis.
    /// Typically each view/locale pair will have it's own content context.
    /// </summary>
    public interface IContentContext
    {
        /// <summary>
        /// The location of the content context. Typically this is the view file name
        /// </summary>
        string Location { get; }

        /// <summary>
        /// The current locale.
        /// </summary>
        string Locale { get; }

        /// <summary>
        /// Returns <c>true</c> if an administrator is logged in and able to edit content.
        /// </summary>
        bool EditingEnabled { get; }
    }
}
