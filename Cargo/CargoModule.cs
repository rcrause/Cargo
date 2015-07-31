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
            //all routes start with this
            string prefix = cargoEngine.Configuration.CargoRoutePrefix;
            if (prefix != "/" && prefix.EndsWith("/")) prefix = prefix.Substring(0, prefix.Length - 1);

            //get some things from resource
            _cargoJs = GetResourceAsString("Cargo.cargo.js");
            _cargoCss = GetResourceAsString("Cargo.cargo.css");


            //here are our handlers

            Get[prefix + "/js"] = _ => _cargoJs;
            Get[prefix + "/css"] = _ => _cargoCss;
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
