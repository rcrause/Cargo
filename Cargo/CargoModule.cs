using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public class CargoModule : Nancy.NancyModule
    {
        ResourceHelper _resourceHelper = new ResourceHelper();

        public CargoModule(CargoEngine cargoEngine)
        {
            //all routes start with this
            string prefix = cargoEngine.Configuration.CargoRoutePrefix;
            if (prefix != "/" && prefix.EndsWith("/")) prefix = prefix.Substring(0, prefix.Length - 1);
            
            //here are our handlers
            Get[prefix + "/js"] = _ => FileFromResource("cargo.js");
            Get[prefix + "/css"] = _ => FileFromResource("cargo.js");
        }


        private string FileFromResource(string resource)
        {
#if DEBUG
            var asm = typeof(ResourceHelper).Assembly;
            var codeBase = asm.CodeBase.Replace("file:///", "").Replace('/', '\\');
            int placeOfDir = codeBase.IndexOf("\\Cargo\\");
            if (placeOfDir > 0)
            {
                string file = codeBase.Substring(0, placeOfDir) + "\\Cargo\\Cargo\\" + resource;
                if (File.Exists(file))
                    return File.ReadAllText(file);
            }
#endif
            return _resourceHelper[resource];
        }
    }
}
