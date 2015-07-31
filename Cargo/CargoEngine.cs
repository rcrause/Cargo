using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public class CargoEngine
    {
        public CargoConfiguration Configuration { get; set; }

        private ICargoDataSource DataSource { get { return Configuration.DataSource; } }

        public CargoEngine()
            :this(CargoConfiguration.Default)
        {
        }

        public CargoEngine(CargoConfiguration configuration)
        {
            Configuration = configuration;
        }

        public ContentCollection GetContent(IContentContext contentContext)
        {
            //get content for this locality
            var content = DataSource.GetByLocality(contentContext.Locality);

            //create an immutable content collection for use by the view
            var collection = new ContentCollection(content, contentContext);
            
            return collection;
        }
    }
}
