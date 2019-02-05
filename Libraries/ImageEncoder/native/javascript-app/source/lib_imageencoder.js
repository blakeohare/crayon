
var LIB$imageencoder$encodeImage = function(image, format, output, bytesAsValues) {
	var mime = "image/png";
	if (format == 2) mime = "image/jpeg";

	var dataUrl = image.toDataURL(mime);
	var b64 = dataUrl.substring(dataUrl.indexOf(',') + 1, dataUrl.length);
	var lookup = {};
	var alpha = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';
	var i;
	for (i = 0; i < 64; ++i) {
		lookup[alpha.charAt(i)] = [(i >> 4) & 3, (i >> 2) & 3, i & 3];
	}
	lookup['='] = [];
	var bits = [];
	var length = b64.length;
	var triple;
	var j;
	for (i = 0; i < length; ++i) {
		triple = lookup[b64.charAt(i)];
		for (j = 0; j < triple.length; ++j) {
			bits.push(triple[j]);
		}
	}
	while (bits.length % 4 != 0) bits.pop();
	var byteValue = 0;
	
	i = 0;
	while (i < bits.length) {
		byteValue = (bits[i++] << 6) | (bits[i++] << 4) | (bits[i++] << 2) | bits[i++];
		output.push(bytesAsValues[byteValue]);
	}
	return 0;
}