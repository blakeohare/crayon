
def libhelper_imageresources_flipTexture_impl(image, flipH, flipV):
  pg = TranslationHelper_globals['LIB:GAME:pygame']
  return pg.transform.flip(image, flipH, flipV)
TranslationHelper_globals['LIB:GAME:g2dflip'] = libhelper_imageresources_flipTexture_impl
