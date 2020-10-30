const { app } = require('electron');
const { createHub } = require('./messagehubclient');
const renderwindow = require('./renderwindow.js');

app.whenReady().then(() => {

    let rwindow = null;

    const hub = createHub('u3debug');

    hub.addListener('u3init', (msg, cb) => {
        let { title, width, height, initialData } = msg;
        rwindow = renderwindow.createWindow(title, width, height, initialData.length === 0 ? null : initialData);
        rwindow.setListener(winMsgs => {
            hub.send('u3events', winMsgs);
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

