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

        public CargoEngine()
            :this(CargoConfiguration.Default)
        {
        }

        public CargoEngine(CargoConfiguration configuration)
        {
            Configuration = configuration;
        }

        public ContentView GetContent(IContentContext contentContext)
        {
            //get the data source
            var dataSource = Configuration.GetDataSource();

            //get content for this location
            var content = dataSource.GetAllContentForLocation(contentContext.Location);

            //create an immutable content collection for use by the view
            var collection = new ContentView(content, contentContext, dataSource);

            return collection;
        }
    }
}
