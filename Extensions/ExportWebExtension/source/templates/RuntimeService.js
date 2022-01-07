const createRuntimeService = (hub) => {
    let waxhub = hub;
    let GEN;

    let C$wax$send = async (eventLoop, _hubIgnored, isListener, serviceId, payloadJson, cbFpValue) => {
        let vm = eventLoop.vm;
        let glo = GEN.vmGetGlobals(vm);
        if (isListener) {
            throw new Error("TODO: adding a listener");
        } else {
            let response = await waxhub.sendRequest(serviceId, JSON.parse(payloadJson));
            let responseStr = JSON.stringify(response);
            let strVal = GEN.buildString(glo, responseStr);
            GEN.runInterpreterWithFunctionPointer(vm, cbFpValue, [GEN.buildNull(glo), strVal]);
        }
        return null;
    };

    GEN = (() => {
        // @INCLUDE@: vm.js
            
        return {
            buildNull,
            buildString,
            createVm,
            startVm,
            runInterpreter,
            runInterpreterWithFunctionPointer,
            vmEnableLibStackTrace,
            vmGetGlobals,
            vmSetEventLoopObj,
        };
    })();

    let runEventLoopIteration = (evLoop, vm) => {
        if (!evLoop.isRunning) return;
        let node = evLoop.head;
        evLoop.head = node.next;
        let item = node.value;

        let result;
        if (item.startFromBeginning) {
            result = GEN.startVm(vm);
        } else if (item.functionPointer) {
            let args = item.functionPointerArgs;
            if (!args) {
                let nativeArgs = item.functionPointerNativeArgs;
                throw new Error("TODO: native arg function pointer invocation in VM event loop");
            }
            result = GEN.runInterpreterWithFunctionPointer(vm, item.functionPointer, args);
        } else {
            result = GEN.runInterpreter(vm, item.executionContextId);
        }
        switch (result.status) {
            case 1: // FINISHED
                throw new Error("TODO: getter for isRootContext");
            case 2: // SUSPENDED
                break;
            case 3: // FATAL ERROR
                throw new Error("TODO: getter for isRootContext");
            case 5: // RESUME
                throw new Error("TODO: not implemented");
            case 7: // BREAKPOINT
                throw new Error("TODO: not implemented");
        }
    };

    let runVmEventLoop = (vm) => {
        let evLoop = {
            vm,
            isRunning: true,
            head: {
                value: { startFromBeginning: true },
                next: null,
            },
            isDoneCb: null,
        };

        return new Promise(res => {
            evLoop.isDoneCb = () => res(1);
            GEN.vmSetEventLoopObj(vm, evLoop);
            runEventLoopIteration(evLoop, vm);
        });
    };

    let handleRequest = async (req) => {
        console.log("RUNTIME SERVICE");
        console.log("Request:");
        console.log(req);
        
        let vm = null;
        if (req.cbxPath) {
            let cbxBundle = CBX.getBundle(req.cbxPath);
            if (!cbxBundle) return { errors: ["CBX file not found: " + req.cbxPath] };
            
            vm = GEN.createVm(
                await cbxBundle.getByteCode(),
                await cbxBundle.getResourceManifest(),
                await cbxBundle.getImageManifest());
            
        } else {
            throw new Error("Not implemented");
        }
        GEN.vmEnableLibStackTrace(vm);
        
        return runVmEventLoop(vm).then(() => {
            let res = GEN.vmGetWaxResponse(vm);
            if (res === null) res = '{}';
            return JSON.parse(res);
        });
    };

    return {
        name: 'runtime',
        handleRequest,
    };
};
