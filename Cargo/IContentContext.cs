using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public interface IContentContext
    {
        string Locality { get; }

        string Locale { get; }

        bool EditingEnabled { get; }
    }
}
