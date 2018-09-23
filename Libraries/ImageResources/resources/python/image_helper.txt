
def libhelper_imageresources_getImageResourceManifestString():
  manifest = ResourceReader_readTextFile('res/image_sheet_manifest.txt')
  return manifest

def libhelper_imageresources_imageLoadAsync(filename, nativeImageDataNativeData, imageLoaderNativeData):
  # TODO: do this for real instead of faking it. Also see TODO in _checkLoaderIsDone.

  loaded = libhelper_imageresources_imageLoadSync(filename, nativeImageDataNativeData)
  imageLoaderNativeData[2] = 1 if loaded else 2

def libhelper_imageresources_imageLoadSync(filename, nativeImageDataNativeData):
  img = ResourceReader_readImageFile('res/images/' + filename)
  if img != None:
    nativeImageDataNativeData[0] = img
    nativeImageDataNativeData[1] = img.get_width()
    nativeImageDataNativeData[2] = img.get_height()
    return True
  return False

def libhelper_imageresources_checkLoaderIsDone(imageLoaderNativeData, nativeImageDataNativeData):
  # TODO: this will have to have mutex locking when _imageLoadAsync is implemented for real.
  return imageLoaderNativeData[2]
  
# These are actually Graphics2D functions, not imageresource functions, so they'll have to move eventually.

def libhelper_imageresources_generateNativeBitmapOfSize(w, h):
  pg = GetPyGameReferenceWorkaround()
  return pg.Surface((w, h), pg.SRCALPHA)

def libhelper_imageresources_imageResourceBlitImage(target, source, targetX, targetY, sourceX, sourceY, width, height):
  target.blit(source, (targetX, targetY, width, height), area = (sourceX, sourceY, width, height))
