const createWaxHub = () => {
    let servicesById = {};

    let registerService = service => {
        servicesById[service.name] = service;
    };

    let getService = serviceId => {
        let service = servicesById[serviceId];
        if (!service) {
            throw new Error(serviceId + " does not exist.");
            // TODO: extensions
        }
        return service;
    };

    let sendRequest = (serviceId, req) => {
        return Promise.resolve(getService(serviceId).handleRequest(req));
    };

    let addListener = (serviceId, req, cb) => {
        let service = getService(serviceId);
        if (!service.addListener) throw new Error(serviceId + " does not support listeners");
        service.addListener(req, cb);
    };

    return {
        registerService,
        sendRequest,
        addListener,
    };
};
