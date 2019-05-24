using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Game
{
    public static class LibraryWrapper
    {
        public static bool AlwaysFalse() { return false; }

        public static bool AlwaysTrue() { return true; }

        private static readonly int[] PST_IntBuffer16 = new int[16];

        public static int lib_audio_load_sfx_from_resourceImpl(ObjectInstance obj, string path)
        {
            object sfx = Libraries.Game.AudioHelper.GetSoundInstance(path);
            obj.nativeData = new object[1];
            obj.nativeData[0] = sfx;
            return 1;
        }

        public static bool lib_audio_music_load_from_resourceImpl(ObjectInstance musicObj, string path)
        {
            object nativeMusicObject = Libraries.Game.AudioHelper.MusicLoadResource(path);
            if ((nativeMusicObject != null))
            {
                musicObj.nativeData = new object[1];
                musicObj.nativeData[0] = nativeMusicObject;
                return true;
            }
            return false;
        }

        public static int lib_audio_music_playImpl(ObjectInstance musicObject, bool isResource, string path, double startingVolume, bool isLoop)
        {
            Libraries.Game.AudioHelper.MusicSetVolume(startingVolume);
            object nativeObject = null;
            if ((musicObject.nativeData != null))
            {
                nativeObject = musicObject.nativeData[0];
            }
            if (isResource)
            {
                Libraries.Game.AudioHelper.AudioMusicPlay(nativeObject, isLoop);
            }
            else
            {
                if (!System.IO.File.Exists(path))
                {
                    return -1;
                }
                Libraries.Game.AudioHelper.AudioMusicPlay(nativeObject, isLoop);
            }
            return 0;
        }

        public static int lib_audio_sfx_get_stateImpl(object channel, object sfxResource, int resourceId)
        {
            return Libraries.Game.AudioHelper.AudioSoundGetState(channel, sfxResource, resourceId);
        }

        public static int lib_audio_sfx_launch(object sfxResource, object[] channelNativeDataOut, double volume, double pan)
        {
            object channel = Libraries.Game.AudioHelper.AudioSoundPlay(sfxResource, volume, pan);
            if ((channel == null))
            {
                return 0;
            }
            channelNativeDataOut[0] = channel;
            return 1;
        }

        public static int lib_audio_sfx_set_panImpl(object channel, object sfxResource, double pan)
        {
            return 0;
        }

        public static int lib_audio_sfx_set_volumeImpl(object channel, object sfxResource, double volume)
        {
            return 0;
        }

        public static int lib_audio_sfx_stopImpl(object channel, object resource, int resourceId, bool isActivelyPlaying, bool hardStop)
        {
            Libraries.Game.AudioHelper.AudioSoundStop(channel, resource, resourceId, isActivelyPlaying, hardStop);
            return 0;
        }

        public static int lib_audio_sfx_unpause(object channel, object sfxResource, double volume, double pan)
        {
            Libraries.Game.AudioHelper.AudioSoundResume(channel, sfxResource, volume, pan);
            return 0;
        }

        public static int lib_audio_stop(object sound, bool reset)
        {
            Libraries.Game.AudioHelper.AudioStop(sound);
            return 0;
        }

        public static Value lib_game_audio_getAudioResourcePath(VmContext vm, Value[] args)
        {
            return Interpreter.Vm.CrayonWrapper.resource_manager_getResourceOfType(vm, (string)args[0].internalValue, "SND");
        }

        public static Value lib_game_audio_is_supported(VmContext vm, Value[] args)
        {
            if (AlwaysTrue())
            {
                return vm.globals.boolTrue;
            }
            return vm.globals.boolFalse;
        }

        public static Value lib_game_audio_music_is_playing(VmContext vm, Value[] args)
        {
            if (Libraries.Game.AudioHelper.AudioMusicIsPlaying())
            {
                return vm.globals.boolTrue;
            }
            return vm.globals.boolFalse;
        }

        public static Value lib_game_audio_music_load_from_file(VmContext vm, Value[] args)
        {
            return vm.globals.valueNull;
        }

        public static Value lib_game_audio_music_load_from_resource(VmContext vm, Value[] args)
        {
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            if (lib_audio_music_load_from_resourceImpl(objInstance1, (string)args[1].internalValue))
            {
                return vm.globals.boolTrue;
            }
            return vm.globals.boolFalse;
        }

        public static Value lib_game_audio_music_play(VmContext vm, Value[] args)
        {
            return Interpreter.Vm.CrayonWrapper.buildBoolean(vm.globals, (lib_audio_music_playImpl((ObjectInstance)args[0].internalValue, (bool)args[1].internalValue, (string)args[2].internalValue, (double)args[3].internalValue, (bool)args[4].internalValue) != -1));
        }

        public static Value lib_game_audio_music_set_volume(VmContext vm, Value[] args)
        {
            Libraries.Game.AudioHelper.MusicSetVolume((double)args[0].internalValue);
            return vm.globals.valueNull;
        }

        public static Value lib_game_audio_music_stop(VmContext vm, Value[] args)
        {
            return vm.globals.valueNull;
        }

        public static Value lib_game_audio_sfx_get_state(VmContext vm, Value[] args)
        {
            ObjectInstance channelInstance = (ObjectInstance)args[0].internalValue;
            object nativeChannel = channelInstance.nativeData[0];
            ObjectInstance soundInstance = (ObjectInstance)args[1].internalValue;
            object nativeSound = soundInstance.nativeData[0];
            int resourceId = (int)args[2].internalValue;
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, lib_audio_sfx_get_stateImpl(nativeChannel, nativeSound, resourceId));
        }

        public static Value lib_game_audio_sfx_load_from_file(VmContext vm, Value[] args)
        {
            return vm.globals.valueNull;
        }

        public static Value lib_game_audio_sfx_load_from_resource(VmContext vm, Value[] args)
        {
            ObjectInstance soundInstance = (ObjectInstance)args[0].internalValue;
            lib_audio_load_sfx_from_resourceImpl(soundInstance, (string)args[1].internalValue);
            return vm.globals.valueNull;
        }

        public static Value lib_game_audio_sfx_play(VmContext vm, Value[] args)
        {
            ObjectInstance channelInstance = (ObjectInstance)args[0].internalValue;
            ObjectInstance resourceInstance = (ObjectInstance)args[1].internalValue;
            channelInstance.nativeData = new object[1];
            object nativeResource = resourceInstance.nativeData[0];
            double vol = (double)args[2].internalValue;
            double pan = (double)args[3].internalValue;
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, lib_audio_sfx_launch(nativeResource, channelInstance.nativeData, vol, pan));
        }

        public static Value lib_game_audio_sfx_resume(VmContext vm, Value[] args)
        {
            ObjectInstance sndInstance = (ObjectInstance)args[0].internalValue;
            object nativeSound = sndInstance.nativeData[0];
            ObjectInstance sndResInstance = (ObjectInstance)args[1].internalValue;
            object nativeResource = sndResInstance.nativeData[0];
            double vol = (double)args[2].internalValue;
            double pan = (double)args[3].internalValue;
            lib_audio_sfx_unpause(nativeSound, nativeResource, vol, pan);
            return vm.globals.valueNull;
        }

        public static Value lib_game_audio_sfx_set_pan(VmContext vm, Value[] args)
        {
            ObjectInstance channel = (ObjectInstance)args[0].internalValue;
            object nativeChannel = channel.nativeData[0];
            ObjectInstance resource = (ObjectInstance)args[1].internalValue;
            object nativeResource = resource.nativeData[0];
            lib_audio_sfx_set_panImpl(nativeChannel, nativeResource, (double)args[2].internalValue);
            return vm.globals.valueNull;
        }

        public static Value lib_game_audio_sfx_set_volume(VmContext vm, Value[] args)
        {
            ObjectInstance channel = (ObjectInstance)args[0].internalValue;
            object nativeChannel = channel.nativeData[0];
            ObjectInstance resource = (ObjectInstance)args[1].internalValue;
            object nativeResource = resource.nativeData[0];
            lib_audio_sfx_set_volumeImpl(nativeChannel, nativeResource, (double)args[2].internalValue);
            return vm.globals.valueNull;
        }

        public static Value lib_game_audio_sfx_stop(VmContext vm, Value[] args)
        {
            ObjectInstance channel = (ObjectInstance)args[0].internalValue;
            object nativeChannel = channel.nativeData[0];
            ObjectInstance resource = (ObjectInstance)args[1].internalValue;
            object nativeResource = resource.nativeData[0];
            int resourceId = (int)args[2].internalValue;
            int currentState = (int)args[3].internalValue;
            bool completeStopAndFreeChannel = (bool)args[4].internalValue;
            bool isAlreadyPaused = ((currentState == 2) && !completeStopAndFreeChannel);
            if (((currentState != 3) && !isAlreadyPaused))
            {
                lib_audio_sfx_stopImpl(nativeChannel, nativeResource, resourceId, (currentState == 1), completeStopAndFreeChannel);
            }
            return vm.globals.valueNull;
        }

        public static Value lib_game_clock_tick(VmContext vm, Value[] args)
        {
            AlwaysTrue();
            Interpreter.Vm.CrayonWrapper.vm_suspend_context_by_id(vm, (int)args[0].internalValue, 1);
            return vm.globalNull;
        }

        public static Value lib_game_gamepad_current_device_count(VmContext vm, Value[] args)
        {
            int total = 0;
            if ((true && AlwaysTrue()))
            {
                total = Libraries.Game.GamepadTranslationHelper.GetCurrentDeviceCount();
            }
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, total);
        }

        public static Value lib_game_gamepad_get_axis_1d_state(VmContext vm, Value[] args)
        {
            int index = (int)args[1].internalValue;
            ObjectInstance dev = (ObjectInstance)args[0].internalValue;
            return Interpreter.Vm.CrayonWrapper.buildFloat(vm.globals, Libraries.Game.GamepadTranslationHelper.GetDeviceAxis1dState(dev.nativeData[0], index));
        }

        public static Value lib_game_gamepad_get_axis_2d_state(VmContext vm, Value[] args)
        {
            Value arg1 = args[0];
            Value arg2 = args[1];
            Value arg3 = args[2];
            ObjectInstance objInstance1 = (ObjectInstance)arg1.internalValue;
            int int1 = (int)arg2.internalValue;
            ListImpl list1 = (ListImpl)arg3.internalValue;
            Libraries.Game.GamepadTranslationHelper.GetDeviceAxis2dState(objInstance1.nativeData[0], int1, PST_IntBuffer16);
            Interpreter.Vm.CrayonWrapper.clearList(list1);
            Interpreter.Vm.CrayonWrapper.addToList(list1, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, PST_IntBuffer16[0]));
            Interpreter.Vm.CrayonWrapper.addToList(list1, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, PST_IntBuffer16[1]));
            return vm.globalNull;
        }

        public static Value lib_game_gamepad_get_button_state(VmContext vm, Value[] args)
        {
            int int1 = 0;
            ObjectInstance objInstance1 = null;
            Value arg1 = args[0];
            Value arg2 = args[1];
            objInstance1 = (ObjectInstance)arg1.internalValue;
            int1 = (int)arg2.internalValue;
            if (Libraries.Game.GamepadTranslationHelper.GetDeviceButtonState(objInstance1.nativeData[0], int1))
            {
                return vm.globalTrue;
            }
            return vm.globalFalse;
        }

        public static Value lib_game_gamepad_get_save_file_path(VmContext vm, Value[] args)
        {
            string string1 = ".crayon-csotk.gamepad.config";
            if ((string1 != null))
            {
                return Interpreter.Vm.CrayonWrapper.buildString(vm.globals, string1);
            }
            return vm.globalNull;
        }

        public static Value lib_game_gamepad_getPlatform(VmContext vm, Value[] args)
        {
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, 1);
        }

        public static Value lib_game_gamepad_initialize_device(VmContext vm, Value[] args)
        {
            int int1 = (int)args[0].internalValue;
            ObjectInstance objInstance1 = (ObjectInstance)args[1].internalValue;
            ListImpl list1 = (ListImpl)args[2].internalValue;
            object object1 = Libraries.Game.GamepadTranslationHelper.GetDeviceReference(int1);
            objInstance1.nativeData = new object[1];
            objInstance1.nativeData[0] = object1;
            Interpreter.Vm.CrayonWrapper.clearList(list1);
            Interpreter.Vm.CrayonWrapper.addToList(list1, Interpreter.Vm.CrayonWrapper.buildString(vm.globals, Libraries.Game.GamepadTranslationHelper.GetDeviceName(object1)));
            Interpreter.Vm.CrayonWrapper.addToList(list1, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, Libraries.Game.GamepadTranslationHelper.GetDeviceButtonCount(object1)));
            Interpreter.Vm.CrayonWrapper.addToList(list1, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, Libraries.Game.GamepadTranslationHelper.GetDeviceAxis1dCount(object1)));
            Interpreter.Vm.CrayonWrapper.addToList(list1, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, Libraries.Game.GamepadTranslationHelper.GetDeviceAxis2dCount(object1)));
            return vm.globalNull;
        }

        public static Value lib_game_gamepad_is_supported(VmContext vm, Value[] args)
        {
            if ((true && AlwaysTrue()))
            {
                return vm.globalTrue;
            }
            else
            {
                return vm.globalFalse;
            }
        }

        public static Value lib_game_gamepad_jsIsOsx(VmContext vm, Value[] args)
        {
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, 0);
        }

        public static Value lib_game_gamepad_platform_requires_refresh(VmContext vm, Value[] args)
        {
            if ((true && AlwaysFalse()))
            {
                return vm.globalTrue;
            }
            return vm.globalFalse;
        }

        public static Value lib_game_gamepad_poll_universe(VmContext vm, Value[] args)
        {
            Libraries.Game.GamepadTranslationHelper.Poll();
            return vm.globalNull;
        }

        public static Value lib_game_gamepad_refresh_devices(VmContext vm, Value[] args)
        {
            AlwaysTrue();
            return vm.globalNull;
        }

        public static Value lib_game_getScreenInfo(VmContext vm, Value[] args)
        {
            Value outList = args[0];
            int[] o = PST_IntBuffer16;
            if (GameWindow.GetScreenInfo(o))
            {
                ListImpl output = (ListImpl)outList.internalValue;
                Interpreter.Vm.CrayonWrapper.clearList(output);
                Interpreter.Vm.CrayonWrapper.addToList(output, Interpreter.Vm.CrayonWrapper.buildBoolean(vm.globals, (o[0] == 1)));
                Interpreter.Vm.CrayonWrapper.addToList(output, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, o[1]));
                Interpreter.Vm.CrayonWrapper.addToList(output, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, o[2]));
            }
            return outList;
        }

        public static Value lib_game_getTouchState(VmContext vm, Value[] args)
        {
            ListImpl output = (ListImpl)args[0].internalValue;
            int[] data = new int[31];
            data[0] = 0;
            AlwaysTrue();
            int _len = data[0];
            int end = ((_len * 3) + 1);
            int i = 1;
            while ((i < end))
            {
                Interpreter.Vm.CrayonWrapper.addToList(output, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, data[i]));
                i += 1;
            }
            return vm.globalNull;
        }

        public static Value lib_game_initialize(VmContext vm, Value[] args)
        {
            GameWindow.FPS = Interpreter.Vm.CrayonWrapper.getFloat(args[0]);
            return vm.globalNull;
        }

        public static Value lib_game_initialize_screen(VmContext vm, Value[] args)
        {
            int ecId = (int)args[4].internalValue;
            ExecutionContext ec = Interpreter.Vm.CrayonWrapper.getExecutionContext(vm, ecId);
            GameWindow.InitializeScreen((int)args[0].internalValue, (int)args[1].internalValue, (int)args[2].internalValue, (int)args[3].internalValue, ecId);
            Interpreter.Vm.CrayonWrapper.vm_suspend_for_context(ec, 1);
            return vm.globalNull;
        }

        public static Value lib_game_pump_events(VmContext vm, Value[] args)
        {
            ListImpl output = (ListImpl)args[0].internalValue;
            List<PlatformRelayObject> eventList = GameWindow.Instance.GetEvents();
            VmGlobals globals = vm.globals;
            Interpreter.Vm.CrayonWrapper.clearList(output);
            int _len = eventList.Count;
            if ((_len > 0))
            {
                int i = 0;
                i = 0;
                while ((i < _len))
                {
                    PlatformRelayObject ev = eventList[i];
                    Interpreter.Vm.CrayonWrapper.addToList(output, Interpreter.Vm.CrayonWrapper.buildInteger(globals, ev.type));
                    int t = ev.type;
                    Interpreter.Vm.CrayonWrapper.addToList(output, Interpreter.Vm.CrayonWrapper.buildInteger(globals, ev.iarg1));
                    if ((t >= 32))
                    {
                        Interpreter.Vm.CrayonWrapper.addToList(output, Interpreter.Vm.CrayonWrapper.buildInteger(globals, ev.iarg2));
                        if ((t == 37))
                        {
                            Interpreter.Vm.CrayonWrapper.addToList(output, Interpreter.Vm.CrayonWrapper.buildFloat(globals, ev.farg1));
                        }
                        else
                        {
                            if (((t >= 64) && (t < 80)))
                            {
                                Interpreter.Vm.CrayonWrapper.addToList(output, Interpreter.Vm.CrayonWrapper.buildInteger(globals, ev.iarg3));
                            }
                        }
                    }
                    i += 1;
                }
            }
            return args[0];
        }

        public static Value lib_game_set_title(VmContext vm, Value[] args)
        {
            GameWindow.Instance.SetTitle((string)args[0].internalValue);
            return vm.globalNull;
        }

        public static Value lib_game_set_window_mode(VmContext vm, Value[] args)
        {
            int mode = (int)args[0].internalValue;
            int width = (int)args[1].internalValue;
            int height = (int)args[2].internalValue;
            GameWindow.Instance.SetScreenMode(mode, width, height);
            return vm.globalNull;
        }

        public static Value lib_game_setInstance(VmContext vm, Value[] args)
        {
            ObjectInstance o = (ObjectInstance)args[0].internalValue;
            object[] nd = new object[1];
            nd[0] = GameWindow.Instance;
            o.nativeData = nd;
            return vm.globalNull;
        }

        public static Value lib_game_startup(VmContext vm, Value[] args)
        {
            Dictionary<string, System.Func<object[], object>> functionLookup = GameWindow.GetCallbackFunctions();
            string[] names = functionLookup.Keys.ToArray();
            int i = 0;
            while ((i < names.Length))
            {
                System.Func<object[], object> fn = null;
                string name = names[i];
                if (!functionLookup.TryGetValue(name, out fn)) fn = null;
                if ((fn != null))
                {
                    Interpreter.Vm.CrayonWrapper.registerNamedCallback(vm, "Game", name, fn);
                }
                i += 1;
            }
            return vm.globalNull;
        }
    }
}
