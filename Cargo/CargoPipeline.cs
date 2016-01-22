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
        private Func<CargoEngine> _getCargoEngine;
        private Func<IDictionary<string, object>, Task> _next;
        private ResourceHelper _resourceHelper = new ResourceHelper();
        private System.Security.Cryptography.SHA1Managed _sha = new System.Security.Cryptography.SHA1Managed();
        private Lazy<DateTime> _assemblyLastModifiedDate = new Lazy<DateTime>(() => new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime);
        private JsonSerializer jsonSerializer = new JsonSerializer();
        

        public CargoPipeline(Func<IDictionary<string, object>, Task> next, CargoEngine cargoEngine)
            : this(next, () => cargoEngine)
        {
        }

        public CargoPipeline(Func<IDictionary<string, object>, Task> next, Func<CargoEngine> deferredGetCargoEngine)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            if (deferredGetCargoEngine == null) throw new ArgumentNullException(nameof(deferredGetCargoEngine));

            _next = next;
            _getCargoEngine = deferredGetCargoEngine;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var path = Get<string>(environment, "owin.RequestPath");

            //get the cargo engine safely
            CargoEngine cargoEngine = null;
            try { cargoEngine = _getCargoEngine(); } catch { }

            if (cargoEngine != null)
            {
                string cargoRoutePrefix = null;
                CargoConfiguration config = null;

                //get configuration safely
                try
                {
                    config = cargoEngine.Configuration;
                    cargoRoutePrefix = config?.CargoRoutePrefix;
                }
                catch { }

                //if the route prefix matches handle the request
                if (!string.IsNullOrEmpty(cargoRoutePrefix) &&
                    path.StartsWith(cargoRoutePrefix))
                {
                    bool authorized = cargoEngine.AuthenticateRequest(environment);
                    string strippedPath = path.Substring(cargoRoutePrefix.Length);

                    var method = Get<string>(environment, "owin.RequestMethod");
                    var scheme = Get<string>(environment, "owin.RequestScheme");
                    var headers = Get<IDictionary<string, string[]>>(environment, "owin.RequestHeaders");
                    var pathBase = Get<string>(environment, "owin.RequestPathBase");
                    var queryString = Get<string>(environment, "owin.RequestQueryString");
                    var body = Get<Stream>(environment, "owin.RequestBody");
                    var protocol = Get<string>(environment, "owin.RequestProtocol");
                    var cancellationToken = Get<CancellationToken>(environment, "owin.CallCancelled");

                    switch (method)
                    {
                        case "HEAD":
                        case "GET":
                            return HandleGetAsync(cargoEngine, environment, method == "HEAD", strippedPath, cancellationToken);
                        case "POST":
                            return HandlePostAsync(cargoEngine, environment, strippedPath, cancellationToken);
                        default:
                            return Return405Async(environment);
                    }
                }
            }

            //if we get here the request remained unhandled
            return _next(environment);
        }

        private async Task HandleGetAsync(CargoEngine cargoEngine, IDictionary<string, object> environment, bool onlyHead, string strippedPath, CancellationToken cancellationToken)
        {
            if (strippedPath == "/js")
            {
                await WriteFromResource(environment, "cargo.js", "application/json", cancellationToken, TimeSpan.FromDays(10));
            }
            else if (strippedPath == "/css")
            {
                await WriteFromResource(environment, "cargo.css", "text/css", cancellationToken, TimeSpan.FromDays(10));
            }
            else if (strippedPath == "/export")
            {
                using (var ds = cargoEngine.CreateDataSource())
                {
                    await WriteObject(environment, ds.GetAllContent(), cancellationToken);
                }
            }
            else
            {
                await Return404Async(environment);
            }
        }

        private async Task HandlePostAsync(CargoEngine cargoEngine, IDictionary<string, object> environment, string strippedPath, CancellationToken cancellationToken)
        {
            if (strippedPath == "/save")
            {
                await PerformSave(cargoEngine, environment, cancellationToken);
            }
            else if (strippedPath == "/import")
            {
                await PerformImport(cargoEngine, environment, cancellationToken);
            }
            else
            {
                await Return404Async(environment);
            }
        }

        private async Task PerformImport(CargoEngine cargoEngine, IDictionary<string, object> environment, CancellationToken cancellationToken)
        {
            var request = ReadObjectFromRequest<List<ContentItem>>(environment);
            using (var ds = cargoEngine.CreateDataSource())
            {
                ds.Set(request);
                await WriteObject(environment, new { message = "ok" }, cancellationToken);
            }
        }

        private async Task PerformSave(CargoEngine cargoEngine, IDictionary<string, object> environment, CancellationToken cancellationToken)
        {
            int itemsWritten = 0;

            var request = ReadJsonFromRequest(environment) as JObject;
            if (request != null)
            {
                var items = request.Properties()
                    .Select(x => new { id = x.Name, val = ((x.Value as JObject)?.Property("content")?.Value as JValue)?.Value as string })
                    .Where(x => x.id != null && x.val != null)
                    .ToDictionary(x => x.id, x => x.val);

                if (items.Count > 0)
                {
                    using (var ds = cargoEngine.CreateDataSource())
                    {
                        ds.SetById(items);
                        itemsWritten = items.Count;
                    }
                }
            }

            await WriteObject(environment, new { message = $"saved {itemsWritten} items" }, cancellationToken);
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

        private T ReadObjectFromRequest<T>(IDictionary<string, object> environment)
        {
            var body = Get<Stream>(environment, "owin.RequestBody");

            using (StreamReader sr = new StreamReader(body))
            {
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    return jsonSerializer.Deserialize<T>(jr);
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
