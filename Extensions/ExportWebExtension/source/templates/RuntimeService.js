const createRuntimeService = () => {

    const genRuntime = (() => {
        // @INCLUDE@: vm.js
            
        return {
            runInterpreter,
            runInterpreterWithFunctionPointer,
        };
    })();

    let handleRequest = async req => {
        console.log("RUNTIME SERVICE");
        console.log("Request:");
        console.log(req);
        throw new Error("Not implemented");
    };

    return {
        name: 'runtime',
        handleRequest,
    };
};
