
def lib_imageencoder_encodeImage(image, format, output, bytesAsValues):
	if format == 1:
		ext = '.png'
	elif format == 2:
		ext = '.jpg'

	path = 'tmp-img-encode' + ext
	
	pygame.image.save(image, path)
	
	c = open(path, 'rb')
	try:
		b = c.read(1)
		while b != "":
			output.append(bytesAsValues[ord(b)])
			b = c.read(1)
	finally:
		c.close()
		os.remove(path)
	
	return 0
	