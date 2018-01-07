
LIB$json$parseJson = function(rawText) {
    try {
        return LIB$json$convertJsonThing(window.JSON.parse(rawText));
    } catch (e) {
        return null;
    }
};

LIB$json$convertJsonThing = function(thing) {
    var type = C$common$typeClassify(thing);
    switch (type) {
        case 'null': return v_VALUE_NULL;
        case 'bool': return thing ? v_VALUE_TRUE : v_VALUE_FALSE;
        case 'string': return v_buildString(thing);
        case 'list':
            var list = [];
            for (i = 0; i < thing.length; ++i) {
                list.push(LIB$json$convertJsonThing(thing[i]));
            }
            return v_buildList(list);
        case 'dict':
            var keys = [];
            var values = [];
            for (var rawKey in thing) {
                keys.push(rawKey);
                values.push(LIB$json$convertJsonThing(thing[rawKey]));
            }
            return v_buildDictionary(keys, values);
        case 'int':
            return v_buildInteger(thing);
        case 'float':
            return v_buildFloat(thing);
        default:
            return v_VALUE_NULL;
    }
};
