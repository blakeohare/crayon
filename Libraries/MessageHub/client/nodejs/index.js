const { createHub } = require('./messagehubclient');

console.log("MessageHub client tester for Node.js");

const hubToken = 'abc123'; // This would be provided to the process from e.g. a command line argument.

let hub = createHub(hubToken);

hub.addListener('weather', (data, cb) => {
    console.log(">>> Incoming weather query for " + data.zip);
    let response = "The weather for " + data.zip + " is bad.";
    console.log("<<< " + response);
    cb(response);
});

hub.addListener('evenOddCheck', (data, cb) => {
    let num = data.num;
    let result = "not an integer";
    if (num % 0 === 0) {
        result = num % 2 == 0 ? 'even' : 'odd';
    }
    cb(data.num + " is " + result + ".");
});

hub.start().then(() => {
    console.log(">>> How are you?");
    hub.send('howAreYou', { askerName: "Node.JS MessageHub Client Library" }, result => {
        console.log("<<< " + result.msg);
    });
});
