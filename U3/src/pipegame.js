const namedpipeserver = require('./namedpipeserver.js');
let renderTunnel = require('./rendertunnel.js');
let util = require('./util.js');

const parseGameRender = data => {
    let output = [];
    let len = data.length;
    let i = 1;
    while (i < len) {
        switch (data[i]) {
            case 'F':
                if (i + 3 >= len) throw new Error("Incomplete data");
                output.push('F', parseInt(data[i + 1]), parseInt(data[i + 2]), parseInt(data[i + 3]));
                i += 4;
                break;
            
            case 'R':
                if (i + 7 >= len) throw new Error("Incomplete data");
                output.push('R');
                for (let j = 1; j <= 7; ++j) {
                    output.push(parseInt(data[i + j]));
                }
                i += 8;
                break;
            
            default:
                throw new Error("Unrecognized render command: '" + data[i] + "'");
        }
    }
    return output;
};

const run = () => {
    let tunnel = renderTunnel.createGameTunnel('My Game', 1000, 800);

    tunnel.setListener(msg => {
        console.log(msg);
    });

    namedpipeserver.runServer(
        'u3pipe',
        data => {
            let dataItems = data.split(' ');
            let args;
            let commandName = dataItems[0];
            switch (commandName) {
                case 'GAME_RENDER':
                    args = parseGameRender(dataItems);
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
