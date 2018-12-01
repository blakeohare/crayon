package org.crayonlang.interpreter.libraries.game;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.structs.*;

public final class LibraryWrapper {

  private static java.util.Random PST_random = new java.util.Random();
  private static final String[] PST_emptyArrayString = new String[0];
  @SuppressWarnings("rawtypes")
  private static final ArrayList[] PST_emptyArrayList = new ArrayList[0];
  @SuppressWarnings("rawtypes")
  private static final HashMap[] PST_emptyArrayMap = new HashMap[0];

  private static final int[] PST_intBuffer16 = new int[16];
  private static final double[] PST_floatBuffer16 = new double[16];
  private static final String[] PST_stringBuffer16 = new String[16];

  private static final java.nio.charset.Charset UTF8 = java.nio.charset.Charset.forName("UTF-8");
  private static String PST_base64ToString(String b64Value) {
    int inputLength = b64Value.length();

    if (inputLength == 0) return "";
    while (inputLength > 0 && b64Value.charAt(inputLength - 1) == '=') {
      b64Value = b64Value.substring(0, --inputLength);
    }
    int bitsOfData = inputLength * 6;
    int outputLength = bitsOfData / 8;

    byte[] buffer = new byte[outputLength];
    char c;
    int charValue;
    for (int i = 0; i < inputLength; ++i) {
      c = b64Value.charAt(i);
      charValue = -1;
      switch (c) {
        case '=': break;
        case '+': charValue = 62;
        case '/': charValue = 63;
        default:
          if (c >= 'A' && c <= 'Z') {
            charValue = c - 'A';
          } else if (c >= 'a' && c <= 'z') {
            charValue = c - 'a' + 26;
          } else if (c >= '0' && c <= '9') {
            charValue = c - '0' + 52;
          }
          break;
      }

      if (charValue != -1) {
        int bitOffset = i * 6;
        int targetIndex = bitOffset / 8;
        int bitWithinByte = bitOffset % 8;
        switch (bitOffset % 8) {
          case 0:
            buffer[targetIndex] |= charValue << 2;
            break;
          case 2:
            buffer[targetIndex] |= charValue;
            break;
          case 4:
            buffer[targetIndex] |= charValue >> 2;
            if (targetIndex + 1 < outputLength)
              buffer[targetIndex + 1] |= charValue << 6;
            break;
          case 6:
            buffer[targetIndex] |= charValue >> 4;
            if (targetIndex + 1 < outputLength)
              buffer[targetIndex + 1] |= charValue << 4;
            break;
        }
      }
    }
    return new String(buffer, UTF8);
  }

  private static int[] PST_convertIntegerSetToArray(java.util.Set<Integer> original) {
    int[] output = new int[original.size()];
    int i = 0;
    for (int value : original) {
      output[i++] = value;
    }
    return output;
  }

  private static String[] PST_convertStringSetToArray(java.util.Set<String> original) {
    String[] output = new String[original.size()];
    int i = 0;
    for (String value : original) {
      output[i++] = value;
    }
    return output;
  }

  private static boolean PST_isValidInteger(String value) {
    try {
      Integer.parseInt(value);
    } catch (NumberFormatException nfe) {
      return false;
    }
    return true;
  }

  private static String PST_joinChars(ArrayList<Character> chars) {
    char[] output = new char[chars.size()];
    for (int i = output.length - 1; i >= 0; --i) {
      output[i] = chars.get(i);
    }
    return String.copyValueOf(output);
  }

  private static String PST_joinList(String sep, ArrayList<String> items) {
    int length = items.size();
    if (length < 2) {
      if (length == 0) return "";
      return items.get(0);
    }

    boolean useSeparator = sep.length() > 0;
    StringBuilder sb = new StringBuilder(useSeparator ? (length * 2 - 1) : length);
    sb.append(items.get(0));
    if (useSeparator) {
      for (int i = 1; i < length; ++i) {
        sb.append(sep);
        sb.append(items.get(i));
      }
    } else {
      for (int i = 1; i < length; ++i) {
        sb.append(items.get(i));
      }
    }

    return sb.toString();
  }

  private static <T> T PST_listPop(ArrayList<T> list) {
    return list.remove(list.size() - 1);
  }

