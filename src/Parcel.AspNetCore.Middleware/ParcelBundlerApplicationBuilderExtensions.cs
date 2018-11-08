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
            var environment = app.ApplicationServices.GetService<IHostingEnvironment>();

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

            if (response?.PublicPath == null)
            {
                return app;
            }

            return app.UseProxyToLocalParcelBunderServer(response.PublicPath, response.Port, TimeSpan.FromSeconds(100));
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

        private static IApplicationBuilder UseProxyToLocalParcelBunderServer(this IApplicationBuilder appBuilder, string publicPath, int proxyToPort, TimeSpan requestTimeout)
        {
            // Note that this is hardcoded to make requests to "localhost" regardless of the hostname of the
            // server as far as the client is concerned. This is because ConditionalProxyMiddlewareOptions is
            // the one making the internal HTTP requests, and it's going to be to some port on this machine
            // because aspnet-webpack hosts the dev server there. We can't use the hostname that the client
            // sees, because that could be anything (e.g., some upstream load balancer) and we might not be
            // able to make outbound requests to it from here.
            // Also note that the webpack HMR service always uses HTTP, even if your app server uses HTTPS,
            // because the HMR service has no need for HTTPS (the client doesn't see it directly - all traffic
            // to it is proxied), and the HMR service couldn't use HTTPS anyway (in general it wouldn't have
            // the necessary certificate).
            var proxyOptions = new ConditionalProxyMiddlewareOptions(
                "http", "localhost", proxyToPort.ToString(), requestTimeout);
                
            return appBuilder.UseMiddleware<ConditionalProxyMiddleware>(publicPath, proxyOptions);
        }
    }
}