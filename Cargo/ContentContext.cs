using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public class ContentContext
    {
        public string Locality { get; set; }

        public string Locale { get; set; }

        public bool EditingEnabled { get; set; }
        
        public IDictionary<string, object> Properties { get; set; }
    }
}
