

def lib_graphics2dtext_createNativeFont(fontType, fontClass, fontPath, fontSize, isBold, isItalic):
	fontSize = fontSize * 7 / 5 # adjust to match standard sizes
	pygame = GetPyGameReferenceWorkaround()
	if fontType == 0: # default
		raise Exception("Not implemented: default font by class")
	elif fontType == 1: # resource
		font = pygame.font.Font(os.path.join('res', 'ttf', fontPath), fontSize)
		font.set_bold(isBold)
		font.set_italic(isItalic)
		return font
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
	pygame = GetPyGameReferenceWorkaround()
	return lib_graphics2dtext_name_canonicalizer(name) in pygame.font.get_fonts()

def lib_graphics2dtext_renderText(sizeOut, nativeFont, red, green, blue, text):
	surface = nativeFont.render(text, False, (red, green, blue))
	width, height = surface.get_size()
	side_margin = height // 32
	top_margin = height // 8 + 4
	bottom_margin = 4
	pygame = GetPyGameReferenceWorkaround()
	padded = pygame.Surface((width + side_margin * 2, height + bottom_margin + top_margin), pygame.SRCALPHA)
	padded.blit(surface, (side_margin, top_margin))
	width, height = padded.get_size()
	sizeOut[0] = width
	sizeOut[1] = height
	return padded
