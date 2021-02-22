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
            hex = NoriUtil.encodeHexColor(r, g, b);
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
                    x += w / 2;
                    w /= 2;
                    y += h / 2;
                    h /= 2;
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

                case 'TX':
                    tx = buffer[i + 4];
                    x = buffer[i + 5];
                    y = buffer[i + 6];
                    i += 7;
                    img = getRenderedText(ctx, cvWidth, tx);
                    ctx.drawImage(img, x, y);
                    break;

                default:
                    throw new Error("Unknown draw instruction: " + buffer[i]);
            }
        }
    };

    let recentTextCache = {};
    let olderTextCache = {};
    let lastCacheFlush = (new Date()).getTime();

    let alignLookup = ['L', 'C', 'R', 'J'];

    let renderTextAndGetSize = (ctx, left, instructions) => {
        let layout = {
            width: 0,
            height: 0,
        };
        let largestSize = instructions.filter(t => t[0] === 'S').map(t => t[1]).reduce((a, b) => Math.max(a, b));
        let lineHeight = Math.floor(Math.max(1, largestSize * 1.5 + 1));
        let x = 0;
        let baseline = largestSize;
        let y = 0;

        let width;
        let currentFont = 'Arial';
        let currentSize = 12;
        for (let inst of instructions) {
            value = inst[1];
            switch (inst[0]) {
                case 'T':
                    width = ctx.measureText(value).width;
                    ctx.fillText(value, left + x, y + baseline);
                    x += width;
                    break;
                case 'C': ctx.fillStyle = value; break;
                case 'F': currentFont = value; ctx.font = currentSize + 'px ' + currentFont; break;
                case 'S': currentSize = value; ctx.font = currentSize + 'px ' + currentFont; break;
                case 'B': break; // :)
                case 'I': break; // :)
                case 'K': break; // :)
                case 'A': break; // :)
                default:
                    throw new Error("Unhandled text arg: " + JSON.stringify(inst));
            }
        }

        layout.width = x;
        layout.height = y + lineHeight;
        return layout;
    };

    let getRenderedText = (ctx, ctxWidth, rawInstruction) => {
        let img = recentTextCache[rawInstruction] || olderTextCache[rawInstruction];
        if (img) {
            recentTextCache[rawInstruction] = img;
            return img;
        }

        let parts = rawInstruction.split(',');
        let wrapWidth = 0;
        let alignment = 'L';
        let value = '';
        let largestSize = 0;

        let sb = [];
        let newInstructions = [];
        let pushText = t => {
            sb.push(t);
            if (newInstructions.length > 0) {
                let last = newInstructions[newInstructions.length - 1];
                if (last[0] == 'T') {
                    last[1].push(t);
                    return;
                }
            }
            newInstructions.push(['T', [t]]);
        };
        for (let i = 0; i < parts.length; i += 2) {
            let value = parts[i + 1];
            switch (parts[i]) {
                case 'T': pushText(value); break;
                case 'M': pushText(','); break;
                case 'W': wrapWidth = parseInt(value); break;
                case 'A': alignment = alignLookup[parseInt(value)]; break;
                case 'B': case 'I': newInstructions.push([parts[i], value == '1']); break;
                case 'K': case 'E': newInstructions.push([parts[i], parseFloat(value)]); break;
                case 'F': newInstructions.push(['F', value]); break;
                case 'S':
                    value = parseFloat(value);
                    largestSize = Math.max(largestSize, value);
                    newInstructions.push(['S', value]);
                    break;
                case 'C':
                    value = value.split('/');
                    newInstructions.push(['C', NoriUtil.encodeHexColor(
                        parseInt(value[0]), parseInt(value[1]), parseInt(value[2]))]);
                    break;
                default:
                    throw new Error("Unknown: " + parts[i]);
            }
        }
        newInstructions.forEach(item => { if (item[0] === 'T') item[1] = item[1].join(''); });

        wrapWidth = wrapWidth <= 0 ? null : wrapWidth;

        let bounds = renderTextAndGetSize(ctx, ctxWidth + 10, newInstructions);
        img = document.createElement('canvas');
        img.width = bounds.width;
        img.height = bounds.height;
        let ctx2 = img.getContext('2d');
        renderTextAndGetSize(ctx2, 0, newInstructions);
        recentTextCache[rawInstruction] = img;
        return img;
    };

    return {
        handleCanvasData,
    };

})();
