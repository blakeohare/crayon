function slow_dictionary_get(dictionary, key, defaultValue) {
    var output = dictionary[key];
    if (output === undefined) return defaultValue;
    return output;
}

function slow_dictionary_get_keys(dictionary) {
    var output = [];
    for (var key in dictionary) {
        output.push(key);
    }
    return output;
}

function slow_dictionary_get_values(dictionary) {
    var output = [];
    for (var key in dictionary) {
        output.push(dictionary[key]);
    }
    return output;
}

function create_list_of_size(n) {
    var output = [];
    while (output.length < n) output.push(null);
    return output;
}