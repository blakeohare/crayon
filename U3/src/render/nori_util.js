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
    
    let encodeHexColor = (r, g, b) => TO_HEX_HASH[r] + TO_HEX[g] + TO_HEX[b];

    let decodeB64 = (str) => {
        return decodeURIComponent(atob(str).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
    };
    
    return {
        noopFn,
        promiseWait,
        escapeHtml,
        decodeB64,
        encodeHexColor,
    };
})();