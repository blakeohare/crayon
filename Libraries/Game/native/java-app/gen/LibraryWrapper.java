package org.crayonlang.libraries.game;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  private static final int[] PST_intBuffer16 = new int[16];

  private static String[] PST_convertStringSetToArray(java.util.Set<String> original) {
    String[] output = new String[original.size()];
    int i = 0;
    for (String value : original) {
      output[i++] = value;
    }
    return output;
  }

  public static int lib_audio_load_sfx_from_resourceImpl(ObjectInstance obj, String path) {
    Object sfx = TranslationHelper.NoopWithReturnNull();
    obj.nativeData = new Object[1];
    obj.nativeData[0] = sfx;
    return 1;
  }

  public static boolean lib_audio_music_load_from_resourceImpl(ObjectInstance musicObj, String path) {
    Object nativeMusicObject = AudioLibHelper.loadMusicFromResource(path);
    if ((nativeMusicObject != null)) {
      musicObj.nativeData = new Object[1];
      musicObj.nativeData[0] = nativeMusicObject;
      return true;
    }
    return false;
  }

  public static int lib_audio_music_playImpl(ObjectInstance musicObject, boolean isResource, String path, double startingVolume, boolean isLoop) {
    TranslationHelper.Noop();
    Object nativeObject = null;
    if ((musicObject.nativeData != null)) {
      nativeObject = musicObject.nativeData[0];
    }
    if (isResource) {
      TranslationHelper.Noop();
    } else {
      if (!AudioLibHelper.checkPathExistence(path)) {
        return -1;
      }
      TranslationHelper.NoopWithReturnNull();
    }
    return 0;
  }

  public static int lib_audio_sfx_get_stateImpl(Object channel, Object sfxResource, int resourceId) {
    return 3;
  }

  public static int lib_audio_sfx_launch(Object sfxResource, Object[] channelNativeDataOut, double volume, double pan) {
    Object channel = TranslationHelper.NoopWithReturnNull();
    if ((channel == null)) {
      return 0;
    }
    channelNativeDataOut[0] = channel;
    return 1;
  }

  public static int lib_audio_sfx_set_panImpl(Object channel, Object sfxResource, double pan) {
    return 0;
  }

  public static int lib_audio_sfx_set_volumeImpl(Object channel, Object sfxResource, double volume) {
    return 0;
  }

  public static int lib_audio_sfx_stopImpl(Object channel, Object resource, int resourceId, boolean isActivelyPlaying, boolean hardStop) {
    TranslationHelper.Noop();
    return 0;
  }

  public static int lib_audio_sfx_unpause(Object channel, Object sfxResource, double volume, double pan) {
    TranslationHelper.Noop();
    return 0;
  }

  public static int lib_audio_stop(Object sound, boolean reset) {
    TranslationHelper.Noop();
    return 0;
  }

  public static Value lib_game_audio_getAudioResourcePath(VmContext vm, Value[] args) {
    return org.crayonlang.interpreter.vm.CrayonWrapper.resource_manager_getResourceOfType(vm, ((String) args[0].internalValue), "SND");
  }

  public static Value lib_game_audio_is_supported(VmContext vm, Value[] args) {
    if (TranslationHelper.alwaysFalse()) {
      return vm.globals.boolTrue;
    }
    return vm.globals.boolFalse;
  }

  public static Value lib_game_audio_music_is_playing(VmContext vm, Value[] args) {
    if (false) {
      return vm.globals.boolTrue;
    }
    return vm.globals.boolFalse;
  }

  public static Value lib_game_audio_music_load_from_file(VmContext vm, Value[] args) {
    return vm.globals.valueNull;
  }

  public static Value lib_game_audio_music_load_from_resource(VmContext vm, Value[] args) {
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    if (lib_audio_music_load_from_resourceImpl(objInstance1, ((String) args[1].internalValue))) {
      return vm.globals.boolTrue;
    }
    return vm.globals.boolFalse;
  }

  public static Value lib_game_audio_music_play(VmContext vm, Value[] args) {
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(vm.globals, (lib_audio_music_playImpl(((ObjectInstance) args[0].internalValue), ((boolean) args[1].internalValue), ((String) args[2].internalValue), ((double) args[3].internalValue), ((boolean) args[4].internalValue)) != -1));
  }

  public static Value lib_game_audio_music_set_volume(VmContext vm, Value[] args) {
    TranslationHelper.Noop();
    return vm.globals.valueNull;
  }

  public static Value lib_game_audio_music_stop(VmContext vm, Value[] args) {
    return vm.globals.valueNull;
  }

  public static Value lib_game_audio_sfx_get_state(VmContext vm, Value[] args) {
    ObjectInstance channelInstance = ((ObjectInstance) args[0].internalValue);
    Object nativeChannel = channelInstance.nativeData[0];
    ObjectInstance soundInstance = ((ObjectInstance) args[1].internalValue);
    Object nativeSound = soundInstance.nativeData[0];
    int resourceId = ((int) args[2].internalValue);
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, lib_audio_sfx_get_stateImpl(nativeChannel, nativeSound, resourceId));
  }

  public static Value lib_game_audio_sfx_load_from_file(VmContext vm, Value[] args) {
    return vm.globals.valueNull;
  }

  public static Value lib_game_audio_sfx_load_from_resource(VmContext vm, Value[] args) {
    ObjectInstance soundInstance = ((ObjectInstance) args[0].internalValue);
    lib_audio_load_sfx_from_resourceImpl(soundInstance, ((String) args[1].internalValue));
    return vm.globals.valueNull;
  }

  public static Value lib_game_audio_sfx_play(VmContext vm, Value[] args) {
    ObjectInstance channelInstance = ((ObjectInstance) args[0].internalValue);
    ObjectInstance resourceInstance = ((ObjectInstance) args[1].internalValue);
    channelInstance.nativeData = new Object[1];
    Object nativeResource = resourceInstance.nativeData[0];
    double vol = ((double) args[2].internalValue);
    double pan = ((double) args[3].internalValue);
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, lib_audio_sfx_launch(nativeResource, channelInstance.nativeData, vol, pan));
  }

  public static Value lib_game_audio_sfx_resume(VmContext vm, Value[] args) {
    ObjectInstance sndInstance = ((ObjectInstance) args[0].internalValue);
    Object nativeSound = sndInstance.nativeData[0];
    ObjectInstance sndResInstance = ((ObjectInstance) args[1].internalValue);
    Object nativeResource = sndResInstance.nativeData[0];
    double vol = ((double) args[2].internalValue);
    double pan = ((double) args[3].internalValue);
    lib_audio_sfx_unpause(nativeSound, nativeResource, vol, pan);
    return vm.globals.valueNull;
  }

  public static Value lib_game_audio_sfx_set_pan(VmContext vm, Value[] args) {
    ObjectInstance channel = ((ObjectInstance) args[0].internalValue);
    Object nativeChannel = channel.nativeData[0];
    ObjectInstance resource = ((ObjectInstance) args[1].internalValue);
    Object nativeResource = resource.nativeData[0];
    lib_audio_sfx_set_panImpl(nativeChannel, nativeResource, ((double) args[2].internalValue));
    return vm.globals.valueNull;
  }

  public static Value lib_game_audio_sfx_set_volume(VmContext vm, Value[] args) {
    ObjectInstance channel = ((ObjectInstance) args[0].internalValue);
    Object nativeChannel = channel.nativeData[0];
    ObjectInstance resource = ((ObjectInstance) args[1].internalValue);
    Object nativeResource = resource.nativeData[0];
    lib_audio_sfx_set_volumeImpl(nativeChannel, nativeResource, ((double) args[2].internalValue));
    return vm.globals.valueNull;
  }

  public static Value lib_game_audio_sfx_stop(VmContext vm, Value[] args) {
    ObjectInstance channel = ((ObjectInstance) args[0].internalValue);
    Object nativeChannel = channel.nativeData[0];
    ObjectInstance resource = ((ObjectInstance) args[1].internalValue);
    Object nativeResource = resource.nativeData[0];
    int resourceId = ((int) args[2].internalValue);
    int currentState = ((int) args[3].internalValue);
    boolean completeStopAndFreeChannel = ((boolean) args[4].internalValue);
    boolean isAlreadyPaused = ((currentState == 2) && !completeStopAndFreeChannel);
    if (((currentState != 3) && !isAlreadyPaused)) {
      lib_audio_sfx_stopImpl(nativeChannel, nativeResource, resourceId, (currentState == 1), completeStopAndFreeChannel);
    }
    return vm.globals.valueNull;
  }

  public static Value lib_game_clock_tick(VmContext vm, Value[] args) {
    TranslationHelper.alwaysTrue();
    org.crayonlang.interpreter.vm.CrayonWrapper.vm_suspend_context_by_id(vm, ((int) args[0].internalValue), 1);
    return vm.globalNull;
  }

  public static Value lib_game_gamepad_current_device_count(VmContext vm, Value[] args) {
    int total = 0;
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, total);
  }

  public static Value lib_game_gamepad_get_axis_1d_state(VmContext vm, Value[] args) {
    return vm.globals.floatZero;
  }

  public static Value lib_game_gamepad_get_axis_2d_state(VmContext vm, Value[] args) {
    Value arg1 = args[0];
    Value arg2 = args[1];
    Value arg3 = args[2];
    return vm.globalNull;
  }

  public static Value lib_game_gamepad_get_button_state(VmContext vm, Value[] args) {
    return vm.globalTrue;
  }

  public static Value lib_game_gamepad_get_save_file_path(VmContext vm, Value[] args) {
    return vm.globalNull;
  }

  public static Value lib_game_gamepad_getPlatform(VmContext vm, Value[] args) {
    return vm.globalNull;
  }

  public static Value lib_game_gamepad_initialize_device(VmContext vm, Value[] args) {
    return vm.globalNull;
  }

  public static Value lib_game_gamepad_is_supported(VmContext vm, Value[] args) {
    return vm.globalFalse;
  }

  public static Value lib_game_gamepad_jsIsOsx(VmContext vm, Value[] args) {
    return vm.globalNull;
  }

  public static Value lib_game_gamepad_platform_requires_refresh(VmContext vm, Value[] args) {
    return vm.globalFalse;
  }

  public static Value lib_game_gamepad_poll_universe(VmContext vm, Value[] args) {
    return vm.globalNull;
  }

  public static Value lib_game_gamepad_refresh_devices(VmContext vm, Value[] args) {
    return vm.globalNull;
  }

  public static Value lib_game_getScreenInfo(VmContext vm, Value[] args) {
    Value outList = args[0];
    int[] o = PST_intBuffer16;
    if (TranslationHelper.alwaysFalse()) {
      ListImpl output = ((ListImpl) outList.internalValue);
      org.crayonlang.interpreter.vm.CrayonWrapper.clearList(output);
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(vm.globals, (o[0] == 1)));
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, o[1]));
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, o[2]));
    }
    return outList;
  }

  public static Value lib_game_getTouchState(VmContext vm, Value[] args) {
    ListImpl output = ((ListImpl) args[0].internalValue);
    int[] data = new int[31];
    data[0] = 0;
    TranslationHelper.alwaysFalse();
    int _len = data[0];
    int end = ((_len * 3) + 1);
    int i = 1;
    while ((i < end)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, data[i]));
      i += 1;
    }
    return vm.globalNull;
  }

  public static Value lib_game_initialize(VmContext vm, Value[] args) {
    GameWindow.FPS = org.crayonlang.interpreter.vm.CrayonWrapper.getFloat(args[0]);
    return vm.globalNull;
  }

  public static Value lib_game_initialize_screen(VmContext vm, Value[] args) {
    int ecId = ((int) args[4].internalValue);
    ExecutionContext ec = org.crayonlang.interpreter.vm.CrayonWrapper.getExecutionContext(vm, ecId);
    GameWindow.initializeScreen(((int) args[0].internalValue), ((int) args[1].internalValue), ((int) args[2].internalValue), ((int) args[3].internalValue), ecId);
    org.crayonlang.interpreter.vm.CrayonWrapper.vm_suspend_for_context(ec, 1);
    return vm.globalNull;
  }

  public static Value lib_game_pump_events(VmContext vm, Value[] args) {
    ListImpl output = ((ListImpl) args[0].internalValue);
    ArrayList<PlatformRelayObject> eventList = GameWindow.INSTANCE.pumpEventQueue();
    VmGlobals globals = vm.globals;
    org.crayonlang.interpreter.vm.CrayonWrapper.clearList(output);
    int _len = eventList.size();
    if ((_len > 0)) {
      int i = 0;
      i = 0;
      while ((i < _len)) {
        PlatformRelayObject ev = eventList.get(i);
        org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(globals, ev.type));
        int t = ev.type;
        org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(globals, ev.iarg1));
        if ((t >= 32)) {
          org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(globals, ev.iarg2));
          if ((t == 37)) {
            org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, org.crayonlang.interpreter.vm.CrayonWrapper.buildFloat(globals, ev.farg1));
          } else {
            if (((t >= 64) && (t < 80))) {
              org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(globals, ev.iarg3));
            }
          }
        }
        i += 1;
      }
    }
    return args[0];
  }

  public static Value lib_game_set_title(VmContext vm, Value[] args) {
    GameWindow.INSTANCE.setTitle(((String) args[0].internalValue));
    return vm.globalNull;
  }

  public static Value lib_game_set_window_mode(VmContext vm, Value[] args) {
    int mode = ((int) args[0].internalValue);
    int width = ((int) args[1].internalValue);
    int height = ((int) args[2].internalValue);
    TranslationHelper.alwaysFalse();
    return vm.globalNull;
  }

  public static Value lib_game_setInstance(VmContext vm, Value[] args) {
    ObjectInstance o = ((ObjectInstance) args[0].internalValue);
    Object[] nd = new Object[1];
    nd[0] = GameWindow.INSTANCE;
    o.nativeData = nd;
    return vm.globalNull;
  }

  public static Value lib_game_startup(VmContext vm, Value[] args) {
    HashMap<String, java.lang.reflect.Method> functionLookup = GameWindow.getCallbackFunctions();
    String[] names = PST_convertStringSetToArray(functionLookup.keySet());
    int i = 0;
    while ((i < names.length)) {
      java.lang.reflect.Method fn = null;
      String name = names[i];
      java.lang.reflect.Method _PST_dictLookup0 = functionLookup.get(name);
      fn = _PST_dictLookup0 == null ? (functionLookup.containsKey(name) ? null : (null)) : _PST_dictLookup0;
      if ((fn != null)) {
        org.crayonlang.interpreter.vm.CrayonWrapper.registerNamedCallback(vm, "Game", name, fn);
      }
      i += 1;
    }
    return vm.globalNull;
  }
}
