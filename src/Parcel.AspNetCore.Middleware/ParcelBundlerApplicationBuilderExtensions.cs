using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parcel.AspNetCore.Middleware;
using System;
using System.IO;
using System.Linq;

namespace Microsoft.AspNetCore.Builder
{
    public static class ParcelBundlerApplicationBuilderExtensions
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public static IApplicationBuilder UseParcelBundler(this IApplicationBuilder app, ParcelBundlerOptions options)
        {
            var environment = app.ApplicationServices.GetService<IWebHostEnvironment>();

            var nodeServiceOptions = new NodeServicesOptions(app.ApplicationServices)
            {
                WatchFileExtensions = new string[] { }
            };

            string content = LoadScriptContent();
            var script = new StringAsTempFile(content, nodeServiceOptions.ApplicationStoppingToken);

            var node = NodeServicesFactory.CreateNodeServices(nodeServiceOptions);

            var args = new
            {
                entryPoints = options.EntryPoints,
                options = new
                {
                    outDir = Path.Combine(environment.WebRootPath, options.OutDir ?? ""),
                    outFile = options.OutFile,
                    publicUrl = options.PublicUrl,
                    watch = options.Watch,
                    cache = options.Cache,
                    cacheDir = options.CacheDir,
                    contentHash = options.ContentHash,
                    minify = options.Minify,
                    scopeHoist = options.ScopeHoist,
                    target = options.Target?.ToString().ToLower(),
                    https = options.Https?.ToParcelOptions(),
                    hmr = options.HotModuleReload?.IsEnabled,
                    hmrPort = options.HotModuleReload?.Port,
                    sourcemaps = options.SourceMaps,
                    hmrHostname = options.HotModuleReload?.Hostname,
                    detailedReport = options.DetailedReport
                }
            };

            var response = node.InvokeExportAsync<ParcelServerInfo>(script.FileName, "runParcelBundler", JsonConvert.SerializeObject(args, SerializerSettings))
                .Result;

            return app;
        }

        private static string LoadScriptContent()
        {
            var assembly = typeof(ParcelServerInfo).Assembly;
            var scriptResource = assembly.GetManifestResourceNames()
                .Where(name => name.EndsWith("parcel-bundler-middleware.js"))
                .Single();

            using (var stream = assembly.GetManifestResourceStream(scriptResource))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}