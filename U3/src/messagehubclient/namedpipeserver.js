const net = require('net');
const fs = require('fs');

let runServer = (name, onData, onCreate, onClose) => {

    let isWindows = process.platform === 'win32';
    let path = isWindows ? ('\\\\?\\pipe\\' + name) : (process.env['TMPDIR'] + 'org.crayonlang/u3/' + name);

    if (!isWindows && fs.existsSync(path)) {
        fs.unlinkSync(path);
    }

    let server = net.createServer(stream => {
        stream.on('data', c => {
            let dataStr = c.toString('utf8');
            if (onData) onData(dataStr);
        });

        stream.on('end', () => {
            if (onClose) onClose();
        });
    });

    server.listen(path, () => {
        if (onCreate) onCreate();
    });
};

module.exports = {
    runServer,
};
