const { createHub } = require('./messagehubclient');

console.log("Hello, World!");

let hub = createHub('abc123', true);

hub.addListener('weather', (data, cb) => {
    cb("The weather for " + data.zip + " is bad.");
});

hub.addListener('evenOddCheck', (data, cb) => {
    
    let num = data.num;
    let result = "not an integer.";
    if (num % 0 === 0) {
        result = num % 2 == 0 ? 'even' : 'odd';
    }
    cb(data.num + " is " + result);
});

hub.start().then(() => {
    console.log(">>> How are you?");
    hub.send('howAreYou', { name: "Zorp" }, result => {
        console.log("<<< " + result.msg);
    });
});
