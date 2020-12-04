const fs = require('fs');
const os = require('os');
const path = require('path');

let base64ToText = b64Value => {
    return Buffer.from(b64Value, 'base64').toString('utf8');
};

let textToBase64 = rawValue => {
    return Buffer.from(rawValue).toString('base64');
};

let charsForGibberish = 'abcdefghijklmnopqrstuvwxyz0123456789';
let generateGibberish = len => {
    let sb = [];
    while (len --> 0) {
        sb.push(charsForGibberish.charAt(Math.floor(Math.random() * charsForGibberish.length)));
    }
    return sb.join('');
};

let writeBase64ToTempFile = (prefix, suffix, data) => {
    let resolve = null;
    let p = new Promise(res => {
        resolve = res;
    });
    
    let buf = Buffer.from(data, 'base64');
    let iconpath = path.join(os.tmpdir(), prefix + generateGibberish(20) + suffix);
    
    fs.writeFile(iconpath, buf, (err) => {
        resolve({
            path: iconpath,
        });
    });
    return p;
};

module.exports = {
    base64ToText,
    textToBase64,
    writeBase64ToTempFile,
    generateGibberish,
};
