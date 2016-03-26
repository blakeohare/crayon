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
	'clock': pygame.time.Clock(),
	'old_events': [],
	'pumped': False,
}

KEY_LOOKUP = {
	pygame.K_LEFT: 37,
	pygame.K_RIGHT: 39,
	pygame.K_UP: 38,
	pygame.K_DOWN: 40,
	pygame.K_SPACE: 32,
	pygame.K_RETURN: 13,
	pygame.K_ESCAPE: 27,
	pygame.K_TAB: 9,
	pygame.K_PAGEUP: 33,
	pygame.K_PAGEDOWN: 34,
	pygame.K_HOME: 36,
	pygame.K_END: 35,
	pygame.K_DELETE: 46,
	pygame.K_INSERT: 45,
	pygame.K_COMMA: 188,
	pygame.K_PERIOD: 190,
	pygame.K_SEMICOLON: 186,
	pygame.K_SLASH: 191,
	pygame.K_QUOTE: 222,
	pygame.K_BACKQUOTE: 192,
	pygame.K_MINUS: 189,
	pygame.K_EQUALS: 187,
	pygame.K_BACKSPACE: 8,
	pygame.K_BACKSLASH: 220,
	pygame.K_LEFTBRACKET: 219,
	pygame.K_RIGHTBRACKET: 221,
	pygame.K_MENU: 93,
	pygame.K_PRINT: 44,
	pygame.K_CAPSLOCK: 20,
	pygame.K_SCROLLOCK: 145,
	pygame.K_NUMLOCK: 144,
	pygame.K_PAUSE: 19,
	pygame.K_LCTRL: 17,
	pygame.K_RCTRL: 17,
	pygame.K_LSHIFT: 16,
	pygame.K_RSHIFT: 16,
	pygame.K_LALT: 18,
	pygame.K_RALT: 18,
}

for i in range(26):
	KEY_LOOKUP[pygame.K_a + i] = 97 + i
for i in range(10):
	KEY_LOOKUP[pygame.K_0 + i] = 48 + i
for i in range(12):
	KEY_LOOKUP[pygame.K_F1 + i] = 112 + i

def _pygame_pump_events():
	evs = pygame.event.get()
	if len(_global_vars['old_events']) > 0:
		evs += _global_vars['old_events']
		_global_vars['old_events'] = []
	_global_vars['pumped'] = True
	pressed_keys = pygame.key.get_pressed()
	evlist = []
	rwidth,rheight = _global_vars['real_screen'].get_size()
	vwidth,vheight = _global_vars['virtual_screen'].get_size()

	for ev in evs:
		if ev.type == pygame.KEYDOWN or ev.type == pygame.KEYUP:
			if KEY_LOOKUP.get(ev.key) != None:
				keycode = KEY_LOOKUP.get(ev.key)
				down = ev.type == pygame.KEYDOWN
				evlist.append(v_buildRelayObj(16 if down else 17, keycode, 0, 0, 0, None))
			if ev.type == pygame.KEYDOWN and ev.key == pygame.K_F4:
				if pressed_keys[pygame.K_LALT] or pressed_keys[pygame.K_RALT]:
					evlist.append(v_buildRelayObj(1, 0, 0, 0, 0, None))
		elif ev.type == pygame.QUIT:
			evlist.append(v_buildRelayObj(1, 1, 0, 0, 0, None))
		elif ev.type == pygame.MOUSEBUTTONDOWN or ev.type == pygame.MOUSEBUTTONUP:
			x, y = ev.pos
			x = x * vwidth // rwidth
			y = y * vheight // rheight
			right = ev.button == 3
			down = ev.type == pygame.MOUSEBUTTONDOWN
			type = 33
			if not down: type += 1
			if right: type += 2
			evlist.append(v_buildRelayObj(type, x, y, 0, 0, None))
		elif ev.type == pygame.MOUSEMOTION:
			x, y = ev.pos
			x = x * vwidth // rwidth
			y = y * vheight // rheight
			evlist.append(v_buildRelayObj(32, x, y, 0, 0, None))
		
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

_PDL = pygame.draw.line
_PDR = pygame.draw.rect
_PR = pygame.Rect

_temp_image = [None]

def _blit_image_with_alpha(surface, x, y, alpha):
	scr = _global_vars['virtual_screen']
	if alpha > 0:
		if alpha >= 255:
			scr.blit(surface, (x, y))
		else:
			# TODO: there must be a better way
			temp = _temp_image[0]
			if temp == None:
				w = surface.get_width()
				h = surface.get_height()
				if w < 100: w = 100
				if h < 100: h = 100
				temp = pygame.Surface((w, h)).convert()
				_temp_image[0] = temp
			temp.blit(scr, (-x, -y))
			temp.blit(surface, (0, 0))
			temp.set_alpha(alpha)
			scr.blit(temp, (x, y), surface.get_rect())

