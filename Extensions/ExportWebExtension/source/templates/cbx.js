const CBX = (() => {

let cbxFiles = {};

let addBundle = (filename, cbxBundle) => {
    cbxFiles[filename] = cbxBundle;
};

let getBundle = (filename) => {
    return cbxFiles[filename] || null;
};

let createCbxBundleBuilder = () => {
    let files = {};
    let metadata = {};
    let obj;

    let setTextResourceLiteralImpl = (path, txt, data) => {
        data[path] = {
            type: 'TXT',
            value: txt,
        };
        return obj;
    };
    let setTextResourceLiteral = (path, txt) => setTextResourceLiteralImpl(path, txt, files);
    let setMetadata = (type, txt) => setTextResourceLiteralImpl(type, txt, metadata);
    let setBinaryResourceLiteral = (path, b64) => {
        files[path] = {
            type: 'B64',
            value: b64,
        };
        return obj;
    };
    let setImageResourceUrl = (path, url) => {
        files[path] = {
            type: 'IMGURL',
            value: url,
        };
        return obj;
    };

    let getRawTextResource = async (path, data) => {
        let file = data[path];
        if (!file) return null;
        if (file.type == 'TXT') return file.value;
        throw new Error("Not implemented");
    };
    let getTextResource = path => getRawTextResource(path, files);
    let getMetadata = type => getRawTextResource(type, metadata);

    let getBinaryResource = async (path, asB64) => {
        throw new Error("Not implemented");
    };
    let getImageResource = async (path) => {
        let imgCanvasCb = null;
        let p = new Promise(res => {
            imgCanvasCb = res;
        });
        let img = document.createElement('img');
        img.addEventListener('load', () => {
            let w = img.width;
            let h = img.height;
            let canvas = document.createElement('canvas');
            canvas.width = w;
            canvas.height = h;
            let ctx = canvas.getContext('2d');
            ctx.drawImage(img, 0, 0);
            imgCanvasCb(canvas);
        });
        let prefix = metadata['jsPrefix'] || '';
        let url = path;
        if (!prefix === '' && path.charAt(0) !== '/') {
            while (prefix && prefix.charAt(prefix.length - 1) == '/') prefix = prefix.substr(0, prefix.length - 1);
            while (path && path.charAt(0) === '/') path = path.substr(1);
            url = prefix + '/' + path;
            if (url.charAt(0) !== '/') url = '/' + url;
        }
        img.src = url;
        return p;
    };

    obj = {
        getMetadata,
        setMetadata,
        getTextResource,
        setTextResourceLiteral,
        getBinaryResource,
        setBinaryResourceLiteral,
        getImageResource,
        setImageResourceUrl,
        finalize: () => {
            return {
                getByteCode: () => getMetadata('bytecode'),
                getImageManifest: () => getMetadata('imgManifest'),
                getResourceManifest: () => getMetadata('resManifest'),
                getTextResource,
                getBinaryResource,
                getImageResource,
            };
        },
    };
    return obj;
};

return {
    createCbxBundleBuilder,
    addBundle,
    getBundle,
};

})();
