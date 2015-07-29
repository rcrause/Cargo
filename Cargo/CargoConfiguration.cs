using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public abstract class CargoConfiguration
    {
        private static readonly Lazy<DefaultCargoConfiguration> _default = new Lazy<DefaultCargoConfiguration>();
        public static CargoConfiguration Default { get { return _default.Value; } }

        public abstract ICargoDataSource DataSource { get; }
    }
}
