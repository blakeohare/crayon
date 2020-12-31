const NoriGamepad = (() => {

    let gamepads = [];
    let isInitialized = false;

    window.addEventListener('gamepadconnected', e => {
        let index = gamepads.length;
        console.log(e.gamepad);
        window.blarg = e.gamepad;
        gamepads.push({
            name: e.gamepad.id,
            nativeIndex: e.gamepad.index,
            reported: false,
            buttons: {},
            buttonCount: e.gamepad.buttons.length,
            axes: {},
            axisCount: e.gamepad.axes.length,
        });
        if (isInitialized) {
            reportNewDevice(index);
        }
    });

    let init = () => {
        isInitialized = true;
        for (let i = 0; i < gamepads.length; ++i) {
            reportNewDevice(i);
        }
    };

    let pollDevices = () => {
        let changes = ['state'];
        let nativeGamepads = navigator.getGamepads();
        for (let gpIndex = 0; gpIndex < gamepads.length; gpIndex++) {
            let gp = gamepads[gpIndex];
            let ngp = nativeGamepads[gp.nativeIndex];
            let axes = ngp.axes;
            let buttons = ngp.buttons;

            for (let i = 0; i < axes.length; ++i) {
                let value = Math.floor(axes[i] * 1000);
                if (gp.axes[i] !== value) {
                    gp.axes[i] = value;
                    changes.push(gpIndex + '|A|' + i + '|' + value);
                }
            }

            for (let i = 0; i < buttons.length; ++i) {
                let value = !!buttons[i].pressed;
                if (gp.buttons[i] !== value) {
                    gp.buttons[i] = value;
                    changes.push(gpIndex + '|B|' + i + '|' + (value ? '1' : '0'));
                }
            }
        }
        if (changes.length > 1) {
            console.log(changes);
            platformSpecificHandleEvent(-1, 'gamepad', changes.join(':'));
        }
    };

    let reportNewDevice = index => {
        let gp = gamepads[index];
        console.log(gp);
        let arg = [
            'device',
            index,
            Buffer.from(gp.name).toString('base64'),
            gp.axisCount,
            gp.buttonCount,
        ].join(':');
        platformSpecificHandleEvent(-1, 'gamepad', arg);
        if (index === 0) {
            setTimeout(() => {
                setInterval(() => {
                    pollDevices();
                }, 15);
            }, 50);
        }
    };

    return {
        init,
    };
})();
