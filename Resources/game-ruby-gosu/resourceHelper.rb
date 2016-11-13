
def resourceHelper_getManifest
	return File.open(File.join("resources", "manifest.txt"), "rb").read
end

def resourceHelper_getByteCodeString
	return File.open(File.join("resources", "byte_code.txt"), "rb").read
end
