const { b64Encode, b64Decode } = (() => {
  let letters = "abcdefghijklmnopqrstuvwxyz";
  let alphabet = (letters.toUpperCase() + letters + "0123456789+/").split('');

  let alphabetInv = {};
  for (let i = 0; i < alphabet.length; i++) {
    alphabetInv[alphabet[i]] = i;
  }
  
  let b64Encode = str => {
    let buffer = new TextEncoder().encode(str);
    let pairs = [];
    let len = buffer.length;
    let b;
    for (let i = 0; i < len; i++) {
      b = buffer[i];
      pairs.push(
        (b >> 6) & 3,
        (b >> 4) & 3,
        (b >> 2) & 3,
        b & 3);
    }

    while (pairs.length % 3 !== 0) pairs.push(0);
    
    let sb = [];
    for (let i = 0; i < pairs.length; i += 3) {
      let n = (pairs[i] << 4) | (pairs[i + 1] << 2) | pairs[i + 2];
      sb.push(alphabet[n]);
    }
    while (sb.length % 4 !== 0) sb.push('=');
    
    return sb.join('');
  };

  let b64Decode = str => {
    let pairs = [];
    let c, n;
    let len = str.length;
    let eqCount = 0;
    for (let i = 0; i < len; i++) {
      c = str.charAt(i);
      if (c == '=') {
        eqCount++;
        continue;
      }
      n = alphabetInv[c];
      if (n === undefined) throw new Error("Invalid character: " + c);
      pairs.push((n >> 4) & 3, (n >> 2) & 3, n & 3);
    }
    if (eqCount == 1) {
      pairs.pop();
    } else if (eqCount == 2) {
      pairs.pop();
      pairs.pop();
      pairs.pop();
    }
    let bytes = [];
    len = pairs.length;
    for (let i = 0; i < len; i += 4) {
      bytes.push((pairs[i] << 6) | (pairs[i + 1] << 4) | (pairs[i + 2] << 2) | pairs[i + 3]);
    }
    return new TextDecoder('utf-8').decode(new Uint8Array(bytes).buffer);
  };

  return { b64Encode, b64Decode };
})();
