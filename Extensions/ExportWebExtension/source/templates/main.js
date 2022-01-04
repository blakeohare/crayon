const crayonToolchainRun = (() => {

    // @INCLUDE@: argParser.js
    // @INCLUDE@: RouterService.js
    // @INCLUDE@: common.js
    // @INCLUDE@: WaxHub.js

    let hub = createWaxHub();
    hub.registerService(createRouterServicce());
    hub.registerService(createRuntimeService());

    let useU3 = false;
    // @INCLUDE@: USE-U3-OVERRIDE
    if (useU3) hub.registerService(createU3Service());

    let useBuilder = false; // TODO: one of these days...
    // @INCLUDE@: USE-BUILD-OVERRIDE
    if (useBuilder) {
        hub.registerService(createBuilderService());
        hub.registerService(createAssemblyResolverService());
    }

    return args => {
        let cmd = argParse(args);
        console.log(cmd);
        hub.sendRequest('router', cmd)
    };

})();
