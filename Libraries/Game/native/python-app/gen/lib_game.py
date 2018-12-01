import base64
import code.vm as VM
import math
import os
import random
import sys
import time

PST_StringBuffer16 = [None] * 16
PST_IntBuffer16 = [0] * 16
PST_FloatBuffer16 = [0.0] * 16
PST_NoneListOfOne = [None]

PST_StringType = type('')
def PST_base64ToString(value):
  u_value = base64.b64decode(value)
  if type(u_value) == PST_StringType:
    return u_value
  return u_value.decode('utf8')

def PST_isValidInteger(value):
  if len(value) == 0: return False
  if value[0] == '-': value = value[1:]
  return value.isdigit()

def PST_sortedCopyOfList(t):
  t = t[:]
  t.sort()
  return t

def PST_tryParseFloat(value, floatOut):
  try:
    floatOut[1] = float(value)
    floatOut[0] = 1.0
  except:
    floatOut[0] = -1.0

def PST_stringCheckSlice(haystack, i, needle):
  return haystack[i:i + len(needle)] == needle

def always_true(): return True
def always_false(): return False

def lib_audio_getAudioResourcePath(vm, args):
  return resource_manager_getResourceOfType(vm, args[0][1], "SND")

def lib_audio_is_supported(vm, args):
  if always_true():
    return vm[13][1]
  return vm[13][2]

def lib_audio_load_sfx_from_resourceImpl(obj, path):
  sfx = readLocalSoundResource(path)
  obj[3] = [None]
  obj[3][0] = sfx
  return 1

def lib_audio_music_is_playing(vm, args):
  if audio_music_is_playing():
    return vm[13][1]
  return vm[13][2]

def lib_audio_music_load_from_file(vm, args):
  return vm[13][0]

def lib_audio_music_load_from_resource(vm, args):
  objInstance1 = args[0][1]
  if lib_audio_music_load_from_resourceImpl(objInstance1, args[1][1]):
    return vm[13][1]
  return vm[13][2]

def lib_audio_music_load_from_resourceImpl(musicObj, path):
  nativeMusicObject = 1
  if (nativeMusicObject != None):
    musicObj[3] = [None]
    musicObj[3][0] = nativeMusicObject
    return True
  return False

def lib_audio_music_play(vm, args):
  return buildBoolean(vm[13], (lib_audio_music_playImpl(args[0][1], args[1][1], args[2][1], args[3][1], args[4][1]) != -1))

def lib_audio_music_playImpl(musicObject, isResource, path, startingVolume, isLoop):
  audio_music_set_volume(startingVolume)
  nativeObject = None
  if (musicObject[3] != None):
    nativeObject = musicObject[3][0]
  if isResource:
    audio_music_play_resource(path, isLoop)
  else:
    if not (audio_music_verify_file_exists(path)):
      return -1
    audio_music_play_file(nativeObject, path, isLoop)
  return 0

def lib_audio_music_set_volume(vm, args):
  audio_music_set_volume(args[0][1])
  return vm[13][0]

def lib_audio_music_stop(vm, args):
  return vm[13][0]

def lib_audio_sfx_get_state(vm, args):
  channelInstance = args[0][1]
  nativeChannel = channelInstance[3][0]
  soundInstance = args[1][1]
  nativeSound = soundInstance[3][0]
  resourceId = args[2][1]
  return buildInteger(vm[13], lib_audio_sfx_get_stateImpl(nativeChannel, nativeSound, resourceId))

def lib_audio_sfx_get_stateImpl(channel, sfxResource, resourceId):
  return audio_sound_get_state(channel, sfxResource, resourceId)

def lib_audio_sfx_launch(sfxResource, channelNativeDataOut, volume, pan):
  channel = audio_sound_play(sfxResource, volume, pan)
  if (channel == None):
    return 0
  channelNativeDataOut[0] = channel
  return 1

def lib_audio_sfx_load_from_file(vm, args):
  return vm[13][0]

def lib_audio_sfx_load_from_resource(vm, args):
  soundInstance = args[0][1]
  lib_audio_load_sfx_from_resourceImpl(soundInstance, args[1][1])
  return vm[13][0]

