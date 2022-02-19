(() => {
  let sendStringToSwift = (type, msg) => {
    let payload = b64Encode(type) + ":" + b64Encode(msg)
    window.webkit.messageHandlers.interop.postMessage(payload);
  };

  let handleMessage = (type, value) => {
    // There are no messages types...yet
  };

  window.sendStringToJavaScript = msg => {
    let parts = msg.split(':', 2); // comes in as base64 so 2 will not truncate any parts.
    let msgType = b64Decode(parts[0]);
    let msgValue = b64Decode(parts[1] || "");
    handleMessage(msgType, msgValue);
  };
  
  window.addEventListener('load', () => {
    sendStringToSwift("READY", "");
  });

  // TODO: fix this up so that it's closer to parity with Node's STDOUT
  let stringify = (value) => {
    if (value === null) return 'null';
    if (value === undefined) return 'undefined';
    if (value === true) return 'true';
    if (value === false) return 'false';
    if (value === "") return '""';
    let type = typeof value;
    if (type === 'number' && isNaN(value)) return 'NaN';
    if (type === 'object') {
      try {
        return JSON.stringify(value);
      } catch (err) {
        return Array.isArray(value)
          ? ("<Array:len=" + value.length + ">")
          : ("<Object:keys=" + Object.keys(value).join(',') + ">");
      }
    }
    return value + '';
  };

  console.log = function() {
    let sb = [];
    for (let i = 0; i < arguments.length; i++) {
      sb.push(stringify(arguments[i]));
    }
    sendStringToSwift("STDOUT", sb.join(" "));
  };
  
})();
