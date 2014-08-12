function slow_dictionary_get(dictionary, key, defaultValue) {
    var output = dictionary[key];
    if (output === undefined) return defaultValue;
    return output;
}

function slow_dictionary_set(dictionary, key, value) {
    dictionary[key] = value;
}
