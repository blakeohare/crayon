/*
Dictionary is any dictionary to use as the virtual disk.
Ideally, this can be the localStorage object.
	
Keys are the prefix 'crayondisk:' followed by the node ID.
The root node ID is always an empty string.
Nodes are the following list:
for directories: [true, dictionary of children (strings) to node ID's (strings)]
for files: [false, is_binary, size, contents]
	
additionally there is a crayondisk_next_id key that is an integer that returns the next integer 
available for use as a node ID.
*/
function createFakeDisk(dictionary) {
	var fakeDisk = {};

	// illegal characters behave like Windows.
	var t = ':*?\\/|><"';
	var illegal_chars = {};
	for (var i = 0; i < t.length; ++i) {
		illegal_chars[t[i]] = true;
	}

	fakeDisk.disk = dictionary;
	if (fakeDisk.disk['crayondisk:0'] === undefined) fakeDisk.disk['crayondisk:0'] = [true, {}];
	if (fakeDisk.disk['crayondisk_next_id'] === undefined) fakeDisk.disk['crayondisk_next_id'] = 1;

	var allocate_id = function () {
		// TODO: does ++ work on local storage?
		return fakeDisk.disk['crayondisk_next_id']++;
	}

	fakeDisk.cache = { '/': 0 }; // populate with root

	var create_file_node = function (content) {
		var id = allocate_id();
		fakeDisk.disk['crayondisk:' + id] = [false, false, content.length, content];
		return id;
	}

	var create_folder_node = function () {
		var id = allocate_id();
		fakeDisk.disk['crayondisk:' + id] = [true, {}];
		return id;
	}

	var canonicalize_path = function (path) {
		if (path === null) {
			window.alert("Pause me");
		}
		path = path.replace('\\', '/');
		if (path == '/' || path.length == 0) return '/';
		if (path[0] != '/') path = '/' + path;
		if (path[path.length - 1] == '/') path = path.substr(0, path.length - 1);
		return path;
	}

	var get_parent_and_file = function (path) {
		if (path.length < 2) return null;
		var lastSlash = path.lastIndexOf('/');
		if (lastSlash == 0) return ['/', path.substr(1)];
		return [path.substr(0, lastSlash), path.substr(lastSlash + 1)];
	}

	var convert_path_to_node_id_impl = function (path) {
		if (path.length < 2) return 0; // could be empty from level_ups, otherwise we know the first char is / so <2 is always root.
		var node_id = fakeDisk.cache[path];
		if (node_id) return node_id;

		var t = get_parent_and_file(path);
		var level_up = t[0];
		var this_file = t[1];
		var parent_id = convert_path_to_node_id_impl(level_up);

		if (parent_id === null) return null;

		var directory = fakeDisk.disk['crayondisk:' + parent_id];
		if (!directory[0]) {
			return null;
		}

		var child_id = directory[1][this_file];
		if (child_id !== undefined) {
			fakeDisk.cache[path] = child_id;
			return child_id;
		}

		return null;
	}

	var convert_path_to_node_id = function (path) {
		path = canonicalize_path(path);
		if (path == '/') return 0;
		return convert_path_to_node_id_impl(path);
	};

	var get_node = function (path) {
		var node_id = convert_path_to_node_id(path);
		if (node_id == null) return null;
		return fakeDisk.disk['crayondisk:' + node_id];
	};

	var contains_illegal_chars = function (string) {
		for (var i = 0; i < string.length; ++i) {
			if (illegal_chars[string[i]]) return true;
		}
		return false;
	};

	fakeDisk.path_exists = function (path) {
		var node = get_node(path);
		return node != null;
	};

	fakeDisk.is_directory = function (path) {
		var node = get_node(path);
		return node != null && node[0];
	};

	fakeDisk.list_dir = function (path) {
		var node = get_node(path);
		if (node == null || !node[0]) return null;
		var output = [];
		for (var key in node[1]) {
			output.push(key);
		}
		output.sort();
		return output;
	};

	fakeDisk.read_file = function (path) {
		var node = get_node(path);
		if (node == null || node[0]) return null;
		return node[3];
	};

	// returns same values as write_text
	fakeDisk.write_binary = function (path, byte_array) {
		var text = [];
		var size = byte_array.length;
		for (var i = 0; i < size; ++i) {
			text.push(String.fromCharCode(byte_array[i]));
		}
		return fakeDisk.write_text(path, text.join(''));
	};

	/*
	return codes:
	0 - success
	1 - directory does not exist
	2 - disk full
	3 - path already occupied by a directory
	4 - corrupted disk
	5 - path name contains illegal characters
	*/
	fakeDisk.write_text = function (path, text) {
		path = canonicalize_path(path);
		if (path == '/') return 3;
		var t = get_parent_and_file(path);
		var dir = t[0];
		var file = t[1];
		if (contains_illegal_chars(file)) return 5;
		node = get_node(dir);
		if (node == null) return 1; // parent path doesn't exist.
		if (!node[0]) return 1; // parent path is a file.
		var existing_id = node[1][file];
		if (existing_id === undefined) {
			existing_id = create_file_node(text);
			node[1][file] = existing_id;
			return 0;
		} else {
			var existing_node = fakeDisk.disk['crayondisk:' + existing_id];
			if (existing_node) {
				if (existing_node[0]) return 3;
				existing_node[2] = text.length;
				existing_node[3] = text;
				fakeDisk.disk['crayondisk:' + existing_id] = existing_node;
				return 0;
			} else {
				// corrupted disk. hopefully this never happens.
				return 4;
			}
		}
	};

	/*
	return codes
	0 - success
	1 - path already exists
	2 - parent directory does not exist
	3 - disk full
	4 - path contains illegal characters
	*/
	fakeDisk.mk_dir = function (path) {
		path = canonicalize_path(path);
		if (path == '/') return 1;
		var node = get_node(path);
		if (node != null) return 1;
		var t = get_parent_and_file(path);
		path = t[0];
		var newdir = t[1];
		if (contains_illegal_chars(newdir)) return 4;
		node = get_node(path);
		if (node == null) return 2;
		if (!node[0]) return 2;
		if (node[1][newdir] === undefined) {
			node[1][newdir] = create_folder_node();
			return 0;
		}
		return 1;
	}

	return fakeDisk;
}
