using Cargo.Newtonsoft;
using Cargo.Newtonsoft.Linq;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// The pipeline for use with OWIN type architecture
    /// </summary>
    internal class CargoPipeline
    {
        private CargoEngine _cargoEngine;
        private Func<IDictionary<string, object>, Task> _next;
        private string _cargoRoutePrefix;
        private ResourceHelper _resourceHelper = new ResourceHelper();
        private System.Security.Cryptography.SHA1Managed _sha = new System.Security.Cryptography.SHA1Managed();
        private Lazy<DateTime> _assemblyLastModifiedDate = new Lazy<DateTime>(() => new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime);
        private JsonSerializer jsonSerializer = new JsonSerializer();

        public CargoPipeline(Func<IDictionary<string, object>, Task> next, CargoEngine cargoEngine)
        {
            _cargoEngine = cargoEngine;
            _next = next;
            
            var configuration = cargoEngine.Configuration;
            _cargoRoutePrefix = configuration.CargoRoutePrefix;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var path = Get<string>(environment, "owin.RequestPath");

            if (path.StartsWith(_cargoRoutePrefix))
            {
                string strippedPath = path.Substring(_cargoRoutePrefix.Length);

                var method = Get<string>(environment, "owin.RequestMethod");
                var scheme = Get<string>(environment, "owin.RequestScheme");
                var headers = Get<IDictionary<string, string[]>>(environment, "owin.RequestHeaders");
                var pathBase = Get<string>(environment, "owin.RequestPathBase");
                var queryString = Get<string>(environment, "owin.RequestQueryString");
                var body = Get<Stream>(environment, "owin.RequestBody");
                var protocol = Get<string>(environment, "owin.RequestProtocol");
                var cancellationToken = Get<CancellationToken>(environment, "owin.CallCancelled");

                switch(method)
                {
                    case "HEAD":
                    case "GET":
                        await HandleGetAsync(environment, method == "HEAD", strippedPath, cancellationToken);
                        break;
                    case "POST":
                        await HandlePostAsync(environment, strippedPath, cancellationToken);
                        break;
                    default:
                        await Return405Async(environment);
                        break;
                }
            }
            else
            {
                await _next(environment);
            }
        }

        private async Task HandleGetAsync(IDictionary<string, object> environment, bool onlyHead, string strippedPath, CancellationToken cancellationToken)
        {
            if(strippedPath.StartsWith("/js"))
            {
                await WriteFromResource(environment, "cargo.js", "application/json", cancellationToken, TimeSpan.FromDays(10));
            }
            else if(strippedPath.StartsWith("/css"))
            {
                await WriteFromResource(environment, "cargo.css", "text/css", cancellationToken, TimeSpan.FromDays(10));
            }
            else
            {
                await Return404Async(environment);
            }
        }

        private async Task HandlePostAsync(IDictionary<string, object> environment, string strippedPath, CancellationToken cancellationToken)
        {
            if(strippedPath == "/save")
            {
                var request = ReadJsonFromRequest(environment);
                await WriteObject(environment, new { message = "ok" }, cancellationToken);
            }
            else
            {
                await Return404Async(environment);
            }
        }

        private Task Return405Async(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = 405;
            environment["owin.ResponseReasonPhrase"] = "Bad Method";

            return Task.CompletedTask;
        }

        private Task Return404Async(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = 404;
            environment["owin.ResponseReasonPhrase"] = "Not Found";

            return Task.CompletedTask;
        }

        private JToken ReadJsonFromRequest(IDictionary<string, object> environment)
        {
            var body = Get<Stream>(environment, "owin.RequestBody");

            using (StreamReader sr = new StreamReader(body))
            {
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    return JToken.ReadFrom(jr);
                }
            }
        }

        private async Task WriteObject<T>(IDictionary<string, object> environment, T write, CancellationToken cancellationToken)
        {
            environment["owin.ResponseStatusCode"] = 200;
            environment["owin.ResponseReasonPhrase"] = "Bad Method";
            var owinResponseHeaders = Get<IDictionary<string, string[]>>(environment, "owin.ResponseHeaders");
            var owinResponseBody = Get<Stream>(environment, "owin.ResponseBody");
            owinResponseHeaders["Content-Type"] = new[] { "application/json; charset=utf-8" };
            byte[] toWrite;

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    using (JsonWriter js = new JsonTextWriter(sw))
                    {
                        jsonSerializer.Serialize(js, write);
                    }
                }

                toWrite = ms.ToArray();
            }

                owinResponseHeaders["Content-Length"] = new[] { toWrite.Length.ToString() };

            await owinResponseBody.WriteAsync(toWrite, 0, toWrite.Length, cancellationToken);
        }

        private async Task WriteFromResource(IDictionary<string, object> environment, string resource, string contentType, CancellationToken cancellationToken, TimeSpan? cacheDuration = null)
        {
            string contents = _resourceHelper[resource];

#if DEBUG
            var asm = typeof(ResourceHelper).Assembly;
            var codeBase = asm.CodeBase.Replace("file:///", "").Replace('/', '\\');
            int placeOfDir = codeBase.IndexOf("\\Cargo\\");
            if (placeOfDir > 0)
            {
                string file = codeBase.Substring(0, placeOfDir) + "\\Cargo\\Cargo\\" + resource;
                if (File.Exists(file))
                    contents = File.ReadAllText(file);
            }
#endif
            byte[] responseData = Encoding.UTF8.GetBytes(contents);
            if (!contentType.EndsWith("charset=utf-8")) contentType += "; charset=utf-8";

            environment["owin.ResponseStatusCode"] = 200;
            environment["owin.ResponseReasonPhrase"] = "Bad Method";
            var owinResponseHeaders = Get<IDictionary<string, string[]>>(environment, "owin.ResponseHeaders");
            var owinResponseBody = Get<Stream>(environment, "owin.ResponseBody");
            owinResponseHeaders["Content-Type"] = new[] { contentType };
            owinResponseHeaders["Content-Length"] = new[] { responseData.Length.ToString() };
            owinResponseHeaders["Last-Modified"] = new[] { _assemblyLastModifiedDate.Value.ToString("R") };

#if !DEBUG
            if(cacheDuration.HasValue)
            {
                owinResponseHeaders["Cache-Control"] = new[] { $"public, max-age={cacheDuration.Value.TotalSeconds}" };
            }
#endif

            await owinResponseBody.WriteAsync(responseData, 0, responseData.Length, cancellationToken);
        }

        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) && value is T ? (T)value : default(T);
        }
    }
}
