
LIB$json$parseJson = function(globals, rawText) {
    try {
        return LIB$json$convertJsonThing(globals, window.JSON.parse(rawText));
    } catch (e) {
        return null;
    }
};

LIB$json$convertJsonThing = function(globals, thing) {
    var type = LIB$Json$typeClassifyHelper(thing);
    switch (type) {
        case 'null': return buildNull(globals);
        case 'bool': return buildBoolean(globals, thing);
        case 'string': return buildString(globals, thing);
        case 'list':
            var list = [];
            for (i = 0; i < thing.length; ++i) {
                list.push(LIB$json$convertJsonThing(globals, thing[i]));
            }
            return buildList(list);
        case 'dict':
            var keys = [];
            var values = [];
            for (var rawKey in thing) {
                keys.push(rawKey);
                values.push(LIB$json$convertJsonThing(globals, thing[rawKey]));
            }
            return buildStringDictionary(globals, keys, values);
        case 'int':
            return buildInteger(globals, thing);
        case 'float':
            return buildFloat(globals, thing);
        default:
            return buildNull(globals);
    }
};

LIB$Json$typeClassifyHelper = function(t) {
    if (t === null) return 'null';
    if (t === true || t === false) return 'bool';
    if (typeof t == "string") return 'string';
    if (typeof t == "number") {
        if (t % 1 == 0) return 'int';
        return 'float';
    }
    ts = Object.prototype.toString.call(t);
    if (ts == '[object Array]') {
        return 'list';
    }
    if (ts == '[object Object]') {
        return 'dict';
    }
    return 'null';
};
