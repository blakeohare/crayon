window.addEventListener('load', () => {

    let initialized = false;
    let handleMessageBuffer = (buffer, options) => {
        if (!initialized) {
            initialized = true;
            shimInit(buffer, options);
        } else {
            flushUpdates(buffer);
        }
    };

    registerMessageListener(data => {
        if (data.buffer) {
            handleMessageBuffer(data.buffer, data.options);
        } else if (data.buffers) {
            let options = data.options || null;
            for (let buffer of data.buffers) {
                handleMessageBuffer(buffer, options);
                options = null;
            }
        } else {
            throw new Error("Empty message!");
        }
    });
});
