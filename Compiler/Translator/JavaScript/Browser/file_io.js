
R.readResourceText = function (path) {
	var output = R.resources[path];
	if (output === undefined) return null;
	return output;
};

R.IO = {};

R.IO.virtualDisk = null;
R.IO.userData = null;
R.IO.get_disk = function (isUserData) {
	if (isUserData) {
		if (R.IO.userData === null) {
			try {
				if ('localStorage' in window && window['localStorage'] !== null) {
					R.IO.userData = createFakeDisk(localStorage);
				}
			} catch (e) { }
			if (R.IO.userData == null) R.IO.userData = createFakeDisk(null);
		}
		return R.IO.userData;
	} else {
		if (R.IO.virtualDisk === null) {
			R.IO.virtualDisk = createFakeDisk(null);
		}
		return R.IO.virtualDisk;
	}
};

R.IO.checkPath = function (path, isDir, isUserData) {
	disk = R.IO.get_disk(isUserData);
	if (isDir) {
		return disk.is_directory(path);
	} else {
		return disk.path_exists(path);
	}
	return false;
};

R.IO.listFiles = function (path, isUserData) {
	disk = R.IO.get_disk(isUserData);
	return disk.list_dir(path);
};

R.IO.readFile = function (path, isUserData) {
	disk = R.IO.get_disk(isUserData);
	return disk.read_file(path);
};

R.IO.writeFile = function (path, content, isUserData) {
	disk = R.IO.get_disk(isUserData);
	return disk.write_text(path, content);
};
