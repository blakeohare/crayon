const namedpipeserver = require('./namedpipeserver.js');
let renderTunnel = require('./rendertunnel.js');
let util = require('./util.js');

const run = () => {
    let tunnel = renderTunnel.createGameTunnel('My Game', 1000, 800);

    tunnel.setListener(msg => {
        console.log(msg);
    });

    namedpipeserver.runServer(
        'u3pipe',
        data => {
            let dataItems = data.split(' ');
            let args = dataItems;
            let commandName = dataItems[0];
            switch (commandName) {
                case 'GAME_RENDER':
                    // send data as-is
                    break;

                case 'GAME_INIT':
                    args = dataItems.slice(1);
                    tunnel.setTitle(util.base64ToText(args[0]));
                    let width = parseInt(args[1]);
                    let height = parseInt(args[2]);
                    tunnel.setSize(width, height);
                    break;
                
                default:
                    throw new Error("Unrecognized command: " + commandName.substr(0, 100) + "...");
            }
            tunnel.send(commandName, args);
        },
        () => {
            console.log("Now listening");
        },
        () => {
            console.log("Pipe closed");
        });
};

module.exports = { run };
