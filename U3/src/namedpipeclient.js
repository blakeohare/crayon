const net = require('net');

let runClient = (name) => {

    let msgQueue = [];
    let onConnect = () => {
        for (let msg of msgQueue) {
            socket.write(msg);
        }
        msgQueue = null;
    };
    let socket = null;
    let counter = 1;
    let connectionAttempter = () => {
        socket = net.createConnection('\\\\?\\pipe\\' + name, onConnect);
        socket.on('error', (err) => {
            if (err.code === 'ENOENT' && err.syscall === 'connect') {
                console.log("Could not connect to parent process. Re-attempt #" + counter);
                counter++;
                setTimeout(connectionAttempter, 2000);
            } else {
                throw new Error("Unknown error: " + err.code);
            }
        });
    };

    connectionAttempter();

    return {
        send: str => {
            if (msgQueue !== null) {
                msgQueue.push(str);
            } else {
                socket.write(str);
            }
        },
        close: () => {
            socket.close();
        },
    };
};

module.exports = {
    runClient,
};
