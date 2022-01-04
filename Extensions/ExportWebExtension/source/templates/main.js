const main = () => {

    // @INCLUDE@: argParser.js

    let runtime = (() => {
        // @INCLUDE@: vm.js

        return {
            runInterpreter,
            runInterpreterWithFunctionPointer,
        };
    })();
    
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

    let args = [];
    // @INCLUDE@: ARGS-OVERRIDE
    let cmd = argParse(args);

    hub.sendRequest('router', cmd)
};
