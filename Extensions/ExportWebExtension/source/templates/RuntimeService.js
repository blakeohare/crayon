const createRuntimeService = (hub) => {
    let waxhub = hub;

    const GEN = (() => {
        // @INCLUDE@: vm.js
            
        return {
            createVm,
            startVm,
            runInterpreter,
            runInterpreterWithFunctionPointer,
            vmEnableLibStackTrace,
        };
    })();

    let runEventLoopIteration = (q, vm, endCb) => {
        if (!q.isRunning) return;
        let node = q.head;
        q.head = node.next;
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
        let q = {
            isRunning: true,
            head: {
                value: { startFromBeginning: true },
                next: null,
            }
        };
        return new Promise(res => {
            runEventLoopIteration(q, vm, () => res(1));
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