def lib_audio_sfx_play(vm, args):
  channelInstance = args[0][1]
  resourceInstance = args[1][1]
  channelInstance[3] = [None]
  nativeResource = resourceInstance[3][0]
  vol = args[2][1]
  pan = args[3][1]
  return buildInteger(vm[13], lib_audio_sfx_launch(nativeResource, channelInstance[3], vol, pan))

def lib_audio_sfx_resume(vm, args):
  sndInstance = args[0][1]
  nativeSound = sndInstance[3][0]
  sndResInstance = args[1][1]
  nativeResource = sndResInstance[3][0]
  vol = args[2][1]
  pan = args[3][1]
  lib_audio_sfx_unpause(nativeSound, nativeResource, vol, pan)
  return vm[13][0]

def lib_audio_sfx_set_pan(vm, args):
  channel = args[0][1]
  nativeChannel = channel[3][0]
  resource = args[1][1]
  nativeResource = resource[3][0]
  lib_audio_sfx_set_panImpl(nativeChannel, nativeResource, args[2][1])
  return vm[13][0]

def lib_audio_sfx_set_panImpl(channel, sfxResource, pan):
  return 0

def lib_audio_sfx_set_volume(vm, args):
  channel = args[0][1]
  nativeChannel = channel[3][0]
  resource = args[1][1]
  nativeResource = resource[3][0]
  lib_audio_sfx_set_volumeImpl(nativeChannel, nativeResource, args[2][1])
  return vm[13][0]

def lib_audio_sfx_set_volumeImpl(channel, sfxResource, volume):
  return 0

def lib_audio_sfx_stop(vm, args):
  channel = args[0][1]
  nativeChannel = channel[3][0]
  resource = args[1][1]
  nativeResource = resource[3][0]
  resourceId = args[2][1]
  currentState = args[3][1]
  completeStopAndFreeChannel = args[4][1]
  isAlreadyPaused = ((currentState == 2) and not (completeStopAndFreeChannel))
  if ((currentState != 3) and not (isAlreadyPaused)):
    lib_audio_sfx_stopImpl(nativeChannel, nativeResource, resourceId, (currentState == 1), completeStopAndFreeChannel)
  return vm[13][0]

def lib_audio_sfx_stopImpl(channel, resource, resourceId, isActivelyPlaying, hardStop):
  audio_sound_stop(channel, resource, resourceId, isActivelyPlaying, hardStop)
  return 0

def lib_audio_sfx_unpause(channel, sfxResource, volume, pan):
  audio_sound_resume(channel, sfxResource, volume, pan)
  return 0

def lib_audio_stop(sound, reset):
  stopSoundImpl(sound)
  return 0

def lib_game_clock_tick(vm, args):
  always_true()
  vm_suspend(vm, 1)
  return vm[14]

def lib_game_gamepad_current_device_count(vm, args):
  total = 0
  if (True and always_true()):
    total = libhelper_gamepad_get_current_joystick_count()
  return buildInteger(vm[13], total)

def lib_game_gamepad_get_axis_1d_state(vm, args):
  index = args[1][1]
  dev = args[0][1]
  return buildFloat(vm[13], libhelper_gamepad_get_joystick_axis_1d_state(dev[3][0], index))

def lib_game_gamepad_get_axis_2d_state(vm, args):
  arg1 = args[0]
  arg2 = args[1]
  arg3 = args[2]
  objInstance1 = arg1[1]
  int1 = arg2[1]
  list1 = arg3[1]
  libhelper_gamepad_get_joystick_axis_2d_state(objInstance1[3][0], int1, PST_IntBuffer16)
  clearList(list1)
  addToList(list1, buildInteger(vm[13], PST_IntBuffer16[0]))
  addToList(list1, buildInteger(vm[13], PST_IntBuffer16[1]))
  return vm[14]

def lib_game_gamepad_get_button_state(vm, args):
  int1 = 0
  objInstance1 = None
  arg1 = args[0]
  arg2 = args[1]
  objInstance1 = arg1[1]
  int1 = arg2[1]
  if libhelper_gamepad_get_joystick_button_state(objInstance1[3][0], int1):
    return vm[15]
  return vm[16]

def lib_game_gamepad_get_save_file_path(vm, args):
  string1 = ".crayon-pygame.gamepad.config"
  if (string1 != None):
    return buildString(vm[13], string1)
  return vm[14]

