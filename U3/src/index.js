const { app } = require('electron');
const { createHub } = require('./messagehubclient');
const renderwindow = require('./renderwindow.js');

const HIDE_MENU_AND_DEBUG = false;

let args = (() => {
    let output = {};
    let argv = process.argv;
    for (let i = 0; i < argv.length; ++i) {
        let arg = argv[i];
        if (arg.startsWith('--u3:')) {
            let key = arg.substr('--u3:'.length);
            let value = i + 1 < argv.length ? argv[i + 1] : '';
            output[key] = value;
        }
    }
    return output;
})();

let u3Token = args.token || 'u3debug';
let pid = args.pid === undefined ? null : args.pid;
if (pid <= 0) pid = null; // TODO: monitor this process ID and terminate the u3 window if the pid goes away. 

app.whenReady().then(() => {

    let rwindow = null;

    const hub = createHub(u3Token);

    let queuedEventBatch = [];

    hub.addListener('u3init', (msg, cb) => {
        let { title, width, height, initialData } = msg;
        rwindow = renderwindow.createWindow(
            title, 
            width, 
            height, 
            initialData.length === 0 ? null : initialData,
            HIDE_MENU_AND_DEBUG,
            (closeBehavior) => {
                hub.send('u3close', closeBehavior);
            });
        rwindow.setListener('events', msgs => {
            hub.send('u3events', msgs);
        });
        rwindow.setListener('shown', () => {
            // Don't fire the callback until the window is fully ready.
            // Otherwise Crayon can send new messages that will get dropped. 
            if (cb) cb(true); 

            let batchSender = () => {
                if (queuedEventBatch.length > 0) {
                    let events = queuedEventBatch;
                    queuedEventBatch = [];
                    hub.send('u3batch', events);
                }
                setTimeout(batchSender, 50); // limit input data to 20 FPS
            };
            
            batchSender();
        });
        rwindow.setListener('eventBatch', data => {
            queuedEventBatch = queuedEventBatch.concat(data);
        });
    });

    hub.addListener('u3data', msg => {
        if (rwindow === null) {
            throw new Error("The first message must be u3init");
        }
        let { buffer } = msg;
        rwindow.send(buffer);
    });

    hub.addListener('u3close', _ => {
        rwindow.close();
    });

    hub.start();
});

