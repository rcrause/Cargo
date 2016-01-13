using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cargo
{
    /// <summary>
    /// The main Cargo nancy module. Contains the serverside code for the admin interface.
    /// </summary>
    public class CargoModule : Nancy.NancyModule
    {
        private static ResourceHelper _resourceHelper = new ResourceHelper();
        private static System.Security.Cryptography.SHA1Managed _sha = new System.Security.Cryptography.SHA1Managed();
        private static Lazy<DateTime> _assemblyLastModifiedDate = new Lazy<DateTime>(() => new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime);

        public CargoModule(CargoEngine cargoEngine)
        {
            //all routes start with this
            string prefix = cargoEngine.Configuration.CargoRoutePrefix;
            if (prefix.EndsWith("/")) prefix = prefix.Substring(0, prefix.Length - 1);


            Get[prefix + @"/js"] = r =>
            {
                string loaderjs = FileFromResource("cargo-loader.js");
                var lmd = _assemblyLastModifiedDate.Value.ToString("yyyyMMddhhmmss");
                loaderjs = loaderjs.Replace("cargo.js", $"js/{lmd}");
                loaderjs = loaderjs.Replace("cargo.css", $"css/{lmd}");
                return Content(loaderjs, "text/javascript");
            };
            
            Get[prefix + @"/js/{hash}"] = r =>
            {
                string cargojs = FileFromResource("cargo.js");
                return Content(cargojs, "text/javascript", TimeSpan.FromDays(10));
            };

            Get[prefix + @"/css/{hash}"] = r =>
            {
                string cargocss = FileFromResource("cargo.css");
                return Content(cargocss, "text/css", TimeSpan.FromDays(10));
            };

            Post[prefix + @"/save"] = r =>
            {
                JToken request = ReadJsonFromRequest();
                return new { message = "ok" };
            };
        }

        private static Response NotFound()
        {
            return new NotFoundResponse();
        }

        private static Response Nothing()
        {
            return new Response();
        }

        private JToken ReadJsonFromRequest()
        {
            using (StreamReader sr = new StreamReader(Request.Body))
            {
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    return JToken.ReadFrom(jr);
                }
            }
        }

        private static Response Content(string content, string contentType, TimeSpan? cacheDuration = null)
        {
            return new Response
            {
                Contents = s =>
                {
                    StreamWriter sw = new StreamWriter(s, Encoding.UTF8);
                    sw.Write(content);
                    sw.Flush();
                    s.Flush();
                    sw.Close();
                    s.Close();
                },
                ContentType = contentType,
                Headers = new Dictionary<string, string>
                {
                    ["Cache-Control"] = cacheDuration.HasValue ? $"public, max-age={cacheDuration.Value.TotalSeconds}" : "",
                    ["Last-Modified"] = _assemblyLastModifiedDate.Value.ToString("R")
                },
                ReasonPhrase = "OK",
                StatusCode = HttpStatusCode.OK
            };
        }

        private static string FileFromResource(string resource)
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