def lib_game_gamepad_getPlatform(vm, args):
  return buildInteger(vm[13], 2)

def lib_game_gamepad_initialize_device(vm, args):
  int1 = args[0][1]
  objInstance1 = args[1][1]
  list1 = args[2][1]
  object1 = libhelper_gamepad_get_joystick(int1)
  objInstance1[3] = [None]
  objInstance1[3][0] = object1
  clearList(list1)
  addToList(list1, buildString(vm[13], libhelper_gamepad_get_joystick_name(object1)))
  addToList(list1, buildInteger(vm[13], libhelper_gamepad_get_joystick_button_count(object1)))
  addToList(list1, buildInteger(vm[13], libhelper_gamepad_get_joystick_axis_1d_count(object1)))
  addToList(list1, buildInteger(vm[13], libhelper_gamepad_get_joystick_axis_2d_count(object1)))
  return vm[14]

def lib_game_gamepad_is_supported(vm, args):
  if (True and always_true()):
    return vm[15]
  else:
    return vm[16]

def lib_game_gamepad_jsIsOsx(vm, args):
  return buildInteger(vm[13], 0)

def lib_game_gamepad_poll_universe(vm, args):
  always_true()
  return vm[14]

def lib_game_gamepad_refresh_devices(vm, args):
  always_true()
  return vm[14]

def lib_game_getScreenInfo(vm, args):
  outList = args[0]
  o = PST_IntBuffer16
  if always_false():
    output = outList[1]
    clearList(output)
    addToList(output, buildBoolean(vm[13], (o[0] == 1)))
    addToList(output, buildInteger(vm[13], o[1]))
    addToList(output, buildInteger(vm[13], o[2]))
  return outList

def lib_game_getTouchState(vm, args):
  output = args[0][1]
  data = (PST_NoneListOfOne * 31)
  data[0] = 0
  always_false()
  _len = data[0]
  end = ((_len * 3) + 1)
  i = 1
  while (i < end):
    addToList(output, buildInteger(vm[13], data[i]))
    i += 1
  return vm[14]

def lib_game_initialize(vm, args):
  platform_begin(getFloat(args[0]))
  return vm[14]

def lib_game_initialize_screen(vm, args):
  ec = getExecutionContext(vm, vm_getCurrentExecutionContextId(vm))
  pygame_initialize_screen(args[0][1], args[1][1], (args[2][1], args[3][1]), vm_getCurrentExecutionContextId(vm))
  vm_suspend_for_context(ec, 1)
  return vm[14]

def lib_game_pump_events(vm, args):
  output = args[0][1]
  eventList = pygame_pump_events()
  globals = vm[13]
  clearList(output)
  _len = len(eventList)
  if (_len > 0):
    i = 0
    i = 0
    while (i < _len):
      ev = eventList[i]
      addToList(output, buildInteger(globals, ev[0]))
      t = ev[0]
      addToList(output, buildInteger(globals, ev[1]))
      if (t >= 32):
        addToList(output, buildInteger(globals, ev[2]))
        if (t == 37):
          addToList(output, buildFloat(globals, ev[4]))
        elif ((t >= 64) and (t < 80)):
          addToList(output, buildInteger(globals, ev[3]))
      i += 1
  return args[0]

def lib_game_set_title(vm, args):
  pygame.display.set_caption(args[0][1])
  return vm[14]

def lib_game_set_window_mode(vm, args):
  mode = args[0][1]
  width = args[1][1]
  height = args[2][1]
  always_false()
  return vm[14]

def lib_game_setInstance(vm, args):
  o = args[0][1]
  nd = [None]
  nd[0] = None
  o[3] = nd
  return vm[14]

def lib_game_startup(vm, args):
  functionLookup = pygame_getCallbackFunctions()
  names = list(functionLookup.keys())
  i = 0
  while (i < len(names)):
    fn = None
    name = names[i]
    fn = functionLookup.get(name, None)
    if (fn != None):
      registerNamedCallback(vm, "Game", name, fn)
    i += 1
  return vm[14]

def lib_gamepad_platform_requires_refresh(vm, args):
  if (True and always_false()):
    return vm[15]
  return vm[16]
