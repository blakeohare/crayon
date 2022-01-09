const { receiveMessage, sendMessage } = (() => {

    COMMON.environment.descriptor = 'android|javascript';
    COMMON.environment.isMobile = true;
    COMMON.environment.fullscreen = true;
    COMMON.environment.isAndroid = true;
    COMMON.environment.aspectRatio = 1.0;
    
    let receiveMessageDecoder = function(s) {
        let output = [];
        let codes = s.split(' ');
        let length = codes.length;
        for (let i = 0; i < length; ++i) {
            output.push(String.fromCharCode(parseInt(codes[i])));
        }
        return output.join('');
    };
    
    let receiveMessage = function(type, payload, useEncoding) {
        if (useEncoding) {
            type = receiveMessageDecoder(type);
            payload = receiveMessageDecoder(payload);
        }
        let parts = payload.split(' ');
        switch (type) {
            // These are no longer used.
            case 'touch-points-status': break;
            case 'touch-event': break;
            case 'screen-ratio': break;
            case 'back-button':
                let backUnhandledCb = () => { sendMessage('unhandled-back-button', ''); };
                if (window.noriNotifyHardwareBackButtonPressed) {
                    window.noriNotifyHardwareBackButtonPressed(backUnhandledCb);
                } else {
                    backUnhandledCb();
                }
                break;
    
            default:
                console.log("Unknown message type: " + type);
                break;
        }
    };
    
    let sendMessage = function(type, msg) {
        JavaScriptBridge.onSendNativeMessage(type, msg);
    };
    
    COMMON.setUrlPath = (_) => { };
    COMMON.getUrlPath = () => null;
    COMMON.launchBrowser = (url, fr, name) => { console.log("TODO: Open URLs (" + url + ")"); };
    
    return { receiveMessage, sendMessage };
})();

sendMessage('ready', '');
