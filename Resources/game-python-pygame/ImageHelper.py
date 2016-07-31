
def _getImageResourceManifestString():
	return RESOURCES.readTextFile('resources/image_sheet_manifest.txt')

def _imageLoadAsync(filename, nativeImageDataNativeData, imageLoaderNativeData):
	# TODO: do this for real instead of faking it. Also see TODO in _checkLoaderIsDone.

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

def _checkLoaderIsDone(imageLoaderNativeData, nativeImageDataNativeData, output):
	# TODO: this will have to have mutex locking when _imageLoadAsync is implemented for real.
	isDone = imageLoaderNativeData[2] != 0
	output[0] = v_VALUE_TRUE if isDone else v_VALUE_FALSE

def _generateNativeBitmapOfSize(w, h):
	return pygame.Surface((w, h)).convert_alpha()

def _imageResourceBlitImage(target, source, targetX, targetY, sourceX, sourceY, width, height):
	target.blit(source, (targetX, targetY, width, height), area = (sourceX, sourceY, width, height))

def _flipTexture(image, flipH, flipV):
	return pygame.transform.flip(image, flipH, flipV)