  private static String[] PST_literalStringSplit(String original, String sep) {
    ArrayList<String> output = new ArrayList<String>();
    ArrayList<String> currentPiece = new ArrayList<String>();
    int length = original.length();
    int sepLength = sep.length();
    char firstSepChar = sep.charAt(0);
    char c;
    int j;
    boolean match;
    for (int i = 0; i < length; ++i) {
      c = original.charAt(i);
      match = false;
      if (c == firstSepChar) {
        match = true;
        for (j = 1; j < sepLength; ++j) {
          if (i + j < length ) {
            if (sep.charAt(j) != original.charAt(i + j)) {
              match = false;
              break;
            }
          } else {
            match = false;
          }
        }
      }

      if (match) {
        output.add(PST_joinList("", currentPiece));
        currentPiece.clear();
        i += sepLength - 1;
      } else {
        currentPiece.add("" + c);
      }
    }
    output.add(PST_joinList("", currentPiece));
    return output.toArray(new String[output.size()]);
  }

  private static String PST_reverseString(String original) {
    char[] output = original.toCharArray();
    int length = output.length;
    int lengthMinusOne = length - 1;
    char c;
    for (int i = length / 2 - 1; i >= 0; --i) {
      c = output[i];
      output[i] = output[lengthMinusOne - i];
      output[lengthMinusOne] = c;
    }
    return String.copyValueOf(output);
  }

  private static boolean PST_checkStringInString(String haystack, int index, String expectedValue) {
    int evLength = expectedValue.length();
    if (evLength + index > haystack.length()) return false;
    if (evLength == 0) return true;
    if (expectedValue.charAt(0) != haystack.charAt(index)) return false;
    if (expectedValue.charAt(evLength - 1) != haystack.charAt(index + evLength - 1)) return false;
    if (evLength <= 2) return true;
    for (int i = evLength - 2; i > 1; --i) {
      if (expectedValue.charAt(i) != haystack.charAt(index + i)) return false;
    }
    return true;
  }

  private static String PST_trimSide(String value, boolean isLeft) {
    int i = isLeft ? 0 : value.length() - 1;
    int end = isLeft ? value.length() : -1;
    int step = isLeft ? 1 : -1;
    char c;
    boolean trimming = true;
    while (trimming && i != end) {
      c = value.charAt(i);
      switch (c) {
        case ' ':
        case '\n':
        case '\t':
        case '\r':
          i += step;
          break;
        default:
          trimming = false;
          break;
      }
    }

    return isLeft ? value.substring(i) : value.substring(0, i + 1);
  }

  private static void PST_parseFloatOrReturnNull(double[] outParam, String rawValue) {
    try {
      outParam[1] = Double.parseDouble(rawValue);
      outParam[0] = 1;
    } catch (NumberFormatException nfe) {
      outParam[0] = -1;
    }
  }

  private static <T> ArrayList<T> PST_multiplyList(ArrayList<T> list, int n) {
    int len = list.size();
    ArrayList<T> output = new ArrayList<T>(len * n);
    if (len > 0) {
      if (len == 1) {
        T t = list.get(0);
        while (n --> 0) {
          output.add(t);
        }
      } else {
        while (n --> 0) {
          output.addAll(list);
        }
      }
    }
    return output;
  }

  private static <T> ArrayList<T> PST_concatLists(ArrayList<T> a, ArrayList<T> b) {
    ArrayList<T> output = new ArrayList(a.size() + b.size());
    output.addAll(a);
    output.addAll(b);
    return output;
  }

  private static <T> void PST_listShuffle(ArrayList<T> list) {
    int len = list.size();
    for (int i = len - 1; i >= 0; --i) {
      int ti = PST_random.nextInt(len);
      if (ti != i) {
        T t = list.get(ti);
        list.set(ti, list.get(i));
        list.set(i, t);
      }
    }
  }

  private static boolean[] PST_listToArrayBool(ArrayList<Boolean> list) {
    int length = list.size();
    boolean[] output = new boolean[length];
    for (int i = 0; i < length; ++i) output[i] = list.get(i);
    return output;
  }

  private static byte[] PST_listToArrayByte(ArrayList<Byte> list) {
    int length = list.size();
    byte[] output = new byte[length];
    for (int i = 0; i < length; ++i) output[i] = list.get(i);
    return output;
  }

