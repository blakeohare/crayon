(() => {
    window.registerMessageListener = null;
    window.sendMessage = null;
})();

const createU3Service = (hub) => {
    let waxhub = hub;
    
    let handleReqImpl = async req => {
        console.log(req);
        throw new Error("Not implemented");
        // TODO: implement window.sendMessage and registerMessageListener here upon creating a U3 instance.

        /*
       
        let listener = null;
        window.registerMessageListener = (cb) => { listener = cb; };
        
        window.sendMessage = async (msgType, msgPayload) => {

        };
        //*/
    };
    
    let handleRequest = async (req) => {
        try {
            return handleReqImpl(req);
        } catch (e) {
            return { errors: [e] };
        }
    };
    
    return {
        name: 'u3',
        handleRequest,
    };
};
