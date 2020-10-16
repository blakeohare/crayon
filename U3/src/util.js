let base64ToText = b64Value => {
    return Buffer.from(b64Value, 'base64').toString('utf8');
};

module.exports = {
    base64ToText,
};