  private static int[] PST_listToArrayInt(ArrayList<Integer> list) {
    int length = list.size();
    int[] output = new int[length];
    for (int i = 0; i < length; ++i) output[i] = list.get(i);
    return output;
  }

  private static double[] PST_listToArrayDouble(ArrayList<Double> list) {
    int length = list.size();
    double[] output = new double[length];
    for (int i = 0; i < length; ++i) output[i] = list.get(i);
    return output;
  }

  private static char[] PST_listToArrayChar(ArrayList<Character> list) {
    int length = list.size();
    char[] output = new char[length];
    for (int i = 0; i < length; ++i) output[i] = list.get(i);
    return output;
  }

  private static int[] PST_sortedCopyOfIntArray(int[] nums) {
    int[] output = java.util.Arrays.copyOf(nums, nums.length);
    java.util.Arrays.sort(output);
    return output;
  }

  private static String[] PST_sortedCopyOfStringArray(String[] values) {
    String[] output = java.util.Arrays.copyOf(values, values.length);
    java.util.Arrays.sort(output);
    return output;
  }

  public static Value lib_audio_getAudioResourcePath(VmContext vm, Value[] args) {
    return org.crayonlang.interpreter.vm.CrayonWrapper.resource_manager_getResourceOfType(vm, ((String) args[0].internalValue), "SND");
  }

  public static Value lib_audio_is_supported(VmContext vm, Value[] args) {
    if (TranslationHelper.alwaysFalse()) {
      return vm.globals.boolTrue;
    }
    return vm.globals.boolFalse;
  }

  public static int lib_audio_load_sfx_from_resourceImpl(ObjectInstance obj, String path) {
    Object sfx = TranslationHelper.NoopWithReturnNull();
    obj.nativeData = new Object[1];
    obj.nativeData[0] = sfx;
    return 1;
  }

  public static Value lib_audio_music_is_playing(VmContext vm, Value[] args) {
    if (false) {
      return vm.globals.boolTrue;
    }
    return vm.globals.boolFalse;
  }

  public static Value lib_audio_music_load_from_file(VmContext vm, Value[] args) {
    return vm.globals.valueNull;
  }

