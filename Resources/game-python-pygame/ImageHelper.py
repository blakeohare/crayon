
def _getImageResourceManifestString():
	return RESOURCES.readTextFile('resources/image_sheet_manifest.txt')

def _imageLoadAsync(filename, nativeImageDataNativeData, imageLoaderNativeData):
	# TODO: do this for real instead of faking it.

	loaded = _imageLoadSync(filename, nativeImageDataNativeData, None)
	imageLoaderNativeData[2] = 1 if loaded else 2

def _imageLoadSync(filename, nativeImageDataNativeData, statusOutCheesy):
	img = RESOURCES.readImageFile('resources/images/' + filename)
	if img != None:
		if statusOutCheesy != None: 
			statusOutCheesy[0] = statusOutCheesy[-1]
		nativeImageDataNativeData[0] = img
		nativeImageDataNativeData[1] = img.get_width()
		nativeImageDataNativeData[2] = img.get_height()
		return True
	return False
