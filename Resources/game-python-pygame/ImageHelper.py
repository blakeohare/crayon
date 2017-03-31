
def libhelper_imageresources_getImageResourceManifestString():
  return RESOURCES.readTextFile('resources/image_sheet_manifest.txt')

def libhelper_imageresources_imageLoadAsync(filename, nativeImageDataNativeData, imageLoaderNativeData):
  # TODO: do this for real instead of faking it. Also see TODO in _checkLoaderIsDone.

  loaded = _imageLoadSync(filename, nativeImageDataNativeData, None)
  imageLoaderNativeData[2] = 1 if loaded else 2

def libhelper_imageresources_imageLoadSync(filename, nativeImageDataNativeData, statusOutCheesy):
  img = RESOURCES.readImageFile('resources/images/' + filename)
  if img != None:
    if statusOutCheesy != None:
      statusOutCheesy[0] = statusOutCheesy[-1]
    nativeImageDataNativeData[0] = img
    nativeImageDataNativeData[1] = img.get_width()
    nativeImageDataNativeData[2] = img.get_height()
    return True
  return False

def libhelper_imageresources_checkLoaderIsDone(imageLoaderNativeData, nativeImageDataNativeData, output):
  # TODO: this will have to have mutex locking when _imageLoadAsync is implemented for real.
  output[0] = v_buildInteger(imageLoaderNativeData[2])

def libhelper_imageresources_generateNativeBitmapOfSize(w, h):
  return pygame.Surface((w, h), pygame.SRCALPHA)

def libhelper_imageresources_imageResourceBlitImage(target, source, targetX, targetY, sourceX, sourceY, width, height):
  target.blit(source, (targetX, targetY, width, height), area = (sourceX, sourceY, width, height))

def libhelper_imageresources_flipTexture(image, flipH, flipV):
  return pygame.transform.flip(image, flipH, flipV)
