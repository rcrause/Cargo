using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// The main Cargo class, providing main cargo functionality. Pass it a <see cref="CargoConfiguration"/> in the
    /// constructor or set <see cref="CargoEngine.Configuration"/> to configure.
    /// </summary>
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
            var content = DataSource.GetContentForLocality(contentContext.Locality);

            //create an immutable content collection for use by the view
            var collection = new ContentCollection(content, contentContext);
            
            return collection;
        }
    }
}
