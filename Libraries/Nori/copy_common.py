import os
import shutil

SOURCE_FILE = 'common/nori.js'
TARGET_LOCATIONS = [
	'native/csharp-app/source/TextResources/nori.js',
	'native/javascript-app/source/nori.js',
]

def main():
	contents = []
	for target in TARGET_LOCATIONS:
		if os.path.exists(target):
			c = open(target, 'rt')
			contents.append(c.read())
			c.close()
	
	for content in contents[1:]:
		if content != contents[0]:
			print("Looks like some of the nori.js files have specific changes in them. You probably don't want to overwrite them. Revert the changes or delete the different files and re-run.")
			return
	
	for target in TARGET_LOCATIONS:
		if os.path.exists(target):
			os.remove(target)
		shutil.copyfile(SOURCE_FILE, target)

main()
