const Bundler = require('parcel-bundler');

exports.runParcelBundler = (callback, argsJson) => {
    const args = JSON.parse(argsJson);

    const bundler = new Bundler(args.entryPoints, args.options);

    bundler.serve(bundler.options.hmrPort)
        .then(server => callback(null, {
            Port: server.address().port,
            PublicPath: bundler.options.publicURL
        }));
};