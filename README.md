This repository contains a ASP.NET Core middlware for running [ParcelJS](https://parceljs.org) in your development environment.

# Setup

Add the middleware in your `Startup.Configure`-method.

```csharp
if (env.IsDevelopment())
{
    services.UseParcelBundler(new ParcelBundlerOptions("Client/index.html"));
}
```

This will start Parcel in the background with the given file as its entry point. Several entry points can be configured:

```csharp
services.UseParcelBundler(new ParcelBundlerOptions("Client/js/app.js", "Client/css/app.scss"));
```

Content will be built to the web-root of ASP.NET Core (see [`WebHost.UseWebRoot`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-2.1#web-root)), which by default is `wwwroot`.

To build to a specific directory in your web-root you can specify the `OutDir`-option:

```csharp
services.UseParcelBundler(new ParcelBundlerOptions("Client/index.html")
{
    OutDir = "dist"
})
```

This configuration will build the content to `wwwroot/dist`.

The other possible options in `ParcelBundlerOptions` are the same as those provided by the [Parcel Bundler API](https://parceljs.org/api.html#bundler).

# Known issues

## Hot Module Reload and HTTPS

HMR does not work out-of-the-box when hosting your application under HTTPS. To make 
HMR work you will either need to use `HTTP` in ASP.NET Core or configure the middleware 
with some trusted certificate using `ParcelBundlerOptions.Https`.

The reason that HRM does not work is that the HMR-code in Parcel will look at the protocol
used by the browser when accessing your application to determine whether to use a secure
WebSocket when connecting to Parcel's dev-server. If you application is hosted using HTTPS
but Parcel is not, then it will try to connect using the wrong protocol.

**This is not an issue which can be fixed in this repository. It is an issue which needs to be
fixed in Parcel itself.**

Some ideas to make things easier, which I might propose to the maintainers of Parcel:

1) Add support for PFX-files when configuring HTTPS, to make it possible for this package
   to configure Parcel to use the same self-signed certificate used by ASP.NET Core.
2) Make the HMR-code in Parcel rely on the configuration given when starting the server,
   rather than relying on the protocol used by the browser.

