const findProcess = require('find-process');

let watchProcess = (pid, onStopped) => {
 
    let doWatch = () => {
        findProcess('pid', pid).then(list => {
            if (list.length === 0) {
                onStopped();
            } else {
                setTimeout(doWatch, 50);
            }
        });
    };

    doWatch();
};

module.exports = {
    watchProcess,
};