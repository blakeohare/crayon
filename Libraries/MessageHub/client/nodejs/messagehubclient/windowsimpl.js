const namedbigpipeserver = require('./namedbigpipeserver.js');
const namedpipeclient = require('./namedpipeclient.js');

let messageIdAlloc = 0;

const createHub = (token, isDebugMode) => {

    let hub = {};
    let client = null;

    const onUpstreamReady = (client, onReady) => {
        console.log("Upstream client created.");
        client.send('DOWNSTREAM_READY');
        onReady(null);
    };

    const onDownstreamReady = (token, onReady) => {
        console.log("I got this far.");

        client = namedpipeclient.runClient(
            'msghub_' + token + '_us',
            () => { onUpstreamReady(client, onReady) },
            () => {
                console.log("Client closed, I guess");
            });
    };

    const listenersByType = {};

    hub.addListener = (type, f) => {
        listenersByType[type] = f;
        return hub;
    };

    let callbacksById = {};

    let handleDownstreamData = rawString => {
        let data = JSON.parse(rawString);
        let isResponse = data['r'] === 1;
        let payload = data['p'];
        let id = data['i'];
        if (isResponse) {
            let callback = callbacksById[id];
            if (callback) {
                delete callbacksById[id];
                callback(payload);
            }
        } else {
            let callbackWanted = data['c'] === 1;
            let type = data['y'];
            let responseCallback = null;
            if (callbackWanted) {
                responseCallback = (responseObj) => { sendResponse(id, responseObj); };
            } else {
                responseCallback = (responseObj) => {};
            }
            let handler = listenersByType[type];
            if (handler) {
                handler(payload, responseCallback);
            }
        }
    };

    const sendResponse = (responseId, responseObj) => {
        let msg = {
            i: responseId,
            p: responseObj,
            r: 1,
        };
        sendRawString(JSON.stringify(msg));
    };

    hub.start = () => {
        return new Promise((res) => {
            namedbigpipeserver.runServer(
                'msghub_' + token + '_ds',
                data => {
                    console.log("Received downstream data:", data);
                    handleDownstreamData(data);
                },
                () => {
                    console.log("Pipe ready.");
                    onDownstreamReady(token, res);
                },
                () => {
                    console.log("Pipe closed");
                    if (spinnerId !== null) {
                        clearInterval(spinnerId);
                    }
                });
        });
    };

    let sendRawString = rawString => {
        client.send(rawString);
    };

    hub.send = (type, obj, optionalCb) => {
        let callbackWanted = !!optionalCb;
        let id = ++messageIdAlloc;
        let msg = {
            i: id,
            y: type,
            p: obj,
            c: callbackWanted ? 1 : 0,
        };

        if (callbackWanted) {
            callbacksById[id] = optionalCb;
        }

        sendRawString(JSON.stringify(msg));
    };

    hub.sendPromise = (type, obj) => {
        return new Promise(res => {
            hub.send(type, obj, result => { res(result); });
        });
    };

    return hub;
};


module.exports = {
    createHub,
};
