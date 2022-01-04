const createRouterServicce = () => {

    let handleReqImpl = req => {
        // TODO: build phase

        // Get bundle phase
        console.log(req);
        throw new Error("Not implemented");

        // Extension phase

        // Run phase
    };

    let handleRequest = async req => {
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
