(() => {

    let hex = [];
    let poundHex = [];

    for (let i = 0; i < 256; ++i) {
        let a = i >> 4;
        let b = i & 15;
        let h = '0123456789abcdef'.charAt(a) + '0123456789abcdef'.charAt(b);
        hex.push(h);
        poundHex.push('#' + h);
    }

    let rgbToHex = (r, g, b) => {
        r = Math.max(0, Math.min(255, Math.floor(r)));
        g = Math.max(0, Math.min(255, Math.floor(g)));
        b = Math.max(0, Math.min(255, Math.floor(b)));
        return poundHex[r] + hex[g] + hex[b];
    };

    window.initGame = (options) => {

        let root = document.getElementById('html_render_host');
        let canvas = document.createElement('canvas');
        root.append(canvas);
        let s = canvas.style;
        s.width = '100%';
        s.height = '100%';
        let ctx = canvas.getContext('2d');

        let gameInitHandler = data => {
            let title = data[0]; // TODO: decode base64
            let width = parseInt(data[1]);
            let height = parseInt(data[2]);
            canvas.width = width;
            canvas.height = height;
        };

        let gameRenderHandler = data => {
            let len = data.length;
            let i = 1;
            let r, g, b, a, width, height, x, y;
            while (i < len) {
                switch (data[i]) {
                    case 'F':
                        r = parseInt(data[i + 1]);
                        g = parseInt(data[i + 2]);
                        b = parseInt(data[i + 3]);
                        i += 4;
                        ctx.fillStyle = rgbToHex(r, g, b);
                        ctx.fillRect(0, 0, canvas.width, canvas.height);
                        break;
                    
                    case 'R':
                        x = parseInt(data[i + 1]);
                        y = parseInt(data[i + 2]);
                        width = parseInt(data[i + 3]);
                        height = parseInt(data[i + 4]);
                        r = parseInt(data[i + 5]);
                        g = parseInt(data[i + 6]);
                        b = parseInt(data[i + 7]);
                        a = parseInt(data[i + 8]);
                        i += 9;
                        ctx.fillStyle = rgbToHex(r, g, b);
                        ctx.fillRect(x, y, width, height);
                        break;
                    
                    case 'E':
                        x = parseInt(data[i + 1]);
                        y = parseInt(data[i + 2]);
                        width = parseInt(data[i + 3]);
                        height = parseInt(data[i + 4]);
                        x += width / 2.0;
                        y += height / 2.0;
                        width *= .5;
                        height *= .5;
                        r = parseInt(data[i + 5]);
                        g = parseInt(data[i + 6]);
                        b = parseInt(data[i + 7]);
                        a = parseInt(data[i + 8]);
                        i += 9;
                        ctx.fillStyle = rgbToHex(r, g, b);
                        ctx.beginPath();
                        ctx.ellipse(x, y, width, height, 0, 0, Math.PI * 2);
                        ctx.fill();
                        break;

                    default:
                        throw new Error("Not implemented: '" + data[i] + "'");
                }
            }
        };

        registerMessageListener((type, value) => {
            switch (type) {
                case 'GAME_INIT': gameInitHandler(value); break;
                case 'GAME_RENDER': gameRenderHandler(value); break;
                default:
                    throw new Error("Not implemented: '" + type + "'");
            }
        });
    };

})();
