const { ipcRenderer } = require('electron');

window.registerMessageListener = null;
window.sendMessage = null;
(() => {

    let listener = null;
    window.registerMessageListener = f => {
        listener = f;
    };

    ipcRenderer.on('rboundmsg', (event, data) => {
        if (listener !== null) {
            listener(data);
        }
    });

    window.sendMessage = (data) => {
        ipcRenderer.send('mboundmsg', data);
    };
})();
