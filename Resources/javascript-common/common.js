C$common = 1;
C$common$textResources = {};
C$common$jsFilePrefix = ''; // overridden by resources.js if present.
C$common$programData = null;
C$common$resources = {};

C$common$readResourceText = function (path) {
    var output = C$common$resources[path];
    if (output === undefined) return null;
    return output;
};

C$common$alwaysTrue = function () { return true; };
C$common$alwaysFalse = function () { return false; };

C$common$now = function () {
    return (Date.now ? Date.now() : new Date().getTime()) / 1000.0;
};

C$common$addTextRes = function (path, value) {
    C$common$textResources[path] = value;
};

C$common$getTextRes = function (path) {
    return C$common$textResources[path];
};


C$common$print = function (value) {
    console.log(value);
};

C$common$is_valid_integer = function (value) {
    var test = parseInt(value);
    // NaN produces a paradocical value that fails the following tests...
    // TODO: verify this on all browsers
    return test < 0 || test >= 0;
};

C$common$sortedCopyOfArray = function(nums) {
    var newArray = nums.concat([]);
    newArray.sort();
    return newArray;
};

C$common$floatParseHelper = function(floatOut, text) {
    var output = parseFloat(text);
    if (output + '' == 'NaN') {
        floatOut[0] = -1;
        return;
    }
    floatOut[0] = 1;
    floatOut[1] = output;
};

C$common$parseJson = function(rawText) {
    try {
        return C$common$convertJsonThing(JSON.parse(rawText));
    } catch (e) {
        return null;
    }
};

C$common$convertJsonThing = function(thing) {
    var type = C$common$typeClassify(thing);
    switch (type) {
        case 'null': return v_VALUE_NULL;
        case 'bool': return thing ? v_VALUE_TRUE : v_VALUE_FALSE;
        case 'string': return v_buildString(thing);
        case 'list':
            var list = [];
            for (i = 0; i < thing.length; ++i) {
                list.push(C$common$convertJsonThing(thing[i]));
            }
            return v_buildListByWrappingInput(list);
        case 'dict':
            var keys = [];
            var values = [];
            for (var rawKey in thing) {
                keys.push(rawKey);
                values.push(C$common$convertJsonThing(thing[rawKey]));
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

C$common$typeClassify = function(t) {
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

C$common$pumpAsyncMessageQueue = function () {
    return null;
};

C$common$dictionaryKeys = function (dictionary) {
    var output = [];
    for (var key in dictionary) {
        output.push(key);
    }
    return output;
};

C$common$dictionaryValues = function (dictionary) {
    var output = [];
    for (var key in dictionary) {
        output.push(dictionary[key]);
    }
    return output;
};

C$common$stringEndsWith = function (value, findme) {
    return value.indexOf(findme, value.length - findme.length) !== -1;
};

C$common$shuffle = function (list) {
    var t;
    var length = list.length;
    var tindex;
    for (i = length - 1; i >= 0; --i) {
        tindex = Math.floor(Math.random() * length);
        t = list[tindex];
        list[tindex] = list[i];
        list[i] = t;
    }
};

C$common$createNewArray = function (size) {
    var output = [];
    while (size-- > 0) output.push(null);
    return output;
};

C$common$multiplyList = function (list, size) {
    var output = [];
    var length = list.length;
    var i;
    while (size-- > 0) {
        for (i = 0; i < length; ++i) {
            output.push(list[i]);
        }
    }
    return output;
};

C$common$clearList = function (list) {
    list.length = 0;
};

C$common$getElement = function (id) {
    return document.getElementById(id);
};
