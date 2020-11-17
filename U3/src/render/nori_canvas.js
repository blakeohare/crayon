const NoriCanvas = (() => {

    let handleCanvasData = (canvas, buffer, start, len) => {
        let ctx = canvas.getContext('2d');
        let cvWidth = canvas.width;
        let cvHeight = canvas.height;
        let end = start + len;
        let r, g, b, a, w, h, x, y, lineWidth, x2, y2, hex, img;
        let i = start;
        let imgLookup = canvas._NORI_imgLookup;
        if (!imgLookup) {
            imgLookup = {};
            canvas._NORI_imgLookup = imgLookup;
        }
        while (i < end) {
            r = buffer[i + 1];
            g = buffer[i + 2];
            b = buffer[i + 3];
            hex = NoriUtil.encodeHex(r, g, b);
            switch (buffer[i]) {
                case 'F':
                    i += 4;
                    ctx.fillStyle = hex;
                    ctx.fillRect(0, 0, cvWidth, cvHeight);
                    break;
                
                case 'R':
                    a = buffer[i + 4];
                    x = buffer[i + 5];
                    y = buffer[i + 6];
                    w = buffer[i + 7];
                    h = buffer[i + 8];
                    i += 9;
                    if (a !== 255) {
                        ctx.globalAlpha = a / 255;
                        ctx.fillStyle = hex;
                        ctx.fillRect(x, y, w, h);
                        ctx.globalAlpha = 1;
                    } else {
                        ctx.fillStyle = hex;
                        ctx.fillRect(x, y, w, h);
                    }
                    break;
                
                case 'E':
                    r = buffer[i + 1];
                    g = buffer[i + 2];
                    b = buffer[i + 3];
                    a = buffer[i + 4];
                    x = buffer[i + 5];
                    y = buffer[i + 6];
                    w = buffer[i + 7];
                    h = buffer[i + 8];
                    i += 9;
                    // TODO: this
                    break;
                
                case 'L':
                    r = buffer[i + 1];
                    g = buffer[i + 2];
                    b = buffer[i + 3];
                    a = buffer[i + 4];
                    x = buffer[i + 5];
                    y = buffer[i + 6];
                    x2 = buffer[i + 7];
                    y2 = buffer[i + 8];
                    lineWidth = buffer[i + 9];
                    i += 10;
                    // TODO: this
                    break;
                
                case 'ImgData':
                    r = buffer[i + 4]; // version key
                    g = buffer[i + 5]; // resource ID
                    b = buffer[i + 6]; // actual image data
                    ((imageData, versionKey) => {
                        let imgLoader = new Image();
                        imgLoader.onload = () => {
                            let newImg = document.createElement('canvas');
                            newImg.width = imgLoader.width;
                            newImg.height = imgLoader.height;
                            let ctx = newImg.getContext('2d');
                            ctx.drawImage(imgLoader, 0, 0);
                            imgLookup[versionKey] = newImg;
                        };
                        imgLoader.src = 'data:image/png;base64,' + imageData;
                    })(b, r);
                    i += 7;
                    break;

                case 'I1':
                    r = buffer[i + 4]; // version key
                    img = imgLookup[r];
                    if (img) {
                        x = buffer[i + 5];
                        y = buffer[i + 6];
                        ctx.drawImage(img, x, y);
                    }
                    i += 7;
                    break;

                default:
                    throw new Error("Unknown draw instruction: " + buffer[i]);
            }
        }
    };

    return {
        handleCanvasData,
    };
    
})();
