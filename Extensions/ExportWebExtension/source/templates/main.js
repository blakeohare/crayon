const Wax = (() => {

    // @INCLUDE@: argParser.js
    // @INCLUDE@: RouterService.js
    // @INCLUDE@: WaxHub.js

    let hub = createWaxHub();
    hub.registerService(createRouterServicce(hub));
    hub.registerService(createRuntimeService(hub));

    let useU3 = false;
    // @INCLUDE@: USE-U3-OVERRIDE
    if (useU3) hub.registerService(createU3Service(hub));

    let useBuilder = false; // TODO: one of these days...
    // @INCLUDE@: USE-BUILD-OVERRIDE
    if (useBuilder) {
        hub.registerService(createBuilderService(hub));
        hub.registerService(createAssemblyResolverService(hub));
    }

    return {
        toolchainRun: args => {
            let cmd = argParse(args);
            console.log(cmd);
            hub.sendRequest('router', cmd)
        },
    };

})();
