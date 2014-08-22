function slow_dictionary_get(dictionary, key, defaultValue) {
    var output = dictionary[key];
    if (output === undefined) return defaultValue;
    return output;
}

function slow_dictionary_set(dictionary, key, value) {
    dictionary[key] = value;
}

function slow_dictionary_get_keys(dictionary) {
    var output = [];
    for (var key in dictionary) {
        output.push(key);
    }
    return output;
}
