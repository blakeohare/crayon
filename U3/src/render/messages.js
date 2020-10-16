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
            listener(data.type, data.value);
        }
    });

    window.sendMessage = (type, value) => {
        ipcRenderer.send('mboundmsg', { type, value });
    };
})();
