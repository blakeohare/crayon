
LIB$json$parseJson = function(globals, rawText) {
    try {
        return LIB$json$convertJsonThing(globals, window.JSON.parse(rawText));
    } catch (e) {
        return null;
    }
};

LIB$json$convertJsonThing = function(globals, thing) {
    var type = C$common$typeClassify(thing);
    switch (type) {
        case 'null': return v_buildNull(globals);
        case 'bool': return v_buildBoolean(globals, thing);
        case 'string': return v_buildString(globals, thing);
        case 'list':
            var list = [];
            for (i = 0; i < thing.length; ++i) {
                list.push(LIB$json$convertJsonThing(globals, thing[i]));
            }
            return v_buildList(list);
        case 'dict':
            var keys = [];
            var values = [];
            for (var rawKey in thing) {
                keys.push(rawKey);
                values.push(LIB$json$convertJsonThing(globals, thing[rawKey]));
            }
            return v_buildStringDictionary(globals, keys, values);
        case 'int':
            return v_buildInteger(globals, thing);
        case 'float':
            return v_buildFloat(globals, thing);
        default:
            return v_buildNull(globals);
    }
};
