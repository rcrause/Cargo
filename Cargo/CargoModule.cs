using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public class CargoModule : Nancy.NancyModule
    {
        string _cargoJs;
        string _cargoCss;

        public CargoModule(CargoEngine cargoEngine)
        {
            _cargoJs = GetResourceAsString("Cargo.cargo.js");
            _cargoCss = GetResourceAsString("Cargo.cargo.css");

            Get["/__cargo/"] = _ =>
            {
                return "Hello World";
            };

            Get["/__cargo/js"] = _ => { return _cargoJs; };
            Get["/__cargo/css"] = _ => { return _cargoCss; };
        }

        string GetResourceAsString(string resourceName)
        {
            var assembly = typeof(CargoModule).Assembly;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var tr = new StreamReader(stream))
                {
                    return tr.ReadToEnd();
                }
            }
        }
    }
}
