using System;
using System.Linq;
using System.Collections.Generic;

namespace Parcel.AspNetCore.Middleware
{
    public enum ParcelTarget
    {
        Browser,
        Node,
        Electron
    }

    public enum ParcelLogLevel
    {
        Errors = 1,
        WarningsAndErrors,
        Everything
    }

    public class ParcelHttpsOptions
    {
        private ParcelHttpsOptions() 
        {

        }

        public static readonly ParcelHttpsOptions Disabled = new ParcelHttpsOptions
        {
            IsEnabled = false,
            CertFile = null,
            KeyFile = null
        };
        
        public static readonly ParcelHttpsOptions Generated = new ParcelHttpsOptions
        {
            IsEnabled = true,
            CertFile = null,
            KeyFile = null
        };

        public static ParcelHttpsOptions Custom(string certFile, string keyFile) => new ParcelHttpsOptions
        {
            IsEnabled = true,
            CertFile = null,
            KeyFile = null
        };

        internal bool IsEnabled { get; private set; }
        internal string CertFile { get; private set; }
        internal string KeyFile { get; private set; }

        internal object ToParcelOptions()
        {
            if (!IsEnabled)
                return false;

            if (string.IsNullOrEmpty(CertFile) || string.IsNullOrEmpty(KeyFile))
                return true;

            return new 
            {
                certFile = CertFile,
                keyFile = KeyFile
            };
        }
    }

    public class HotModuleReloadOptions
    {
        private HotModuleReloadOptions()
        {

        }

        public static readonly HotModuleReloadOptions Disabled = new HotModuleReloadOptions
        {
            IsEnabled = false
        };

        public static HotModuleReloadOptions Enabled(short port = 0, string hostname = "") => new HotModuleReloadOptions
        {
            IsEnabled = true,
            Port = port,
            Hostname = hostname
        };

        internal bool IsEnabled { get; set; }
        internal int Port { get; set; }
        internal string Hostname { get; set; }
    }

    public class ParcelBundlerOptions
    {
        public ParcelBundlerOptions(string entryPoint, params string[] entryPoints)
            : this(entryPoints.AsEnumerable().Append(entryPoint))
        {
            
        }

        public ParcelBundlerOptions(IEnumerable<string> entryPoints)
        {
            EntryPoints = entryPoints
                .ToList();
        }

        public IEnumerable<string> EntryPoints { get; set; }
        public ParcelTarget? Target { get; set; }

        public string OutDir { get; set; }
        public string OutFile { get; set; }

        public bool? Watch { get; set; }
        public string PublicUrl { get; set; }
        public HotModuleReloadOptions HotModuleReload { get; set; }
        public ParcelHttpsOptions Https { get; set; }

        public bool? Cache { get; set; }
        public string CacheDir { get; set; }
        
        public bool? ContentHash { get; set; }
        public bool? Minify { get; set; }
        public bool? SourceMaps { get; set; }
        public bool? ScopeHoist { get; set; }

        public ParcelLogLevel? LogLevel { get; set; }
        public bool? DetailedReport { get; set; }
    }
}