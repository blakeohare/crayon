const NoriUtil = (() => {

    let noopFn = () => { };

    let promiseWait = ms => {
        return new Promise(res => {
           window.setTimeout(() => res(true), ms);
        });
    };

    function escapeHtml(text, preserveWhitespace) {
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
                case '\r': c = ''; break;
                case '\n': c = preserveWhitespace ? '<br/>' : '\n'; break;
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

    let encodeHexColor = (r, g, b) => TO_HEX_HASH[r] + TO_HEX[g] + TO_HEX[b];

    let decodeB64 = (str) => {
        return decodeURIComponent(atob(str).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
    };

    let encodeB64 = (str) => {
        if (window.Buffer) return Buffer.from(str).toString('base64');
        // TODO: this is currently ASCII-centric
        let charCodes = str.split('').map(c => Math.min(c.charCodeAt(0), 127));
        let pairs = [];
        for (let cc of charCodes) {
            pairs.push(
                (cc >> 6) & 3,
                (cc >> 4) & 3,
                (cc >> 2) & 3,
                (cc) & 3);
        }

        let sb = [];
        let alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'.split('');
        for (let i = 0; i < pairs.length; i += 3) {
            let n = pairs[i];
            n *= 4;
            if (i + 1 < pairs.length) n += pairs[i + 1];
            n *= 4;
            if (i + 2 < pairs.length) n += pairs[i + 2];
            sb.push(alphabet[n]);
        }
        while (sb.length % 4 != 0) sb.push('=');
        return sb.join('');
    };

    return {
        noopFn,
        promiseWait,
        escapeHtml,
        decodeB64,
        encodeB64,
        encodeHexColor,
    };
})();
