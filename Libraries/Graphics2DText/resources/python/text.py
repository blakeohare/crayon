

def lib_graphics2dtext_createNativeFont(fontType, fontClass, fontPath, fontSize, isBold, isItalic):
	if fontType == 0: # default
		raise Exception("Not implemented: default font by class")
	elif fontType == 1: # resource
		raise Exception("Not implemented: font embedded resource")
	elif fontType == 2: # file system
		raise Exception("Not implemented: font from file system")
	elif fontType == 3: # system
		return pygame.font.SysFont(fontPath, fontSize, isBold, isItalic)

def lib_graphics2dtext_name_canonicalizer(name):
	output = []
	a = ord('a')
	z = ord('z')
	for char in name.lower():
		if a <= ord(char) <= z:
			output.append(char)
	return ''.join(output)

def lib_graphics2dtext_isSystemFontAvailable(name):
	return lib_graphics2dtext_name_canonicalizer(name) in pygame.font.get_fonts()

def lib_graphics2dtext_renderText(sizeOut, nativeFont, red, green, blue, text):
	surface = nativeFont.render(text, False, (red, green, blue))
	width, height = surface.get_size()
	sizeOut[0] = width
	sizeOut[1] = height
	return surface
