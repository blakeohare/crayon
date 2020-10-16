const { app } = require('electron');
const tunnel = require('./rendertunnel.js');
//let testgame = require('./testgame.js');
let pipegame = require('./pipegame.js');

// TODO: just wrap this in tunnel logic
app.whenReady().then(() => {
    tunnel.markAsReady();
    //testgame.run();
    pipegame.run();
});
