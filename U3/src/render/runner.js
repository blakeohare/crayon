window.init = () => {

    let initialized = false;
    let handleMessageBuffer = buffer => {
        if (!initialized) {
            initialized = true;
            shimInit(buffer);
        } else {
            flushUpdates(buffer);
        }
    };
    registerMessageListener(data => {
        if (data.buffer) {
            handleMessageBuffer(data.buffer);
        } else if (data.buffers) {
            for (let buffer of data.buffers) {
                handleMessageBuffer(buffer);
            }
        } else {
            throw new Error("Empty message!");
        }
    });
    
};
