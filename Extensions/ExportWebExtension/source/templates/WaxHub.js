const createWaxHub = () => {
    let servicesById = {};

    let registerService = service => {
        servicesById[service.name] = service;
    };

    let sendRequest = (serviceId, req) => {
        let service = servicesById[serviceId];
        if (!service) {
            throw new Error(serviceId + " does not exist.");
            // TODO: extensions
        }
        return Promise.resolve(service.handleRequest(req));
    };

    return {
        registerService,
        sendRequest,
    };
};
