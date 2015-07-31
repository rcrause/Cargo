using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public class ContentContext : IContentContext
    {
        public virtual bool EditingEnabled { get; set; }

        public virtual string Locale { get; set; }

        public virtual string Locality { get; set; }
    }
}
