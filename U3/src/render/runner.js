window.init = () => {
    registerMessageListener((type, value) => {
        switch (type) {
            case 'INIT':
                if (value.type === 'GAME') {
                    initGame(value.options);
                } else {
                    throw new Error("INIT type not implemented: '" + value.type + "'.");
                }
                break;
            default:
                throw new Error("Message type not implemented: '" + type + "'.");
        }
    });
};
