const NoriUtil = (() => {

    let noopFn = () => { };

    let promiseWait = ms => {
        return new Promise(res => {
           window.setTimeout(() => res(true), ms); 
        });
    };

    function escapeHtml(text) {
        let o = [];
        let len = text.length;
        let c;
        for (let i = 0; i < len; ++i) {
            c = text.charAt(i);
            switch (c) {
                case '<': c ='&lt;'; break;
                case '>': c = '&gt;'; break;
                case '&': c = '&amp;'; break;
                case '"': c = '&quot;'; break;
            }
            o.push(c);
        }
        return o.join('');
    }
    
    let TO_HEX = (() => {
        let h = '0123456789abcdef'.split('');
        let arr = [];
        for (let a of h) {
            for (let b of h) {
                arr.push(a + b);
            }
        }
        return arr;
    })();
    let TO_HEX_HASH = TO_HEX.map(t => '#' + t);
    
    let encodeHex = (r, g, b) => TO_HEX_HASH[r] + TO_HEX[g] + TO_HEX[b];

    let HEX_LOOKUP = {};
    for (let i = 0; i < 10; ++i) {
        HEX_LOOKUP[i + ''] = i;
    }
    for (let i = 0; i < 6; ++i) {
        HEX_LOOKUP['abcdef'.charAt(i)] = i + 10;
        HEX_LOOKUP['ABCDEF'.charAt(i)] = i + 10;
    }

    let decodeHex = (s) => {
        let o = [];
        let len = s.length;
        let a;
        let b;
        for (let i = 0; i < len; i += 2) {
            a = HEX_LOOKUP[s.charAt(i)];
            b = HEX_LOOKUP[s.charAt(i + 1)];
            o.push(String.fromCharCode(a * 16 + b));
        }
        return o.join('');
    };
    
    return {
        noopFn,
        promiseWait,
        escapeHtml,
        decodeHex,
        encodeHex,
    };
})();