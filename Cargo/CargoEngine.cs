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
        {
            _configuration = CargoConfiguration.Default;
            DataSource = _configuration.DataSource;
        }

        public CargoEngine(CargoConfiguration configuration)
        {
            _configuration = configuration;
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
