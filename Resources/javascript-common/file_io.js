
C$io = 1;
C$io$virtualDisk = null;
C$io$virtualDisk = null;
C$io$userData = null;

C$io$getDisk = function (isUserData) {
	if (isUserData) {
	    if (C$io$userData === null) {
			try {
				if ('localStorage' in window && window['localStorage'] !== null) {
				    C$io$userData = createFakeDisk(localStorage);
				}
			} catch (e) { }
			if (C$io$userData == null) C$io$userData = createFakeDisk(null);
		}
	    return C$io$userData;
	} else {
	    if (C$io$virtualDisk === null) {
	        C$io$virtualDisk = createFakeDisk(null);
		}
	    return C$io$virtualDisk;
	}
};

C$io$checkPath = function (path, isDir, isUserData) {
    disk = C$io$getDisk(isUserData);
	if (isDir) {
		return disk.is_directory(path);
	} else {
		return disk.path_exists(path);
	}
	return false;
};

C$io$listFiles = function (path, isUserData) {
    disk = C$io$getDisk(isUserData);
	return disk.list_dir(path);
};

C$io$readFile = function (path, isUserData) {
    disk = C$io$getDisk(isUserData);
	return disk.read_file(path);
};

C$io$writeFile = function (path, content, isUserData) {
    disk = C$io$getDisk(isUserData);
	return disk.write_text(path, content);
};
