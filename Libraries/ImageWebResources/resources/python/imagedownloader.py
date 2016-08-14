def lib_imagewebresources_imagedownloader_getRandomFile():
	import random
	letters = 'abcdefghijklmnopqrstuvwxyz'
	letters += letters.upper() + '0123456789'
	chars = list(letters)
	filename = ''
	for i in range(15):
		filename += random.choice(chars)
	filename += '.png'
	return filename

def lib_imagewebresources_imagedownloader_bytesToImage(bytes, output):
	try:
		isPng = False
		if len(bytes) > 4 and bytes[1] == ord('P') and bytes[2] == ord('n') and bytes[3] == ord('g'):
			isPng = True
		file = lib_imagewebresources_imagedownloader_getRandomFile() + ('.png' if isPng else '.jpg')
		import array
		bytestring = array.array('B', bytes).tostring()
		c = open(file, 'wb')
		c.write(bytestring)
		c.close()
		
		import pygame
		image = pygame.image.load(file)
		
		import os
		if os.path.exists(file):
			os.remove(file)
		
		output[0] = image
		output[1] = image.get_width()
		output[2] = image.get_height()
		
		return True
	except:
		return False
	
