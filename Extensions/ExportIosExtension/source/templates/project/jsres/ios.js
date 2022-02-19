(() => {
  let sendStringToSwift = (type, msg) => {
    let payload = b64Encode(type) + ":" + b64Encode(msg)
    window.webkit.messageHandlers.interop.postMessage(payload);
  };

  let handleMessage = (type, value) => {
    // There are no messages types...yet
  };

  window.sendStringToJavaScript = msg => {
    let parts = msg.split(':', 2);
    let msgType = b64Decode(parts[0]);
    let msgValue = b64Decode(parts[1] || "");
    handleMessage(msgType, msgValue);
  };
  
  window.addEventListener('load', () => {
    sendStringToSwift("READY", "");
  });

  console.log = function() {
    let sb = [];
    for (let i = 0; i < arguments.length; i++) {
        sb.push(arguments[i] + '');
    }
    sendStringToSwift("STDOUT", sb.join(" "));
  };
  
})();