_images_downloaded = {}

def create_assertion(message):
	raise Exception(message)

def _clear_list(list):
	while len(list) > 0:
		list.pop()

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
	if not _global_vars['pumped']:
		_global_vars['old_events'] += pygame.event.get()
	else:
		_global_vars['pumped'] = False

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

_POLYGON_SURFACE = [None]
_POLYGON_BG = (123, 249, 19)
_POLYGON_BG_ALT = (122, 249, 19)
def draw_triangle(x1, y1, x2, y2, x3, y3, r, g, b, a):
	if a >= 255:
		pygame.draw.polygon(_global_vars['virtual_screen'], (r, g, b), ((x1, y1), (x2, y2), (x3, y3)))
	elif a > 0:
		bounds = [x1, y1, x1, y1]
		if x2 < x1: bounds[0] = x2
		else: bounds[2] = x2

		if y2 < y1:  bounds[1] = y2
		else: bounds[3] = y2

		if x3 < bounds[0]: bounds[0] = x3
		elif x3 > bounds[2]: bounds[2] = x3

		if y3 < bounds[1]: bounds[1] = y3
		elif y3 > bounds[3]: bounds[3] = y3
		
		w = bounds[2] - bounds[0]
		h = bounds[3] - bounds[1]

		if w == 0 or h == 0: return

		ts = _POLYGON_SURFACE[0]
		
		if ts == None:
			ts = pygame.Surface((w, h)).convert()
			_POLYGON_SURFACE[0] = ts

		tw, th = ts.get_size()
		if tw < w or th < h:
			nw = max(tw, w)
			nh = max(th, h)
			ts = pygame.Surface((nw, nh)).convert()
			_POLYGON_SURFACE[0] = ts

		bg = _POLYGON_BG # just some random uncommon color
		if r == bg[0] and g == bg[1] and b == bg[2]:
			bg = _POLYGON_BG_ALT
		ts.fill(bg)
		ts.set_colorkey(bg)
		left = bounds[0]
		top = bounds[1]
		pygame.draw.polygon(ts, (r, g, b), ((x1 - left, y1 - top), (x2 - left, y2 - top), (x3 - left, y3 - top)))
		ts.set_alpha(a)
		_global_vars['virtual_screen'].blit(ts, (left, top), (0, 0, w, h))

def draw_ellipse(left, top, width, height, r, g, b, a):
	
	if a >= 255:
		pygame.draw.ellipse(_global_vars['virtual_screen'], (r, g, b), pygame.Rect(left, top, width, height))
	elif a > 0:
		if width == 0 or height == 0: return
		ts = _POLYGON_SURFACE[0]
		if ts == None:
			ts = pygame.Surface((width, height)).convert()
			_POLYGON_SURFACE[0] = ts

		tw, th = ts.get_size()
		if tw < width or th < height:
			nw = max(tw, width)
			nh = max(th, height)
			ts = pygame.Surface((nw, nh)).convert()
			_POLYGON_SURFACE[0] = ts

		bg = _POLYGON_BG
		if r == bg[0] and g == bg[1] and b == bg[2]:
			bg = _POLYGON_BG_ALT
		ts.fill(bg)
		ts.set_colorkey(bg)
		pygame.draw.ellipse(ts, (r, g, b), pygame.Rect(0, 0, width, height))
		ts.set_alpha(a)
		_global_vars['virtual_screen'].blit(ts, (left, top), (0, 0, width, height))


def readLocalSoundResource(path):
	path = path.replace('/', os.sep)
	if os.path.exists(path):
		try:
			return pygame.mixer.Sound(path)
		except:
			return None
	return None

def playSoundImpl(sfx):
	sfx.play()

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

def _launch_browser(url):
	import webbrowser
	webbrowser.open(url, new = 2)

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

def _parse_float_helper(f_out, value):
	try:
		output = float(value)
		f_out[0] = 1
		f_out[1] = output
	except:
		f_out[0] = -1

def _music_play_now(music, loop):
	pygame.mixer.music.load(music)
	pygame.mixer.music.play(-1 if loop else 1)

def _music_pause():
	pygame.mixer.music.pause()

def _music_resume():
	pygame.mixer.music.unpause()

def _music_set_volume(ratio):
	pygame.mixer.music.set_volume(ratio)

program_data = [None]
