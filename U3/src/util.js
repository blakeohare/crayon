let base64ToText = b64Value => {
    return Buffer.from(b64Value, 'base64').toString('utf8');
};

let textToBase64 = rawValue => {
    return Buffer.from(rawValue).toString('base64');
};

module.exports = {
    base64ToText,
    textToBase64,
};
