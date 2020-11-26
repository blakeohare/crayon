const NoriCanvas = (() => {

    let handleCanvasData = (canvas, buffer, start, len) => {
        let ctx = canvas.getContext('2d');
        let cvWidth = canvas.width;
        let cvHeight = canvas.height;
        let end = start + len;
        let r, g, b, a, w, h, x, y, lineWidth, x2, y2, hex, img;
        let tx, ty, sx, sy, tw, th, sw, sh;
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
                    a = buffer[i + 4];
                    x = buffer[i + 5];
                    y = buffer[i + 6];
                    w = buffer[i + 7];
                    h = buffer[i + 8];
                    i += 9;
                    w = w * 4 / 3; // no idea why this needs to exist to look correct...
                    ctx.beginPath();
                    ctx.moveTo(x, y - h);
                    ctx.bezierCurveTo(
                        x + w, y - h,
                        x + w, y + h,
                        x, y + h);
                    ctx.bezierCurveTo(
                        x - w, y + h,
                        x - w, y - h,
                        x, y - h);
                    
                    ctx.fillStyle = hex;
                    if (a !== 255) {
                        ctx.globalAlpha = a / 255;
                        ctx.fill();
                        ctx.closePath();
                        ctx.globalAlpha = 1;
                    } else {
                        ctx.fill();
                        ctx.closePath();
                    }
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
                        tx = buffer[i + 5];
                        ty = buffer[i + 6];
                        ctx.drawImage(img, tx, ty);
                    }
                    i += 7;
                    break;
                
                case 'I2':
                    r = buffer[i + 4]; // version key
                    img = imgLookup[r];
                    if (img) {
                        tx = buffer[i + 5];
                        ty = buffer[i + 6];
                        sx = buffer[i + 7];
                        sy = buffer[i + 8];
                        sw = buffer[i + 9];
                        sh = buffer[i + 10];
                        ctx.drawImage(img, sx, sy, sw, sh, tx, ty, sw, sh);
                    }
                    i += 11;
                    break;

                case 'I3':
                    r = buffer[i + 4]; // version key
                    img = imgLookup[r];
                    if (img) {
                        tx = buffer[i + 5];
                        ty = buffer[i + 6];
                        r = buffer[i + 7];
                        w = img.width;
                        h = img.height;

                        ctx.save();
                        ctx.translate(tx, ty);
                        ctx.rotate(r);

                        //if (a === 255) {
                            ctx.drawImage(img, -w / 2, -h / 2);
                        //} else {
                        //    ctx.globalAlpha = a / 255;
                        //    ctx.drawImage(canvas, -w / 2, -h / 2);
                        //    ctx.globalAlpha = 1;
                        //}
                        ctx.restore();
                    }
                    i += 8;
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
