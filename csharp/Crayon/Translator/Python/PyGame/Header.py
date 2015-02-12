import pygame
import os
import math
import time
import random

_global_vars = {
	'width': 400,
	'height': 300,
	'fps': 60,
	'clock': pygame.time.Clock()
}

KEY_LOOKUP = {
	pygame.K_LEFT: 'left',
	pygame.K_RIGHT: 'right',
	pygame.K_UP: 'up',
	pygame.K_DOWN: 'down',
	pygame.K_SPACE: 'space',
	pygame.K_RETURN: 'enter',
	pygame.K_TAB: 'tab',
}

for i in range(26):
	KEY_LOOKUP[pygame.K_a + i] = chr(ord('a') + i)
for i in range(10):
	KEY_LOOKUP[pygame.K_0 + i] = chr(ord('0') + i)
for i in range(12):
	KEY_LOOKUP[pygame.K_F1 + i] = 'f' + str(i + 1)

COMMON_STRINGS = {}
for value in list(KEY_LOOKUP.values()) + 'exit closebutton key alt-f4 mousemove mouseleftdown mouseleftup mouserightdown mouserightup'.split(' '):
	COMMON_STRINGS[value] = [%%%TYPE_STRING%%%, value]

def _pygame_pump_events():
	evs = pygame.event.get()
	pressed_keys = pygame.key.get_pressed()
	evlist = []
	rwidth,rheight = _global_vars['real_screen'].get_size()
	vwidth,vheight = _global_vars['virtual_screen'].get_size()

	for ev in evs:
		if ev.type == pygame.KEYDOWN or ev.type == pygame.KEYUP:
			if KEY_LOOKUP.get(ev.key) != None:
				evlist.append([%%%TYPE_LIST%%%, [COMMON_STRINGS['key'], v_VALUE_TRUE if (ev.type == pygame.KEYDOWN) else v_VALUE_FALSE, COMMON_STRINGS[KEY_LOOKUP.get(ev.key)]]])
			if ev.type == pygame.KEYDOWN and ev.key == pygame.K_F4:
				if pressed_keys[pygame.K_LALT] or pressed_keys[pygame.K_RALT]:
					evlist.append([%%%TYPE_LIST%%%, [COMMON_STRINGS['exit'], COMMON_STRINGS['alt-f4']]])
		elif ev.type == pygame.QUIT:
			evlist.append([%%%TYPE_LIST%%%, [COMMON_STRINGS['exit'], COMMON_STRINGS['closebutton']]])
		elif ev.type == pygame.MOUSEBUTTONDOWN or ev.type == pygame.MOUSEBUTTONUP:
			x, y = ev.pos
			right = ev.button == 3
			down = ev.type == pygame.MOUSEBUTTONDOWN
			type = COMMON_STRINGS['mouse' + ('right' if right else 'left') + ('down' if down else 'up')]
			evlist.append([%%%TYPE_LIST%%%, [type, [%%%TYPE_INTEGER%%%, x * vwidth // rwidth], [%%%TYPE_INTEGER%%%, y * vheight // rheight]]])
		elif ev.type == pygame.MOUSEMOTION:
			x, y = ev.pos
			type = COMMON_STRINGS['mousemove']
			evlist.append([%%%TYPE_LIST%%%, [type, [%%%TYPE_INTEGER%%%, x * vwidth // rwidth], [%%%TYPE_INTEGER%%%, y * vheight // rheight]]])
		
	return evlist

def platform_begin(fps):
	pygame.init()
	_global_vars['fps'] = fps

def _pygame_initialize_screen(width, height, pixel_dimensions):
	_global_vars['width'] = width
	_global_vars['height'] = height
	scaled_mode = pixel_dimensions != None
	if scaled_mode:
		real_screen = pygame.display.set_mode(pixel_dimensions)
		virtual_screen = pygame.Surface((width, height)).convert()
	else:
		virtual_screen = pygame.display.set_mode((_global_vars['width'], _global_vars['height']))
		real_screen = virtual_screen
	_global_vars['real_screen'] = real_screen
	_global_vars['virtual_screen'] = virtual_screen
	_global_vars['scaled_mode'] = scaled_mode

_PDE = pygame.draw.ellipse
_PDF = pygame.display.flip
_PDL = pygame.draw.line
_PDR = pygame.draw.rect
_PR = pygame.Rect

_images_downloaded = {}

def create_assertion(message):
	raise Exception(message)

def create_sorted_copy_of_list(items):
	items = items[:]
	items.sort()
	return items

def flush_imagette(imagette):
	width, height, images, xs, ys = imagette
	output = pygame.Surface((width, height))
	for i in range(len(images)):
		output.blit(images[i], (xs[i], ys[i]))
	return output

def load_local_image_resource(path):
	path = path.replace('/', os.sep);
	if not os.path.exists(path): return None
	try:
		img = pygame.image.load(path)
		_images_downloaded[key] = img
		return img
	except:
		return None

def load_local_tile_resource(genName):
	return load_local_image_resource(os.path.join('_generated_files', 'spritesheets', genName + '.png'))


def get_image_impl(key):
	surf = _images_downloaded.get(key, None)
	if surf == None: return None
	return (%%%TYPE_NATIVE_OBJECT_IMAGE%%%, surf)

def wrappedChr(code):
	if code < 0 or code > 255: return '?'
	return chr(code)

_NUM_CHARS = {}
for c in '0123456789':
	_NUM_CHARS[c] = True

def _is_valid_integer(value):
	first = True
	if value == '-': return False
	for c in value:
		if first:
			first = False
			if c == '-':
				continue
		if not _NUM_CHARS.get(c, False):
			return False
	return True

def _pygame_end_of_frame():
	if _global_vars['scaled_mode']:
		vs = _global_vars['virtual_screen']
		rs = _global_vars['real_screen']
		pygame.transform.scale(vs, rs.get_size(), rs)
	pygame.display.flip()
	_global_vars['clock'].tick(_global_vars['fps'])

def _pygame_flip_image(img, flipx, flipy):
	if img[0] != %%%TYPE_NATIVE_OBJECT_IMAGE%%%:
		return None
	image = img[1]
	output = pygame.transform.flip(image, flipx == True, flipy == True)
	return (%%%TYPE_NATIVE_OBJECT_IMAGE%%%, output)

def _read_resource_text(path):
	if os.path.exists(path):
		if not os.path.isdir(path):
			f = open(path, 'rt')
			text = f.read()
			f.close()
			return text
	return None

def _parse_json(raw):
	import json
	try:
		return _parse_json_thing(json.loads(raw))
	except:
		return None

def _parse_json_thing(item):
	if item == None: return v_VALUE_NULL
	if item == True: return v_VALUE_TRUE
	if item == False: return v_VALUE_FALSE
	if item == "": return v_VALUE_EMPTY_STRING
	t = str(type(item))
	if "'int'" in t or "'long'" in t:
		return v_build_integer(item)
	if "'float'" in t:
		return [%%%TYPE_FLOAT%%%, item];
	if "'string'" in t or "'unicode'" in t:
		return [%%%TYPE_STRING%%%, str(item)]
	if "'list'" in t:
		output = []
		for o in item:
			output.append(_parse_json_thing(o))
		return [%%%TYPE_LIST%%%, output]
	if "'dict'" in t:
		keys = []
		values = []
		for key in item.keys():
			keys.append(key)
			values.append(_parse_json_thing(item[key]))
		return v_buildDictionary(keys, values);
	return v_VALUE_NULL;

program_data = [None]
