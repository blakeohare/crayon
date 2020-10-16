const net = require('net');

let runServer = (name, onData, onCreate, onClose) => {

    let server = net.createServer(stream => {
        stream.on('data', c => {
            let dataStr = c.toString('utf8');
            if (onData) onData(dataStr);
        });

        stream.on('end', () => {
            stream.close();
            if (onClose) onClose();
        });
    });
    
    server.listen('\\\\?\\pipe\\' + name, () => {
        if (onCreate) onCreate();
    });
};
    
module.exports = {
    runServer,
};
