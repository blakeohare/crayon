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

When persisted to local storage (saved as a flat string), it takes the following formats:
crayondisk_next_id: just the integer as a decimal string
directories: '1' followed by a list of each key followed by its decimal node ID value all delimited by /'s
e.g. "1/My Documents/42/My Photos/20"
files: '0' followed by '1' or '0' for whether it is binary, followed by 12 characters that indicate the file size.
The format is the decimal length of the file followed by a '@' character followed by ignored characters until 12 digits are used.
The rest of the string is the raw content.
e.g. "0113@*********Hello, World!"

*/
var createFakeDisk = function (persistentStorage /* localStorage or null */) {
	var fakeDisk = {};

	// illegal characters behave like Windows.
	var t = ':*?\\/|><"';
	var illegal_chars = {};
	for (var i = 0; i < t.length; ++i) {
		illegal_chars[t[i]] = true;
	}
	var permanent = persistentStorage;
	var isLocalStorage = persistentStorage !== null;
	if (!isLocalStorage) {
		permanent = {};
	}
	var disk = {};

	var clearCorruptedDisk = function () {
		if (!isLocalStorage) return;
		for (var key in permanent) {
			if (key.indexOf('crayondisk') === 0) {
				permanent[key] = undefined;
			}
		}
	};

	var read_permanent_value = function (key) {
		var value = permanent[key];
		if (value !== undefined && value.length > 0) {
			if (key === 'crayondisk_next_id') {
				return parseInt(value);
			} else if (value.charAt(0) == '1') { // is directory
				var files_to_nodes_list = value.split('/');
				var lookup = {};
				for (var i = 1; i < files_to_nodes_list.length; i += 2) {
					var filename = files_to_nodes_list[i];
					var nodeId = parseInt(files_to_nodes_list[i + 1]);
					lookup[filename] = nodeId;
				}
				return [true, lookup];
			} else if (value.charAt(0) == '0' && value.length >= 14) { // is file
				var isBinary = value.charAt(1) == '1';
				var fileSize = 0;
				for (var i = 2; i < 14; ++i) {
					if (value.charAt(i) == '@') break;
					fileSize = fileSize * 10 + parseInt(value.charAt(i));
				}
				var content = value.substring(14);
				return [false, isBinary, fileSize, content];
			}
		}
		return null;
	};

	var read_value = function (key) {
		var value = disk[key];
		if (value === undefined) {
			if (isLocalStorage) {
				value = read_permanent_value(key);
				if (value !== null) {
					disk[key] = value;
					return value;
				}
			}
			return null;
		}
		return value;
	};

	var save_value = function (key, value) {
		disk[key] = value;
		if (key == 'crayondisk_next_id') permanent[key] = '' + value;
		else if (value[0]) { // directory
			var files = value[1];
			if (files.length == 0) permanent[key] = '1/';
			var output = ['1'];
			for (var filename in files) {
				var nodeId = files[filename];
				output.push(filename);
				output.push(nodeId + '');
			}
			permanent[key] = output.join('/');
		} else { // files
			var header = '0' + // 0 indicates files
				(value[1] ? '1' : '0') + // is binary
				value[2] + '@'; // file size
			while (header.length < 14) { // pad to 14 characters
				header += '*';
			}
			permanent[key] = header + value[3];
		}
	};

	var nextId = read_value('crayondisk_next_id');
	var rootDisk = read_value('crayondisk:0');
	if (rootDisk == null || nextId === null) {
		clearCorruptedDisk();
		save_value('crayondisk_next_id', 1);
		save_value('crayondisk:0', [true, {}]);

	}

	var allocate_id = function () {
		var next_id = read_value('crayondisk_next_id');
		save_value('crayondisk_next_id', next_id + 1);
		return next_id;
	};

	// Simple cache to convert paths into node IDs. Do not persist. Clear liberally on state changes.
	var cache = { '/': 0 };

	var create_file_node = function (content) {
		var id = allocate_id();
		save_value('crayondisk:' + id, [false, false, content.length, content]);
		return id;
	};

	var create_folder_node = function () {
		var id = allocate_id();
		save_value('crayondisk:' + id, [true, {}]);
		return id;
	};

	var canonicalize_path = function (path) {
		path = path.replace('\\', '/');
		if (path == '/' || path.length == 0) return '/';
		if (path[0] != '/') path = '/' + path;
		if (path[path.length - 1] == '/') path = path.substr(0, path.length - 1);
		return path;
	};

	var get_parent_and_file = function (path) {
		if (path.length < 2) return null;
		var lastSlash = path.lastIndexOf('/');
		if (lastSlash == 0) return ['/', path.substr(1)];
		return [path.substr(0, lastSlash), path.substr(lastSlash + 1)];
	};

	var convert_path_to_node_id_impl = function (path) {

		var node_id = cache[path];
		if (node_id) return node_id;

		if (path.length < 2) {
			cache[path] = 0;
			return 0; // could be empty from level_ups, otherwise we know the first char is / so <2 is always root.
		}

		var t = get_parent_and_file(path);
		var level_up = t[0];
		var this_file = t[1];
		var parent_id = convert_path_to_node_id_impl(level_up);

		if (parent_id === null) {
			cache[path] = null;
			return null;
		}

		var directory = read_value('crayondisk:' + parent_id);
		if (!directory[0]) {
			cache[path] = null;
			return null;
		}

		var child_id = directory[1][this_file];
		if (child_id !== undefined) {
			cache[path] = child_id;
			return child_id;
		}

		return null;
	};

	var convert_path_to_node_id = function (path) {
		var id = cache[path];
		if (id === undefined) {
			cpath = canonicalize_path(path);
			id = convert_path_to_node_id_impl(cpath);
			cache[path] = id;
			cache[cpath] = id;
		}
		return id;
	};

	var get_node = function (path) {
		var node_id = convert_path_to_node_id(path);
		if (node_id == null) return null;
		return read_value('crayondisk:' + node_id);
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
		var parent_node_id = cache[dir];
		if (node == null) return 1; // parent path doesn't exist.
		if (!node[0]) return 1; // parent path is a file.
		var existing_id = node[1][file];
		if (existing_id === undefined) {
			existing_id = create_file_node(text);
			node[1][file] = existing_id;
			save_value('crayondisk:' + parent_node_id, node);
			return 0;
		} else {
			var existing_node = read_value('crayondisk:' + existing_id);
			if (existing_node) {
				if (existing_node[0]) return 3;
				existing_node[2] = text.length;
				existing_node[3] = text;
				save_value('crayondisk:' + existing_id, existing_node);
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
	};

	return fakeDisk;
};
