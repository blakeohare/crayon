const { app } = require('electron');
const { createHub } = require('./messagehubclient');
const renderwindow = require('./renderwindow.js');

app.whenReady().then(() => {

    let rwindow = null;

    const hub = createHub('u3debug');

    hub.addListener('u3init', (msg, cb) => {
        let { title, width, height, initialData } = msg;
        rwindow = renderwindow.createWindow(title, width, height, initialData.length === 0 ? null : initialData);
        rwindow.setListener(winMsg => {
            switch (winMsg.type) {
                case 'E':
                    hub.send('u3event', winMsg);
                    break;
                default:
                    throw new Error("Unknown message type: '" + winMsg.type + "'");
            }
        })
        if (cb) cb(true);
    });

    hub.addListener('u3data', msg => {
        if (rwindow === null) {
            throw new Error("The first message must be u3init");
        }
        let { buffer } = msg;
        rwindow.send(buffer);
    });

    hub.start();
});

