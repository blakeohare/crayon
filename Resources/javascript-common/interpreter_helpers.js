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

function stringEndsWith(value, findme) {
	return value.indexOf(findme, value.length - findme.length) !== -1;
}

function shuffle(list) {
	var t;
	var length = list.length;
	var tindex;
	for (i = length - 1; i >= 0; --i) {
		tindex = Math.floor(Math.random() * length);
		t = list[tindex];
		list[tindex] = list[i];
		list[i] = t;
	}
}

function create_new_array(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
}

function multiply_list(list, size) {
	var output = [];
	var length = list.length;
	var i;
	while (size-- > 0) {
		for (i = 0; i < length; ++i) {
			output.push(list[i]);
		}
	}
	return output;
}

function clear_list(list) {
	list.length = 0;
}

function getElement(id) {
    return document.getElementById(id);
};
