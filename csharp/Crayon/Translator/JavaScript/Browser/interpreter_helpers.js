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

function create_new_array(size) {
	var output = [];
	while (size-- > 0) output.push(null);
	return output;
}