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
    public abstract class CargoEngine
    {
        /// <summary>
        /// The configuration for this <see cref="CargoEngine"/>.
        /// </summary>
        public CargoConfiguration Configuration { get; set; }

        /// <summary>
        /// Creates a new <see cref="CargoEngine"/> with the default <see cref="CargoConfiguration"/>.
        /// </summary>
        public CargoEngine()
            :this(new CargoConfiguration())
        {
        }

        /// <summary>
        /// Creates a new <see cref="CargoEngine"/> with given <see cref="CargoConfiguration"/>.
        /// </summary>
        /// <param name="configuration"></param>
        public CargoEngine(CargoConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Returns a <see cref="ContentView"/> for a given <see cref="IContentContext"/>.
        /// </summary>
        /// <param name="contentContext">The context to get the <see cref="ContentView"/> for.</param>
        public ContentView GetContent(IContentContext contentContext)
        {
            var dataSource = CreateDataSource();

            //get content for this location
            var content = dataSource.GetAllContentForLocation(contentContext.Location);

            //create an immutable content collection for use by the view
            var collection = new ContentView(content, contentContext, dataSource);

            return collection;
        }

        /// <summary>
        /// When overridden in a derived class, will create a data source to use for content management.
        /// </summary>
        /// <returns></returns>
        public abstract ICargoDataSource CreateDataSource();

        /// <summary>
        /// When overridden in a derived class, authenticates a request to the Cargo API.
        /// </summary>
        /// <param name="environment">The request OWIN environment</param>
        public abstract bool AuthenticateRequest(IDictionary<string, object> environment);
    }
}
