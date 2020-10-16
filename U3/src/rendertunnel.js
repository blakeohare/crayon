const { BrowserWindow } = require('electron')
const { ipcMain } = require('electron')

const AUTO_OPEN_DEV_TOOLS = false;

let createTunnelImpl = (title, width, height, type) => {
    if (!isReady) {
        throw new Error("Must wait for ");
    }
    switch (type) {
        case 'UI': throw new Error("Not implemented.");
        case 'GAME': break;
        default: throw new Error("Not implemented.");
    }

    let sendToRenderer;
    let close;

    let win = null;
    function createWindow () {
        win = new BrowserWindow({
            width: width,
            height: height,
            title,
            webPreferences: {
                nodeIntegration: true
            }
        });
    
        close = () => { win.close(); };

        sendToRenderer = (type, value) => {
            if (!win.isDestroyed()) {
                win.webContents.send('rboundmsg', { type, value });
            }
        };

        // and load the index.html of the app.
        win.loadFile('render/index.html').then(() => {
            if (AUTO_OPEN_DEV_TOOLS) {
                win.webContents.openDevTools();
            }

            switch (type) {
                case 'GAME':
                    sendToRenderer('INIT', { type: 'GAME', options: { fps: 60 } });
                    break;
                
                case 'UI':
                    sendToRenderer('INIT', { type: 'UI' });
                    break;
            
                default:
                    throw new Error("Not implemented: '" + type + "'.");
            }
        });
    }
    
    let listener = null;
    let messageQueue = [];
    ipcMain.on('mboundmsg', (event, arg) => {
        if (listener === null) {
            messageQueue.push(arg);
        } else {
            listener(arg);
        }
    })
    
    createWindow();

    return {
        send: sendToRenderer,
        setListener: newListener => { 
            let flushQueue = listener === null;
            listener = newListener;
            if (flushQueue) {
                for (let item of messageQueue) {
                    listener(item);
                }
                messageQueue = [];
            }
        },
        close,
        setTitle: title => {
            win.title = title + '';
        },
        setSize: (width, height) => {
            win.setSize(width, height);
        },
    };
};

let createUiTunnel = (title, width, height) => {
    return createTunnelImpl(title, width, height, 'UI');
};

let createGameTunnel = (title, width, height) => {
    return createTunnelImpl(title, width, height, 'GAME');
};

let isReady = false;

module.exports = {
    markAsReady: () => { isReady = true; },
    createUiTunnel,
    createGameTunnel,
};
