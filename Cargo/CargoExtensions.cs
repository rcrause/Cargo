using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// Various extension methods for Cargo.
    /// </summary>
    public static class CargoExtensions
    {
        /// <summary>
        /// Register the cargo API.
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> provided by OWIN.</param>
        /// <param name="cargoEngine">The <see cref="CargoEngine"/> to register the API for.</param>
        public static IAppBuilder UseCargo(this IAppBuilder app, CargoEngine cargoEngine)
        {
            return app.Use(typeof(CargoPipeline), cargoEngine);
        }

        /// <summary>
        /// Register the cargo API for deferred loading.
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> provided by OWIN.</param>
        /// <param name="deferredCargoEngineProc">A function that will provide a <see cref="CargoEngine"/> when requested, or <c>null</c> if it is not available yet.</param>
        public static IAppBuilder UseCargo(this IAppBuilder app, Func<CargoEngine> deferredCargoEngineProc)
        {
            return app.Use(typeof(CargoPipeline), deferredCargoEngineProc);
        }

        public static ICollection<ContentItem> GetGlobalContent(this ICargoDataSource cds)
        {
            return cds.GetAllContentForLocation(null);
        }
    }
}
