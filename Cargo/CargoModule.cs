using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public class CargoModule : Nancy.NancyModule
    {
        public CargoModule(CargoEngine cargoEngine)
        {
            Get["/__cargo/"] = _ =>
            {
                return "Hello World";
            };
        }
    }
}
