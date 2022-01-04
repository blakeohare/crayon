const createWaxHub = () => {
    let servicesById = {};

    let registerService = service => {
        servicesById[service.name] = service;
    };

    let sendRequest = (serviceId, req) => {
        let service = servicessById[serviceId];
        if (!service) {
            throw new Error(serviceId + " does not exist.");
            // TODO: extensions
        }
        return service.handleRequest(req);
    };

    return {
        registerService,
        sendRequest,
    };
};
