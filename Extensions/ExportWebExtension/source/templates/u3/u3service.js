(() => {
    window.registerMessageListener = null;
    window.sendMessage = null;
})();

const createU3Service = (hub) => {
    let waxhub = hub;
    let windowIdAlloc = 1;
    
    let createU3Instance = () => {
        // TODO: these need to be encased in a context object that is passed around.
        let listener = null;
        let windowId = windowIdAlloc++;
        window.registerMessageListener = (cb) => { listener = cb; };
        window.sendMessage = async (msgType, msgPayload) => {
            console.log(msgType, msgPayload);
            throw new Error("Send message not implemetned yet.");
        };
        
        //let uiRoot = document.getElementById('html_render_host');
        let uiRoot = document.getElementById('crayon_host');
        actualU3Initialize(uiRoot);
        return windowId;
    };

    let handleReqImpl = async req => {
        switch (req.type) {
            case 'prepareWindow':
                let winId = createU3Instance();
                return { windowId: winId };

            default:
                console.log(req);
                throw new Error("Not implemented message type: " + req.type);
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
        name: 'u3',
        handleRequest,
    };
};
