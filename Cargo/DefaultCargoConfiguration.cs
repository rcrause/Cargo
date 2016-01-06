using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// The default configuration for Cargo. Uses the default <see cref="CargoFileDataSource"/> as a data source.
    /// </summary>
    public class DefaultCargoConfiguration : CargoConfiguration
    {
        private Lazy<CargoFileDataSource> _cargoFileDataSource = new Lazy<CargoFileDataSource>();

        public override ICargoDataSource DataSource { get { return _cargoFileDataSource.Value; } }
    }
}
