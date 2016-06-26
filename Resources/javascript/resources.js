CRAYON.resources = {};
CRAYON.resourceDirs = {};
CRAYON.getTextResource = function (path) {
    output = CRAYON.resources[path];
    if (!!output) return output;
    return null;
};

CRAYON.addResource = function (path, value) {
    CRAYON.resources[path] = value;
    CRAYON.addResourceToPathLookup(path);
};

CRAYON.addBinaryResource = function (path) {
    CRAYON.addResource(path, null);
};

CRAYON.addResourceToPathLookup = function(path) {
    var parts = path.split('/');
    var builder = [];
    for (var i = 0; i < parts.length - 1; ++i) {
        builder.push(parts[i]);
        var partialPath = builder.join('/');
        var next = parts[i + 1];
        var lookup = CRAYON.resourceDirs[partialPath];
        if (!lookup) {
            lookup = [];
            CRAYON.resourceDirs[partialPath] = lookup;
        }
        lookup.push(next);
    }
};

CRAYON.populateResources = function () {
    var car = CRAYON.addResource;
    var cabr = CRAYON.addBinaryResource;
    %%%JS_RESOURCE_LIST%%%
    %%%JS_BINARY_RESOURCE_LIST%%%
};

CRAYON.populateResources();
