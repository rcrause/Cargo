using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public class CargoEngine
    {
        private CargoConfiguration _configuration;
        public ICargoDataSource DataSource { get; private set; }

        public CargoEngine()
            :this(CargoConfiguration.Default)
        {
        }

        public CargoEngine(CargoConfiguration configuration)
        {
            _configuration = configuration;
            DataSource = _configuration.DataSource;
        }

        public ContentCollection GetContent(ContentContext contentContext)
        {
            string locality = contentContext.Locality;
            var content = DataSource.GetByLocality(locality);
            var collection = new ContentCollection(content, contentContext);
            return collection;
        }
    }
}
