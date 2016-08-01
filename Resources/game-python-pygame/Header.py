
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
	KEY_LOOKUP[pygame.K_a + i] = 65 + i
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

def _pygame_initialize_screen(width, height, pixel_dimensions, execId):
	_global_vars['width'] = width
	_global_vars['height'] = height
	scaled_mode = pixel_dimensions != None and (width != pixel_dimensions[0] or height != pixel_dimensions[1])
	if scaled_mode:
		real_screen = pygame.display.set_mode(pixel_dimensions)
		virtual_screen = pygame.Surface((width, height)).convert()
	else:
		virtual_screen = pygame.display.set_mode((_global_vars['width'], _global_vars['height']))
		real_screen = virtual_screen
	_global_vars['real_screen'] = real_screen
	_global_vars['virtual_screen'] = virtual_screen
	_global_vars['scaled_mode'] = scaled_mode

	while v_runInterpreter(execId) == 2:
		gfxRender()
		_pygame_end_of_frame()

def create_assertion(message):
	raise Exception(message)

def _clear_list(list):
	while len(list) > 0:
		list.pop()

def create_sorted_copy_of_list(items):
	items = items[:]
	items.sort()
	return items

def load_local_image_resource(path):
	path = path.replace('/', os.sep);
	if not os.path.exists(path): return None
	try:
		return pygame.image.load(path)
	except:
		return None

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

def _parse_json(raw):
	import json
	try:
		return _parse_json_thing(json.loads(raw))
	except:
		return None

def _parse_json_thing(item):
	if item == None: return v_VALUE_NULL
	if item == "": return v_VALUE_EMPTY_STRING
	t = str(type(item))
	if "'bool'" in t:
		if item == True:
			return VALUE_TRUE
		return VALUE_FALSE
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

def _music_pause():
	pygame.mixer.music.pause()

def _audio_music_set_volume(ratio):
	pygame.mixer.music.set_volume(ratio)

def _audio_music_play_resource(path, loop):
	path = 'resources/audio/' + path
	pygame.mixer.music.load(path.replace('/', os.sep))
	pygame.mixer.music.play(-1 if loop else 0)
	
def readLocalSoundResource(path):
	path = 'resources/audio/' + path
	path = path.replace('/', os.sep)
	if os.path.exists(path):
		try:
			return pygame.mixer.Sound(path)
		except:
			return None
	return None

def _audio_sound_play(sfx, vol, pan):
	ch = sfx.play()
	ch.set_volume(vol)

def string_check_slice(haystack, i, needle):
	return haystack[i:i + len(needle)] == needle

def string_substring(s, start, length = None):
	if length == None: return s[start:]
	return s[start:start + length]

def _always_true(): return True
def _always_false(): return False

program_data = [None]
