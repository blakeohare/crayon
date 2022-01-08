const createRuntimeService = (hub) => {
    let waxhub = hub;
    let GEN;

    let C$wax$send = async (eventLoop, _hubIgnored, isListener, serviceId, payloadJson, cbFpValue) => {
        let vm = eventLoop.vm;
        let glo = GEN.vmGetGlobals(vm);
        let req = JSON.parse(payloadJson);
        if (isListener) {
            waxhub.addListener(serviceId, req, (res) => {
                GEN.runInterpreterWithFunctionPointer(vm, cbFpValue, [GEN.buildNull(glo), GEN.buildString(glo, JSON.stringify(res))]);
            });
        } else {
            let response = await waxhub.sendRequest(serviceId, req);
            let responseStr = JSON.stringify(response);
            let strVal = GEN.buildString(glo, responseStr);
            GEN.runInterpreterWithFunctionPointer(vm, cbFpValue, [GEN.buildNull(glo), strVal]);
        }
        queueEvSpin(eventLoop);
        return null;
    };

    let C$common$queueExecContext = (evLoop, execId) => {
        evLoop.queueExecId(execId);
        queueEvSpin(evLoop);
    };

    let getType = (t) => {
        if (typeof t == 'string') return 'S';
        if (!t) return t === false ? 'B' : t === 0 ? 'I' : 'N';
        if (t === true) return 'B';
        if (typeof t == 'number') return (t % 1 == 0) ? 'I' : 'F';
        if (typeof t == 'object') return Array.isArray(t) ? 'L' : 'O';
        return 'null';
    };
    let convertToValue = (g, v) => {
        switch (getType(v)) {
            case 'N': return GEN.buildNull(g);
            case 'B': return GEN.buildBoolean(g, v);
            case 'I': return GEN.buildInteger(g, v);
            case 'F': return GEN.buildFloat(g, v);
            case 'S': return GEN.buildString(g, v);
            case 'L':
                let list = [];
                for (let m of v) list.push(convertToValue(g, m));
                return GEN.buildList(list);
            case 'O':
                let keys = Object.keys(v);
                let values = [];
                for (let k of keys) values.push(convertToValue(g, v[k]));
                return GEN.buildStringDictionary(g, keys, values);
            default: return GEN.buildNull(g);
        }
    };

    let C$common$parseJson = (() => {
        
        return (globals, txt) => {
            try {
                return convertToValue(globals, JSON.parse(txt));
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
            getVmReinvokeDelay,
            getVmResultExecId,
            getVmResultStatus,
            isVmResultRootExecContext,
            startVm,
            runInterpreter,
            runInterpreterWithFunctionPointer,
            vmEnableLibStackTrace,
            vmGetEventLoopObj,
            vmGetGlobals,
            vmGetResourceReaderObj,
            vmSetEventLoopObj,
            vmSetResourceReaderObj,
        };
    })();

    COMMON.setRuntime(GEN);

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
                let globals = GEN.vmGetGlobals(vm);
                args = nativeArgs.map(arg => convertToValue(globals, arg));
            }
            result = GEN.runInterpreterWithFunctionPointer(vm, item.functionPointer, args);
        } else {
            throw new Error(); // unknown condition
        }
        switch (GEN.getVmResultStatus(result)) {
            case 1: // FINISHED
            case 3: // FATAL ERROR
                if (GEN.isVmResultRootExecContext(result)) {
                    evLoop.isRunning = false;
                }
                break;
            case 2: // SUSPENDED
                break;
            case 5: // RESUME
                setTimeout(() => {
                    evLoop.queueExecId(GEN.getVmResultExecId(result));
                    queueEvSpin(evLoop);
                }, Math.max(0, Math.floor(1000 * GEN.getVmReinvokeDelay(result) + .5)));
                break;
            case 7: // BREAKPOINT
                throw new Error("Not implemented");
        }
    };

    let queueEvSpin = evLoop => {
        window.setTimeout(() => evSpin(evLoop), 0);
    };

    let evSpin = async (evLoop) => {
        while (evLoop.items.length > 0 && evLoop.isRunning) {
            runEventLoopIteration(evLoop);
        }
    };

    let runVmEventLoop = (vm, cbxBundle) => {
        let evLoop = null;
        evLoop = {
            vm,
            isRunning: true,
            items: [{ startFromBeginning: true }],
            isDoneCb: null,
            queueExecId: id => {
                evLoop.items.push({ execId: id });
            },
            runVmWithNativeArgs: (fp, args) => {
                evLoop.items.push({
                    functionPointer: fp,
                    functionPointerNativeArgs: [...args],
                });
                queueEvSpin(evLoop);
            },
        };

        return new Promise(res => {
            evLoop.isDoneCb = () => res(1);
            GEN.vmSetEventLoopObj(vm, evLoop);
            let resReader = {
                vm,
                evLoop,
                cbxBundle,
            };
            GEN.vmSetResourceReaderObj(vm, resReader);
            evSpin(evLoop);
        });
    };

    let handleRequest = async (req) => {
        console.log("RUNTIME SERVICE");
        console.log("Request:");
        console.log(req);
        
        let vm = null;
        let cbxBundle = null;
        if (req.cbxPath) {
            cbxBundle = CBX.getBundle(req.cbxPath);
            if (!cbxBundle) return { errors: ["CBX file not found: " + req.cbxPath] };
            
            vm = GEN.createVm(
                await cbxBundle.getByteCode(),
                await cbxBundle.getResourceManifest(),
                await cbxBundle.getImageManifest());
            
        } else {
            throw new Error("Not implemented");
        }
        GEN.vmEnableLibStackTrace(vm);
        
        return runVmEventLoop(vm, cbxBundle).then(() => {
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
