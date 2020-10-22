const net = require('net');

const runClient = (name, onCreate, onClose) => {

    let socket = null;
    let msgQueue = [];
    const client = {
        send: (data) => {
            msgQueue.push(data);
        },
        close: () => {
            socket.close();
        }
    };

    let attemptToCreateConnection = () => {

        let resolution = null;
        let p = new Promise(res => {
            resolution = res;
        });

        let attemptedSocket = null;
        attemptedSocket = net.createConnection('\\\\?\\pipe\\' + name, () => {
            resolution(true);
            attemptedSocket.on('close', () => {
                if (onClose) onClose();
            });

            if (onCreate) onCreate();

            client.send = (data) => {
                attemptedSocket.write(data.length + '@' + data);
            };

            for (let item of msgQueue) {
                client.send(item);
            }
            msgQueue = null;
            socket = attemptedSocket;
        });


        attemptedSocket.on('error', err => {
            let code = err.code;
            if (code === 'ENOENT') {
                console.log("Could not connect at the moment. Will retry.");
            } else {
                console.log("ERROR CODE: " + code);
                console.log(err);
            }
            resolution(false);
        });

        return p;
    };

    let counter = 1;
    let attemptRetryLoop = () => {
        attemptToCreateConnection().then(isSuccess => {
            if (!isSuccess) {
                if (counter >= 100) return;
                console.log("Retry attempt #" + counter);
                counter++;

                setTimeout(attemptRetryLoop, 2000);
            }
        });
    };
    attemptRetryLoop();

    return client;
};

module.exports = {
    runClient,
};
