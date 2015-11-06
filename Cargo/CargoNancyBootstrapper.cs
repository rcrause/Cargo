using Nancy;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Conventions;

namespace Cargo
{
    class CargoNancyBootstrapper : DefaultNancyBootstrapper
    {
        private CargoEngine _cargoEngine;

        public CargoNancyBootstrapper(CargoEngine cargoEngine)
        {
            _cargoEngine = cargoEngine;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register((c, n) => _cargoEngine);
        }
    }
}
