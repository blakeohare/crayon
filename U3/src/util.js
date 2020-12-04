const fs = require('fs');
const temp = require('temp');

let base64ToText = b64Value => {
    return Buffer.from(b64Value, 'base64').toString('utf8');
};

let textToBase64 = rawValue => {
    return Buffer.from(rawValue).toString('base64');
};

let writeBase64ToTempFile = (prefix, suffix, data) => {
    let resolve = null;
    let p = new Promise(res => {
        resolve = res;
    });
    temp.open({ prefix, suffix }, (err, info) => {
        let buf = Buffer.from(data, 'base64');
        console.log(info.fd);
        fs.write(info.fd, buf, (err) => {
            resolve({
                path: info.path,
            });
        });
    });
    return p;
};

module.exports = {
    base64ToText,
    textToBase64,
    writeBase64ToTempFile,
};
