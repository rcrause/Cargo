using Nancy.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// A pipeline for use with OWIN type architecture. Loads the Cargo Nancy server side admin interface
    /// </summary>
    internal class CargoPipeline
    {
        private CargoEngine _cargoEngine;
        private Func<IDictionary<string, object>, Task> _next;
        private Func<IDictionary<string, object>, Task> _nancy;
        private string _cargoRoutePrefix;

        public CargoPipeline(Func<IDictionary<string, object>, Task> next, CargoEngine cargoEngine)
        {
            _cargoEngine = cargoEngine;
            _next = next;

            NancyOptions options = new NancyOptions();
            options.Bootstrapper = new CargoNancyBootstrapper(cargoEngine);
            var nancyProc = NancyMiddleware.UseNancy(options);
            var configuration = cargoEngine.Configuration;
            _cargoRoutePrefix = configuration.CargoRoutePrefix;

            _nancy = nancyProc(next);
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            if (IsCargoPath(environment))
            {
                return _nancy(environment);
            }
            else
            {
                return _next(environment);
            }
        }

        private bool IsCargoPath(IDictionary<string, object> environment)
        {
            object requestPathObject;
            string requestPath;

            if (environment.TryGetValue("owin.RequestPath", out requestPathObject))
            {
                requestPath = requestPathObject as string;
                if (requestPath != null)
                {
                    return requestPath.StartsWith(_cargoRoutePrefix, StringComparison.InvariantCultureIgnoreCase);
                }
            }

            return false;
        }

        /*
        private static void UseCargo(IAppBuilder app, CargoEngine cargoEngine)
        {
            app.Use(UseCargoImpl(cargoEngine));
        }

        private static Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>> UseCargoImpl(CargoEngine cargoEngine)
        {
            NancyOptions options = new NancyOptions();
            options.Bootstrapper = new CargoNancyBootstrapper(cargoEngine);
            var nancyProc = NancyMiddleware.UseNancy(options);
            var configuration = cargoEngine.Configuration;
            var cargoRoutePrefix = configuration.CargoRoutePrefix;

            return next =>
            {
                var nancy = nancyProc(next);

                return environment =>
                {
                    if (IsCargoPath(environment, cargoRoutePrefix))
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
        */
    }
}
