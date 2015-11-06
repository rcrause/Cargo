using Nancy.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public static class CargoExtensions
    {
        public static IAppBuilder UseCargo(this IAppBuilder app, CargoEngine cargoEngine)
        {
            return app.Use(typeof(CargoPipeline), cargoEngine);
        }
    }
}
