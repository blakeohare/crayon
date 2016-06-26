
class ResourceReader:

	def getManifest(self):
		output = self.readTextFile("resources/manifest.txt")
		if output == None: return ''
		return output

	def readTextFile(self, path):
		path = self.canonicalizeAndVerifyPath(path)
		if path == None: return None
		file = open(path, 'rt')
		output = file.read()
		file.close()

		if output[:3] == "\xef\xbb\xbf":
			output = output[3:]

		return output

	def readImageFile(self, path):
		path = self.canonicalizeAndVerifyPath(path)
		if path == None: return None
		return pygame.image.load(path)

	def canonicalizeAndVerifyPath(self, path):
		path = path.replace('\\', '/').replace('/', os.sep)
		if os.path.exists(path):
			return path
		return None

RESOURCES = ResourceReader()