  public static Value lib_audio_music_load_from_resource(VmContext vm, Value[] args) {
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    if (lib_audio_music_load_from_resourceImpl(objInstance1, ((String) args[1].internalValue))) {
      return vm.globals.boolTrue;
    }
    return vm.globals.boolFalse;
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

  public static Value lib_audio_music_play(VmContext vm, Value[] args) {
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(vm.globals, (lib_audio_music_playImpl(((ObjectInstance) args[0].internalValue), ((boolean) args[1].internalValue), ((String) args[2].internalValue), ((double) args[3].internalValue), ((boolean) args[4].internalValue)) != -1));
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

  public static Value lib_audio_music_set_volume(VmContext vm, Value[] args) {
    TranslationHelper.Noop();
    return vm.globals.valueNull;
  }

  public static Value lib_audio_music_stop(VmContext vm, Value[] args) {
    return vm.globals.valueNull;
  }

  public static Value lib_audio_sfx_get_state(VmContext vm, Value[] args) {
    ObjectInstance channelInstance = ((ObjectInstance) args[0].internalValue);
    Object nativeChannel = channelInstance.nativeData[0];
    ObjectInstance soundInstance = ((ObjectInstance) args[1].internalValue);
    Object nativeSound = soundInstance.nativeData[0];
    int resourceId = ((int) args[2].internalValue);
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, lib_audio_sfx_get_stateImpl(nativeChannel, nativeSound, resourceId));
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

  public static Value lib_audio_sfx_load_from_file(VmContext vm, Value[] args) {
    return vm.globals.valueNull;
  }

  public static Value lib_audio_sfx_load_from_resource(VmContext vm, Value[] args) {
    ObjectInstance soundInstance = ((ObjectInstance) args[0].internalValue);
    lib_audio_load_sfx_from_resourceImpl(soundInstance, ((String) args[1].internalValue));
    return vm.globals.valueNull;
  }

  public static Value lib_audio_sfx_play(VmContext vm, Value[] args) {
    ObjectInstance channelInstance = ((ObjectInstance) args[0].internalValue);
    ObjectInstance resourceInstance = ((ObjectInstance) args[1].internalValue);
    channelInstance.nativeData = new Object[1];
    Object nativeResource = resourceInstance.nativeData[0];
    double vol = ((double) args[2].internalValue);
    double pan = ((double) args[3].internalValue);
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, lib_audio_sfx_launch(nativeResource, channelInstance.nativeData, vol, pan));
  }

  public static Value lib_audio_sfx_resume(VmContext vm, Value[] args) {
    ObjectInstance sndInstance = ((ObjectInstance) args[0].internalValue);
    Object nativeSound = sndInstance.nativeData[0];
    ObjectInstance sndResInstance = ((ObjectInstance) args[1].internalValue);
    Object nativeResource = sndResInstance.nativeData[0];
    double vol = ((double) args[2].internalValue);
    double pan = ((double) args[3].internalValue);
    lib_audio_sfx_unpause(nativeSound, nativeResource, vol, pan);
    return vm.globals.valueNull;
  }

  public static Value lib_audio_sfx_set_pan(VmContext vm, Value[] args) {
    ObjectInstance channel = ((ObjectInstance) args[0].internalValue);
    Object nativeChannel = channel.nativeData[0];
    ObjectInstance resource = ((ObjectInstance) args[1].internalValue);
    Object nativeResource = resource.nativeData[0];
    lib_audio_sfx_set_panImpl(nativeChannel, nativeResource, ((double) args[2].internalValue));
    return vm.globals.valueNull;
  }

  public static int lib_audio_sfx_set_panImpl(Object channel, Object sfxResource, double pan) {
    return 0;
  }

  public static Value lib_audio_sfx_set_volume(VmContext vm, Value[] args) {
    ObjectInstance channel = ((ObjectInstance) args[0].internalValue);
    Object nativeChannel = channel.nativeData[0];
    ObjectInstance resource = ((ObjectInstance) args[1].internalValue);
    Object nativeResource = resource.nativeData[0];
    lib_audio_sfx_set_volumeImpl(nativeChannel, nativeResource, ((double) args[2].internalValue));
    return vm.globals.valueNull;
  }

  public static int lib_audio_sfx_set_volumeImpl(Object channel, Object sfxResource, double volume) {
    return 0;
  }

  public static Value lib_audio_sfx_stop(VmContext vm, Value[] args) {
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

  public static Value lib_game_clock_tick(VmContext vm, Value[] args) {
    TranslationHelper.alwaysTrue();
    org.crayonlang.interpreter.vm.CrayonWrapper.vm_suspend(vm, 1);
    return vm.globalNull;
  }

  public static Value lib_game_gamepad_current_device_count(VmContext vm, Value[] args) {
    int total = 0;
    if ((false && TranslationHelper.alwaysFalse())) {
      total = 0;
    }
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
    if ((false && TranslationHelper.alwaysFalse())) {
      return vm.globalTrue;
    } else {
      return vm.globalFalse;
    }
  }

  public static Value lib_game_gamepad_jsIsOsx(VmContext vm, Value[] args) {
    return vm.globalNull;
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
    ExecutionContext ec = org.crayonlang.interpreter.vm.CrayonWrapper.getExecutionContext(vm, org.crayonlang.interpreter.vm.CrayonWrapper.vm_getCurrentExecutionContextId(vm));
    GameWindow.initializeScreen(((int) args[0].internalValue), ((int) args[1].internalValue), ((int) args[2].internalValue), ((int) args[3].internalValue), org.crayonlang.interpreter.vm.CrayonWrapper.vm_getCurrentExecutionContextId(vm));
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
      java.lang.reflect.Method dictLookup0 = functionLookup.get(name);
      fn = dictLookup0 == null ? (functionLookup.containsKey(name) ? null : (null)) : dictLookup0;
      if ((fn != null)) {
        org.crayonlang.interpreter.vm.CrayonWrapper.registerNamedCallback(vm, "Game", name, fn);
      }
      i += 1;
    }
    return vm.globalNull;
  }

  public static Value lib_gamepad_platform_requires_refresh(VmContext vm, Value[] args) {
    if ((false && TranslationHelper.alwaysFalse())) {
      return vm.globalTrue;
    }
    return vm.globalFalse;
  }
}
