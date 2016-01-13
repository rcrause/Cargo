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

        public override ICargoDataSource GetDataSource()
        {
            return _cargoFileDataSource.Value;
        }

        public override bool AuthenticateRequest(IDictionary<string, object> environment)
        {
            return true;
        }
    }
}
