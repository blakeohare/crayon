import pygame
import os
import math
import time
import random
import sys
import shutil
import threading

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
	pygame.K_ESCAPE: 'escape',
	pygame.K_TAB: 'tab',
	pygame.K_PAGEUP: 'pageup',
	pygame.K_PAGEDOWN: 'pagedown',
	pygame.K_HOME: 'home',
	pygame.K_END: 'end',
	pygame.K_DELETE: 'delete',
	pygame.K_INSERT: 'insert',
	pygame.K_COMMA: 'comma',
	pygame.K_PERIOD: 'period',
	pygame.K_SEMICOLON: 'semicolon',
	pygame.K_SLASH: 'slash',
	pygame.K_QUOTE: 'apostrophe',
	pygame.K_BACKSPACE: 'backspace',
	pygame.K_BACKSLASH: 'backslash',
	pygame.K_LEFTBRACKET: 'openbracket',
	pygame.K_RIGHTBRACKET: 'closebracket',
	pygame.K_LCTRL: 'ctrl',
	pygame.K_RCTRL: 'ctrl',
	pygame.K_LSHIFT: 'shift',
	pygame.K_RSHIFT: 'shift',
	pygame.K_LALT: 'alt',
	pygame.K_RALT: 'alt'
}

for i in range(26):
	KEY_LOOKUP[pygame.K_a + i] = chr(ord('a') + i)
for i in range(10):
	KEY_LOOKUP[pygame.K_0 + i] = chr(ord('0') + i)
for i in range(12):
	KEY_LOOKUP[pygame.K_F1 + i] = 'f' + str(i + 1)

def _pygame_pump_events():
	evs = pygame.event.get()
	pressed_keys = pygame.key.get_pressed()
	evlist = []
	rwidth,rheight = _global_vars['real_screen'].get_size()
	vwidth,vheight = _global_vars['virtual_screen'].get_size()

	for ev in evs:
		if ev.type == pygame.KEYDOWN or ev.type == pygame.KEYUP:
			if KEY_LOOKUP.get(ev.key) != None:
				keycode = KEY_LOOKUP.get(ev.key)
				down = ev.type == pygame.KEYDOWN
				evlist.append(v_buildGameEvent('keydown' if down else 'keyup', 'key', 0, 0, 0, down, keycode))
			if ev.type == pygame.KEYDOWN and ev.key == pygame.K_F4:
				if pressed_keys[pygame.K_LALT] or pressed_keys[pygame.K_RALT]:
					evlist.append(v_buildGameEvent('quit-altf4', 'quit', 0, 0, 0, False, None))
		elif ev.type == pygame.QUIT:
			evlist.append(v_buildGameEvent('quit-closebutton', 'quit', 0, 0, 0, False, None))
		elif ev.type == pygame.MOUSEBUTTONDOWN or ev.type == pygame.MOUSEBUTTONUP:
			x, y = ev.pos
			x = x * vwidth // rwidth
			y = y * vheight // rheight
			right = ev.button == 3
			down = ev.type == pygame.MOUSEBUTTONDOWN
			if down:
				if right:
					type = 'mouserightdown'
				else:
					type = 'mouseleftdown'
			else:
				if right:
					type = 'mouserightup'
				else:
					type = 'mouseleftup'
			evlist.append(v_buildGameEvent(type, 'mouse', x, y, 0, False, None))
		elif ev.type == pygame.MOUSEMOTION:
			x, y = ev.pos
			x = x * vwidth // rwidth
			y = y * vheight // rheight
			evlist.append(v_buildGameEvent('mousemove', 'mouse', x, y, 0, False, None))
		
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
	output = pygame.Surface((width, height), pygame.SRCALPHA)
	for i in range(len(images)):
		output.blit(images[i], (xs[i], ys[i]))
	return output

def load_local_image_resource(path):
	path = path.replace('/', os.sep);
	if not os.path.exists(path): return None
	try:
		return pygame.image.load(path)
	except:
		return None

def load_local_tile_resource(genName):
	return load_local_image_resource(os.path.join('_generated_files', 'spritesheets', genName + '.png'))

def get_image_impl(key):
	surf = _images_downloaded.get(key, None)
	if surf == None: return None
	return (%%%TYPE_NATIVE_OBJECT_IMAGE%%%, surf)

def blit_partial(surface, targetX, targetY, targetWidth, targetHeight, sourceX, sourceY, sourceWidth, sourceHeight):
	if sourceWidth == targetWidth and sourceHeight == targetHeight:
		_global_vars['virtual_screen'].blit(surface, (targetX, targetY), _PR(sourceX, sourceY, sourceWidth, sourceHeight))
	else:
		# PyGame makes me sad.
		scale_x = 1.0 * targetWidth / sourceWidth
		scale_y = 1.0 * targetHeight / sourceHeight
		original_width, original_height = surface.get_size()
		calculated_full_width = int(original_width * scale_x)
		calculated_full_height = int(original_height * scale_y)
		t_surf = get_temporary_image(calculated_full_width, calculated_full_height)
		pygame.transform.scale(surface, (calculated_full_width, calculated_full_height), t_surf)
		source_x_scaled = int(sourceX * scale_x)
		source_y_scaled = int(sourceY * scale_y)
		_global_vars['virtual_screen'].blit(t_surf, (targetX, targetY), _PR(source_x_scaled, source_y_scaled, targetWidth, targetHeight))

