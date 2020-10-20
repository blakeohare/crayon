const { app } = require('electron');
const namedpipeserver = require('./namedbigpipeserver.js');
const namedpipeclient = require('./namedpipeclient.js');
const renderwindow = require('./renderwindow.js');
const { base64ToText } = require('./util.js');

app.whenReady().then(() => {

    let rwindow = null;

    let handlePipeMessage = data => {
        
        let parts = data.split(' ');

        if (parts[0] === 'INIT') {
            let title = base64ToText(parts[1]);
            let width = parseInt(parts[2]);
            let height = parseInt(parts[3]);
            let initialData = parts.slice(4);
            rwindow = renderwindow.createWindow(title, width, height, initialData.length === 0 ? null : initialData);
        } else if (rwindow === null) {
            throw new Error("The first message must be INIT");
        }

        // even if the window isn't ready yet (as in the case of INIT) the message will be queued
        // and sent in bulk when it is ready.
        rwindow.send(parts);
    };
    
    returnpipe = namedpipeclient.runClient('u3return');

    namedpipeserver.runServer(
        'u3pipe',
        handlePipeMessage,
        () => { console.log("Now listening"); },
        () => { console.log("Pipe closed"); });

    setTimeout(() => {
        returnpipe.send("Hello to Crayon from U3");
    }, 5000)
});

