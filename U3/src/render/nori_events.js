const NoriEvents = (() => {
    let isKeyPressed = {};
    
    window.addEventListener('keydown', e => {
        if (ctx.hasKeyDownListener) handleKeyEvent(e, true);
    });

    window.addEventListener('keyup', e => {
        if (ctx.hasKeyUpListener) handleKeyEvent(e, false);
    });

    const handleKeyEvent = (e, isDown) => {
        let keyName = keyEventToType(e);
        if (isDown && isKeyPressed[keyName]) return;
        isKeyPressed[keyName] = isDown;
        let arg = keyName + '|' + keyEventToChar(e);
        if (e.shiftKey) arg += '|s';
        if (e.ctrlKey) arg += '|c';
        if (e.altKey) arg += '|a';
        platformSpecificHandleEvent(-1, 'frame.' + (isDown ? 'keydown' : 'keyup'), arg);
    };

    const keyEventToChar = e => {
        let c = e.key;
        switch (c) {
            // These mess with protocol delimiters
            case ' ': return 'SPACE';
            case '|': return 'PIPE';
            case 'Enter': return '\n';
            case 'Tab': return '\t';
            
            default:
                if (c.length > 1) return '';
                break;
        }
        return c;
    };

    const keyEventToType = e => {
        let c = e.code;
        switch (c) {
            case 'ShiftRight':
            case 'ShiftLeft':
                return 'shift';
            case 'ControlLeft':
            case 'ControlRight':
                return 'ctrl';
            case 'AltLeft':
            case 'AltRight':
                return 'alt';

            case 'ArrowLeft': return 'left';
            case 'ArrowRight': return 'right';
            case 'ArrowUp': return 'up';
            case 'ArrowDown': return 'down';

            case 'Minus': return '-';
            case 'Equal': return '=';
            case 'BracketLeft': return '[';
            case 'BracketRight': return ']';
            case 'Backslash': return '\\';
            case 'Semicolon': return ';';
            case 'Quote': return "'";
            case 'Comma': return ',';
            case 'Period': return '.';
            case 'Slash': return '/';
            case 'Backquote': return '`';

            case 'ContextMenu': return 'context';
            case 'MetaLeft': return 'os';

            case 'Backspace':
            case 'CapsLock':
            case 'Enter':
            case 'Tab':
            case 'Space': 
            case 'Escape':
            case 'F10':
            case 'F11':
            case 'F12':
            case 'Home':
            case 'End':
            case 'PageUp':
            case 'PageDown':
            case 'Delete':
            case 'Insert':
            case 'PrintScreen':
                return c.toLowerCase();
        }
        if (c.startsWith('Key') && c.length == 4) return c.charAt(3).toLowerCase();
        if (c.startsWith('Digit') && c.length == 6) return c.charAt(5);
        if (c.length == 2 && c.charAt(0) == 'F' && parseInt(c.charAt(1)) + '' == c.charAt(1)) return c.toLowerCase();
        return 'unknown:' + c;
    };

    
    let buildEventHandler = (value, e, eventName, argsOrFunc) => {
        if (value === 0) return NoriUtil.noopFn;
        let isFunc = typeof argsOrFunc === 'function';
        return function(ev) {
            let args = isFunc ? argsOrFunc(ev) : argsOrFunc;
            platformSpecificHandleEvent(e.NORI_id, eventName, args);
        };
    };

    return {
        buildEventHandler,
    };
})();
