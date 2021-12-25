window.registerMessageListener = null;
window.sendMessage = null;
(() => {
  let upstreamQueue = [];

  let handleDownstreamMessage = null;
  window.registerMessageListener = fn => {
    handleDownstreamMessage = fn;
  };

  window.receiveDownstreamData = rawStr => {
    let data = JSON.parse(rawStr);
    handleDownstreamMessage(data);
  };

  window.sendMessage = (type, payload) => {
    if (upstreamQueue !== null) {
      upstreamQueue.push([type, payload]);
    } else {
      let data = { type, message: JSON.stringify(payload) };
      window.downstreamObj(data);
    }
  };

  window.addEventListener('load', () => {
    let uq = upstreamQueue;
    upstreamQueue = null;
    uq.forEach(item => sendMessage(...item));
    sendMessage('bridgeReady', {});
  });
})();
