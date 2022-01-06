const createRouterServicce = (hub) => {
    let waxhub = hub;

    let handleReqImpl = async req => {
        // TODO: build phase

        // Get bundle phase
        console.log(req);
        let cbxFile = null;
        if (req.cbxFile) {
            cbxFile = req.cbxFile;
        }

        console.log(cbxFile);

        // Extension phase
        // TODO: this

        // Run phase
        if (cbxFile !== null) {
            let runtimeReq = {
                realTimePrint: true,
                args: [],
                showLibStack: true,
                useOutputPrefixes: false,
                cbxPath: cbxFile,
            };

            let response = await waxhub.sendRequest('runtime', runtimeReq);
            console.log("RUNTIME COMPLETED:");
            console.log(response);
        }
    };

    let handleRequest = async (req) => {
        try {
            return handleReqImpl(req);
        } catch (e) {
            return { errors: [e] };
        }
    };

    return {
        name: 'router',
        handleRequest,
    };
};
