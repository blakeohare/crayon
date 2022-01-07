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

    let C$common$queueExecContext = (evLoop, execId) => {
        evLoop.queueExecId(execId);
    };

    let C$common$parseJson = (() => {
        let getType = (t) => {
            if (typeof t == "string") return 'S';
            if (!t) return t === false ? 'B' : t === 0 ? 'I' : 'N';
            if (t === true) return 'B';
            if (typeof t == "number") return (t % 1 == 0) ? 'I' : 'F';
            if (typeof t == 'object') return Array.isArray(t) ? 'L' : 'O';
            return 'null';
        };
        let convert = (g, v) => {
            switch (getType(v)) {
                case 'N': return GEN.buildNull(g);
                case 'B': return GEN.buildBoolean(g, v);
                case 'I': return GEN.buildInteger(g, v);
                case 'F': return GEN.buildFloat(g, v);
                case 'S': return GEN.buildString(g, v);
                case 'L':
                    let list = [];
                    for (let m of v) list.push(convert(g, m));
                    return GEN.buildList(list);
                case 'O':
                    let keys = Object.keys(v);
                    let values = [];
                    for (let k of keys) values.push(convert(g, v[k]));
                    return GEN.buildStringDictionary(g, keys, values);
                default: return GEN.buildNull(g);
            }
        };
        
        return (globals, txt) => {
            try {
                return convert(globals, JSON.parse(txt));
            } catch (e) {
                return null;
            }
        };
    })();
        
    GEN = (() => {
        // @INCLUDE@: vm.js
            
        return {
            buildBoolean,
            buildFloat,
            buildInteger,
            buildList,
            buildNull,
            buildString,
            buildStringDictionary,
            createVm,
            startVm,
            runInterpreter,
            runInterpreterWithFunctionPointer,
            vmEnableLibStackTrace,
            vmGetGlobals,
            vmSetEventLoopObj,
        };
    })();

    let runEventLoopIteration = (evLoop) => {
        if (!evLoop.isRunning || evLoop.items.length === 0) return;
        let item = evLoop.items[0];
        evLoop.items = evLoop.items.slice(1);

        let vm = evLoop.vm;
        let result;
        if (item.startFromBeginning) {
            result = GEN.startVm(vm);
        } else if (item.execId !== undefined) {
            result = GEN.runInterpreter(vm, item.execId);
        } else if (item.functionPointer) {
            let args = item.functionPointerArgs;
            if (!args) {
                let nativeArgs = item.functionPointerNativeArgs;
                throw new Error("TODO: native arg function pointer invocation in VM event loop");
            }
            result = GEN.runInterpreterWithFunctionPointer(vm, item.functionPointer, args);
        } else {
            throw new Error(); // unknown condition
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

    let evSpin = async (evLoop) => {
        while (evLoop.items.length > 0 && evLoop.isRunning) {
            runEventLoopIteration(evLoop);
        }
    };

    let runVmEventLoop = (vm) => {
        let evLoop = null;
        evLoop = {
            vm,
            isRunning: true,
            items: [{ startFromBeginning: true }],
            isDoneCb: null,
            queueExecId: id => {
                evLoop.items.push({ execId: id });
            },
        };

        return new Promise(res => {
            evLoop.isDoneCb = () => res(1);
            GEN.vmSetEventLoopObj(vm, evLoop);
            evSpin(evLoop);
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
