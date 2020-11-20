const { BrowserWindow } = require('electron')
const { ipcMain } = require('electron')

const AUTO_OPEN_DEV_TOOLS = false;

let createWindow = (title, width, height, initialData, hideMenu) => {

    let listeners = {};
    let mBoundMessageQueues = {};
    let rBoundMessageQueue = [];
    
    ipcMain.on('mboundmsg', (event, arg) => {
        if (!listeners[arg.type]) {
            let q = mBoundMessageQueues[arg.type] || [];
            mBoundMessageQueues[arg.type] = q;
            q.push(arg.data);
        } else {
            listeners[arg.type](arg.data);
        }
    });
    
    const win = new BrowserWindow({
        width: width,
        height: height,
        title,
        webPreferences: {
            nodeIntegration: true
        }
    });

    if (hideMenu) {
        win.setMenu(null);
    }

    const close = () => { win.close(); };

    const sendToRenderer = data => {
        if (rBoundMessageQueue !== null) {
            rBoundMessageQueue.push(data);
        } else if (!win.isDestroyed()) {
            win.webContents.send('rboundmsg', { buffer: data });
        }
    };

    // and load the index.html of the app.
    win.loadFile('render/index.html').then(() => {
        // TODO: find a more synchronous way to introduce the initialData values into the HTML
        // to prevent any chance of a "start flicker".

        if (AUTO_OPEN_DEV_TOOLS) {
            win.webContents.openDevTools();
        }

        let buffers = initialData === null ? [] : [initialData];
        buffers.concat(rBoundMessageQueue);
        rBoundMessageQueue = null;
        
        win.webContents.send('rboundmsg', { buffers });
    });

    return {
        send: sendToRenderer,
        setListener: (type, newListener) => { 
            let flushQueue = !listeners[type];
            listeners[type] = newListener;
            if (flushQueue && mBoundMessageQueues[type]) {
                mBoundMessageQueues[type].forEach(newListener);
                mBoundMessageQueues[type] = null;
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

module.exports = {
    createWindow,
};
