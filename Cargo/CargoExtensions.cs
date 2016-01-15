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
        public static IAppBuilder UseCargo(this IAppBuilder app, CargoEngine cargoEngine)
        {
            return app.Use(typeof(CargoPipeline), cargoEngine);
        }

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
