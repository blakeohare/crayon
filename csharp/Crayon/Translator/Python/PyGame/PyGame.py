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
for value in KEY_LOOKUP.values() + 'exit closebutton key alt-f4 mousemove mouseleftdown mouseleftup mouserightdown mouserightup'.split(' '):
	COMMON_STRINGS[value] = [%%%TYPE_STRING%%%, value]

def _pygame_pump_events():
	evs = pygame.event.get()
	pressed_keys = pygame.key.get_pressed()
	evlist = []
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
			evlist.append([%%%TYPE_LIST%%%, [type, [%%%TYPE_INTEGER%%%, x], [%%%TYPE_INTEGER%%%, y]]])
		elif ev.type == pygame.MOUSEMOTION:
			x, y = ev.pos
			type = COMMON_STRINGS['mousemove']
			evlist.append([%%%TYPE_LIST%%%, [type, [%%%TYPE_INTEGER%%%, x], [%%%TYPE_INTEGER%%%, y]]])
		
	return [%%%TYPE_LIST%%%, evlist]

def platform_begin(fps):
	pygame.init()
	_global_vars['fps'] = fps

def _pygame_initialize_screen(width, height):
	_global_vars['width'] = width
	_global_vars['height'] = height
	screen = pygame.display.set_mode((_global_vars['width'], _global_vars['height']))
	return [%%%TYPE_NATIVE_OBJECT%%%, (%%%TYPE_NATIVE_OBJECT_SCREEN%%%, screen)]


def _kill_execution(message):
	raise Exception(message)

_PDR = pygame.draw.rect
_PDF = pygame.display.flip
_PR = pygame.Rect
