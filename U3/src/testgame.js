let renderTunnel = require('./rendertunnel.js');

const run = () => {
    let tunnel = renderTunnel.createGameTunnel('My Game', 1000, 800);

    tunnel.setListener(msg => {
        console.log(msg);
    });

    let payload;

    let fill = (r, g, b) => {
        payload.push('F', r, g, b);
    };

    let drawRectangle = (x, y, w, h, r, g, b) => {
        payload.push('R', Math.floor(x), Math.floor(y), Math.floor(w), Math.floor(h), r, g, b);
    };

    let anim = [0, 0];

    let now = () => {
        return Date.now() / 1000.0;
    };

    let doFrame = () => {

        fill(100, 200, 255);

        drawRectangle(anim[0], anim[1], 50, 50, 255, 0, 0);

        anim[0] += 2;
        anim[1] += 1;
    };

    let prevFrame = now();
    let doGameLoop = () => {
        payload = [];

        doFrame();

        let currentTime = now();
        let diff = currentTime - prevFrame;
        let delay = Math.max(0.001, 1 / 60.0 - diff);
        prevFrame = currentTime;
        
        tunnel.send('GAME_RENDER', payload);
        setTimeout(doGameLoop, Math.floor(delay * 1000 + .5));
    };

    setTimeout(doGameLoop, 1);
};

module.exports = { run };
