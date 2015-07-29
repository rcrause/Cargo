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
        public static void UseCargo(this IAppBuilder app, CargoEngine cargoEngine)
        {
            app.Use(UseCargoImpl(cargoEngine));
        }

        private static Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>> UseCargoImpl(CargoEngine cargoEngine)
        {
            NancyOptions options = new NancyOptions();
            options.Bootstrapper = new CargoNancyBootstrapper(cargoEngine);
            var nancyProc = NancyMiddleware.UseNancy(options);

            return next =>
            {
                var nancy = nancyProc(next);

                return environment =>
                {
                    if (IsCargoPath(environment))
                    {
                        return nancy(environment);
                    }
                    else
                    {
                        return next(environment);
                    }
                };
            };
        }

        private static bool IsCargoPath(IDictionary<string, object> environment)
        {
            object requestPathObject;
            string requestPath;

            if (environment.TryGetValue("owin.RequestPath", out requestPathObject))
            {
                requestPath = requestPathObject as string;
                if (requestPath != null)
                {
                    return requestPath.StartsWith("/__cargo");
                }
            }

            return false;
        }
    }
}