_temp_image_cache = {}
def get_temporary_image(width, height):
	key = width * 1000000000 + height
	img = _temp_image_cache.get(key, None)
	if img == None:
		img = pygame.Surface((width, height)).convert_alpha()
		_temp_image_cache[key] = img
	return img

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
		vs.fill((0, 0, 0))
	else:
		pygame.display.flip()
		_global_vars['real_screen'].fill((0, 0, 0))
	_global_vars['clock'].tick(_global_vars['fps'])

def _pygame_flip_image(img, flipx, flipy):
	return pygame.transform.flip(img, flipx, flipy)

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
		return v_buildInteger(item)
	if "'float'" in t:
		return [%%%TYPE_ID_FLOAT%%%, item];
	if "'string'" in t or "'unicode'" in t:
		return [%%%TYPE_ID_STRING%%%, str(item)]
	if "'list'" in t:
		output = []
		for o in item:
			output.append(_parse_json_thing(o))
		return [%%%TYPE_ID_LIST%%%, output]
	if "'dict'" in t:
		keys = []
		values = []
		for key in item.keys():
			keys.append(key)
			values.append(_parse_json_thing(item[key]))
		return v_buildDictionary(keys, values);
	return v_VALUE_NULL;

def _sort_helper(x):
	return x[1]
def sort_primitive_value_list(values, ignoredType):
	values.sort(key=_sort_helper)

_TEMP_SURFACE = [None]
def draw_rectangle(x, y, width, height, r, g, b, a):
	# Pygame has a bug where it doesn't draw rectangles if the height is 1.
	if a == 255 and height > 1:
		_PDR(_global_vars['virtual_screen'], (r, g, b), _PR(x, y, width, height))
	elif a > 0:
		ts = _TEMP_SURFACE[0]
		if ts == None:
			ts = pygame.Surface(_global_vars['virtual_screen'].get_size()).convert()
		ts.fill((r, g, b))
		ts.set_alpha(a)
		_global_vars['virtual_screen'].blit(ts, (x, y), _PR(0, 0, width, height))

def readLocalSoundResource(path):
	path = path.replace('/', os.sep)
	if os.path.exists(path):
		try:
			snd = pygame.mixer.Sound(path)
		except:
			return None
		return v_instantiateSoundInstance(snd, False, -1)
	return None

def playSoundImpl(snd):
	snd[0].play()

def io_helper_check_path(path, isDirCheck, checkCase):
	# TODO: check case.
	if os.path.exists(path):
		return (not isDirCheck) or os.path.isdir(path)
	return False

def io_helper_write_text(path, contents):
	try:
		c = open(path, 'wt')
		c.write(contents)
		c.close()
		return 0
	except:
		# TODO: more specific error cases
		return 4

def io_helper_read_text(path):
	try:
		c = open(path, 'rt')
		output = c.read()
		c.close()
		return output
	except:
		return None

def io_helper_current_directory():
	return os.path.dirname(os.path.realpath(__file__))

def io_create_directory(d):
	try:
		os.mkdir(d)
		return %%%IO_ERROR_NONE%%%
	except OSError:
		return %%%IO_ERROR_UNKNOWN_ERROR%%%

def io_delete_file(path):
	try:
		os.remove(path)
		return %%%IO_ERROR_NONE%%%
	except:
		return %%%IO_ERROR_UNKNOWN_ERROR%%%

def io_delete_directory(d, recurse):
	if recurse:
		try:
			shutil.rmtree(d)
		except:
			return %%%IO_ERROR_UNKNOWN_ERROR%%%
	else:
		try:
			os.rmdir(d)
		except:
			return %%%IO_ERROR_UNKNOWN_ERROR%%%
	return %%%IO_ERROR_NONE%%%

_async_message_queue_mutex = threading.Lock()
_async_message_queue = []
_async_message_queue_count = 0

def _push_async_message_queue(object_array):
	global _async_message_queue_count, _async_message_queue
	try:
		_async_message_queue_mutex.acquire()
		_async_message_queue_count += 1
		# Using += to append. Most frames will have 0 or 1 async messages, so this is generally optimal.
		_async_message_queue += object_array
	finally:
		_async_message_queue_mutex.release()

_list_of_zero = [0]
def _pump_async_message_queue():
	global _async_message_queue_count, _async_message_queue
	try:
		_async_message_queue_mutex.acquire()
		if _async_message_queue_count == 0: return _list_of_zero
		output = [_async_message_queue_count] + _async_message_queue
		_async_message_queue = []
		_async_message_queue_count = 0
		return output
	finally:
		_async_message_queue_mutex.release()

def _http_request_impl(request_object, method, url, body, userAgent, contentType, contentLength, headerNames, headerValues):
	request = HttpAsyncRequest(request_object, url)
	request.set_method(method)
	request.set_header('User-Agent', userAgent)
	if body != None:
		request.set_content(body)
		request.set_header('Content-Type', contentType)
	for i in range(len(headerNames)):
		request.add_header(headerNames[i], headerValues[i])

	request.send()

_app_data_root = [None]
def get_app_data_root():
	adr = _app_data_root[0]
	if adr == None:
		if os.name == 'nt':
			adr = os.environ['APPDATA'].replace('\\', '/') + '/' + '%%%PROJECT_ID%%%'
		else:
			adr = '~/.' + '%%%PROJECT_ID%%%'
		_app_data_root[0] = adr
	return adr

program_data = [None]
