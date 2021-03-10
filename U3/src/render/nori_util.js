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

    let convertRichText = (txt, element) => {
        let rawTokens = txt.split('@');
        let tokens = [];
        let tokenValue = null;
        for (let i = 0; i < rawTokens.length; i++) {
            tokenValue = rawTokens[i];
            if (i % 2 === 0) {
                if (tokenValue.length > 0) {
                    tokens.push({ type: 'str', value: tokenValue });
                }
            } else {
                if (tokenValue.length > 0) {
                    if (tokenValue === 'a') {
                        tokens.push({ type: 'str', value: '@' });
                    } else {
                        tokens.push({ type: 'command', value: tokenValue.split('-') });
                    }
                } else {
                    tokens.push({ type: 'close' });
                }
            }
        }
        let index = [0];
        let output = document.createElement('span');
        convertRichTextImpl(tokens, index, output, element);
        return output;
    };

    let convertRichTextImpl = (tokens, index, output, element) => {
        let nested = null;
        while (index[0] < tokens.length) {
            let token = tokens[index[0]++];
            switch (token.type) {
                case 'str':
                    output.append(token.value);
                    break;
                case 'command':
                    nested = document.createElement('span');
                    switch (token.value[0]) {
                        case 'b': nested.style.fontWeight = 'bold'; break;
                        case 'i': nested.style.fontStyle = 'italic'; break;
                        case 'u': nested.style.textDecoration = 'underline'; break;
                        case 'sup': nested = document.createElement('sup'); break;
                        case 'sub': nested = document.createElement('sub'); break;
                        case 'tx': break;
                        case 'link':
                            (linkIdB64 => {
                                nested = document.createElement('a');
                                nested.href = 'javascript:void(0)'; // without this, the link does not display
                                nested.addEventListener('click', () => {
                                    let handler = element.NORI_handlers.outer['link'];
                                    if (handler) {
                                        handler(linkIdB64);
                                    }
                                });
                            })(token.value[1]);
                            break;
                        case 'sz': nested.style.fontSize = token.value[1] + 'pt'; break;
                        case 'col':
                        case 'bg':
                            let rgba = token.value.slice(1);
                            let color;
                            if (rgba.length === 4) {
                                rgba[3] /= 255;
                                color = 'rgba(' + rgba.join(',') + ')';
                            } else {
                                color = 'rgb(' + rgba.join(',') + ')';
                            }
                            if (token.value[0] === 'col') {
                                nested.style.color = color;
                            } else {
                                nested.style.backgroundColor = color;
                            }
                            break;

                        default:
                            throw new Error();

                    }
                    output.append(nested);
                    convertRichTextImpl(tokens, index, nested, element);
                    break;
                case 'close':
                    return;
                default: throw new Error();
            }
        }
    };

    return {
        noopFn,
        promiseWait,
        escapeHtml,
        convertRichText,
        decodeB64,
        encodeB64,
        encodeHexColor,
    };
})();
