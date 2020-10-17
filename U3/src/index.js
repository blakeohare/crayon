const { app } = require('electron');
const namedpipeserver = require('./namedpipeserver.js');
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
            rwindow = renderwindow.createWindow(title, width, height);
        } else if (rwindow === null) {
            throw new Error("The first message must be INIT");
        }

        // even if the window isn't ready yet (as in the case of INIT) the message will be queued
        // and sent in bulk when it is ready.
        rwindow.send(parts);
    };
    
    namedpipeserver.runServer(
        'u3pipe',
        handlePipeMessage,
        () => { console.log("Now listening"); },
        () => { console.log("Pipe closed"); });
});

