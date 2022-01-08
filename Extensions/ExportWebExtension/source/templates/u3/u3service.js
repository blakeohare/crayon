(() => {
    window.registerMessageListener = null;
    window.sendMessage = null;
})();

const createU3Service = (hub) => {
    let waxhub = hub;
    let windowIdAlloc = 1;
    
    let instances = {};

    let createU3Instance = () => {

        // TODO: Merge this with the nori_context object.
        // This also needs to incorporate any state created in closures that would be invalid between U3 instances in the same browser window.
        let inst;
        inst = {
            windowId: windowIdAlloc++,
            sendMessage: async (msgType, msgPayload) => {
                switch (msgType) {
                    case 'events':
                        inst.listeners.events({ msgs: msgPayload });
                        break;
                    case 'shown':
                        inst.listeners.loaded({});
                        break;
                    case 'eventBatch':
                        inst.listeners.batch({ data: msgPayload });
                        break;
                    default:
                        throw new Error("Send message not implemetned yet for " + msgType);
                }
                
            },
            registerMessageListener: (cb) => { inst.downstreamListener = cb; },
            uiRoot: document.getElementById('crayon_host'), // document.getElementById('html_render_host'); for other platforms
            listeners: {},
            onCloseCb: null,
            downstreamListener: null,
        };
        
        // TODO: the u3 instance should be created by actualU3Initialize
        window.registerMessageListener = inst.registerMessageListener;
        window.sendMessage = inst.sendMessage;

        instances[inst.windowId] = inst;
        
        actualU3Initialize(inst.uiRoot);
        
        return inst.windowId;
    };

    let createAndShowWindow = async (winInst, title, width, height, initialData, keepAspectRatio) => {
        let closeCause = await new Promise(res => {
            winInst.onCloseCb = res;
            winInst.downstreamListener({ buffer: initialData, options: { keepAspectRatio }});
        });
        return { cause: closeCause };
    };

    let handleReqImpl = async req => {
        switch (req.type) {
            case 'prepareWindow':
                let winId = createU3Instance();
                return { windowId: winId };

            case 'show':
                return createAndShowWindow(instances[req.windowId], req.title, req.width, req.height, req.initialData, req.keepAspectRatio);
            case 'data':
                instances[req.windowId].downstreamListener({ buffer: req.buffer });
                return {};
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

    let addListener = (req, cb) => {
        console.log(req);
        let inst = instances[req.windowId];
        if (!inst) return;
        inst.listeners[req.type] = cb;
    };
    
    return {
        name: 'u3',
        handleRequest,
        addListener,
    };
};
