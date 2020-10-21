const namedpipeserver = require('./namedpipeserver.js');

let runServer = (name, onData, onCreate, onClose) => {

    let currentChunk = [];
    let bytesRemaining = null;
    let bytesRemainingBuilder = [];

    let addChar = (c) => {
        if (bytesRemaining === null) {
            if (c === '@') {
                bytesRemaining = parseInt(bytesRemainingBuilder.join(''));
            } else {
                bytesRemainingBuilder.push(c);
            }
        } else {
            currentChunk.push(c);
            if (currentChunk.length === bytesRemaining) {
                bytesRemaining = null;
                bytesRemainingBuilder = [];
                let data = currentChunk.join('');
                currentChunk = [];
                onData(data);
            }
        }
    };
    namedpipeserver.runServer(
        name,
        (str) => {
            let pieces = str.split('\r\n').filter(s => s);
            for (let piece of pieces) {
                let len = piece.length;
                for (let i = 0; i < len; ++i) {
                    addChar(piece.charAt(i));
                }
            }
        },
        () => { if (onCreate) onCreate(); },
        () => { if (onClose) onClose(); });
};

module.exports = {
    runServer,
};
