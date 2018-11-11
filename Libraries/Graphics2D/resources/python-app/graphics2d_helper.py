
def libhelper_imageresources_flipTexture(image, flipH, flipV):
  pg = TranslationHelper_globals['LIB:GAME:pygame']
  return pg.transform.flip(image, flipH, flipV)
