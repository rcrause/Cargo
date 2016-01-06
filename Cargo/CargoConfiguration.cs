using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// Contains the configuration for a <see cref="CargoEngine"/>.
    /// </summary>
    public abstract class CargoConfiguration
    {
        private static readonly Lazy<DefaultCargoConfiguration> _default = new Lazy<DefaultCargoConfiguration>();
        public static CargoConfiguration Default { get { return _default.Value; } }

        public abstract ICargoDataSource DataSource { get; }
        public string CargoRoutePrefix { get; set; } = "/cargo";
    }
}
