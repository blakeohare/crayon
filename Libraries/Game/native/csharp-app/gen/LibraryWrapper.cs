namespace Interpreter.Libraries.Game
{
    public class LibraryWrapper
    {
        private static readonly int[] PST_IntBuffer16 = new int[16];
        private static readonly double[] PST_FloatBuffer16 = new double[16];
        private static readonly string[] PST_StringBuffer16 = new string[16];
        private static readonly System.Random PST_Random = new System.Random();

        public static bool AlwaysTrue() { return true; }
        public static bool AlwaysFalse() { return false; }

        public static string PST_StringReverse(string value)
        {
            if (value.Length < 2) return value;
            char[] chars = value.ToCharArray();
            return new string(chars.Reverse().ToArray());
        }

        private static readonly string[] PST_SplitSep = new string[1];
        private static string[] PST_StringSplit(string value, string sep)
        {
            if (sep.Length == 1) return value.Split(sep[0]);
            if (sep.Length == 0) return value.ToCharArray().Select<char, string>(c => "" + c).ToArray();
            PST_SplitSep[0] = sep;
            return value.Split(PST_SplitSep, StringSplitOptions.None);
        }

        private static string PST_FloatToString(double value)
        {
            string output = value.ToString();
            if (output[0] == '.') output = "0" + output;
            if (!output.Contains('.')) output += ".0";
            return output;
        }

        private static readonly DateTime PST_UnixEpoch = new System.DateTime(1970, 1, 1);
        private static double PST_CurrentTime
        {
            get { return System.DateTime.UtcNow.Subtract(PST_UnixEpoch).TotalSeconds; }
        }

        private static string PST_Base64ToString(string b64Value)
        {
            byte[] utf8Bytes = System.Convert.FromBase64String(b64Value);
            string value = System.Text.Encoding.UTF8.GetString(utf8Bytes);
            return value;
        }

        // TODO: use a model like parse float to avoid double parsing.
        public static bool PST_IsValidInteger(string value)
        {
            if (value.Length == 0) return false;
            char c = value[0];
            if (value.Length == 1) return c >= '0' && c <= '9';
            int length = value.Length;
            for (int i = c == '-' ? 1 : 0; i < length; ++i)
            {
                c = value[i];
                if (c < '0' || c > '9') return false;
            }
            return true;
        }

        public static void PST_ParseFloat(string strValue, double[] output)
        {
            double num = 0.0;
            output[0] = double.TryParse(strValue, out num) ? 1 : -1;
            output[1] = num;
        }

        private static List<T> PST_ListConcat<T>(List<T> a, List<T> b)
        {
            List<T> output = new List<T>(a.Count + b.Count);
            output.AddRange(a);
            output.AddRange(b);
            return output;
        }

        private static List<Value> PST_MultiplyList(List<Value> items, int times)
        {
            List<Value> output = new List<Value>(items.Count * times);
            while (times-- > 0) output.AddRange(items);
            return output;
        }

        private static bool PST_SubstringIsEqualTo(string haystack, int index, string needle)
        {
            int needleLength = needle.Length;
            if (index + needleLength > haystack.Length) return false;
            if (needleLength == 0) return true;
            if (haystack[index] != needle[0]) return false;
            if (needleLength == 1) return true;
            for (int i = 1; i < needleLength; ++i)
            {
                if (needle[i] != haystack[index + i]) return false;
            }
            return true;
        }

        private static void PST_ShuffleInPlace<T>(List<T> list)
        {
            if (list.Count < 2) return;
            int length = list.Count;
            int tIndex;
            T tValue;
            for (int i = length - 1; i >= 0; --i)
            {
                tIndex = PST_Random.Next(length);
                tValue = list[tIndex];
                list[tIndex] = list[i];
                list[i] = tValue;
            }
        }

        public static Value lib_audio_getAudioResourcePath(VmContext vm, Value[] args)
        {
            return Interpreter.Vm.CrayonWrapper.resource_manager_getResourceOfType(vm, (string)args[0].internalValue, "SND");
        }

        public static Value lib_audio_is_supported(VmContext vm, Value[] args)
        {
            if (AlwaysTrue())
            {
                return vm.globals.boolTrue;
            }
            return vm.globals.boolFalse;
        }

        public static int lib_audio_load_sfx_from_resourceImpl(ObjectInstance obj, string path)
        {
            object sfx = Libraries.Game.AudioHelper.GetSoundInstance(path);
            obj.nativeData = new object[1];
            obj.nativeData[0] = sfx;
            return 1;
        }

        public static Value lib_audio_music_is_playing(VmContext vm, Value[] args)
        {
            if (Libraries.Game.AudioHelper.AudioMusicIsPlaying())
            {
                return vm.globals.boolTrue;
            }
            return vm.globals.boolFalse;
        }

        public static Value lib_audio_music_load_from_file(VmContext vm, Value[] args)
        {
            return vm.globals.valueNull;
        }

        public static Value lib_audio_music_load_from_resource(VmContext vm, Value[] args)
        {
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            if (lib_audio_music_load_from_resourceImpl(objInstance1, (string)args[1].internalValue))
            {
                return vm.globals.boolTrue;
            }
            return vm.globals.boolFalse;
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

        public static Value lib_audio_music_play(VmContext vm, Value[] args)
        {
            return Interpreter.Vm.CrayonWrapper.buildBoolean(vm.globals, (lib_audio_music_playImpl((ObjectInstance)args[0].internalValue, (bool)args[1].internalValue, (string)args[2].internalValue, (double)args[3].internalValue, (bool)args[4].internalValue) != -1));
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

        public static Value lib_audio_music_set_volume(VmContext vm, Value[] args)
        {
            Libraries.Game.AudioHelper.MusicSetVolume((double)args[0].internalValue);
            return vm.globals.valueNull;
        }

        public static Value lib_audio_music_stop(VmContext vm, Value[] args)
        {
            return vm.globals.valueNull;
        }

        public static Value lib_audio_sfx_get_state(VmContext vm, Value[] args)
        {
            ObjectInstance channelInstance = (ObjectInstance)args[0].internalValue;
            object nativeChannel = channelInstance.nativeData[0];
            ObjectInstance soundInstance = (ObjectInstance)args[1].internalValue;
            object nativeSound = soundInstance.nativeData[0];
            int resourceId = (int)args[2].internalValue;
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, lib_audio_sfx_get_stateImpl(nativeChannel, nativeSound, resourceId));
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

        public static Value lib_audio_sfx_load_from_file(VmContext vm, Value[] args)
        {
            return vm.globals.valueNull;
        }

        public static Value lib_audio_sfx_load_from_resource(VmContext vm, Value[] args)
        {
            ObjectInstance soundInstance = (ObjectInstance)args[0].internalValue;
            lib_audio_load_sfx_from_resourceImpl(soundInstance, (string)args[1].internalValue);
            return vm.globals.valueNull;
        }

        public static Value lib_audio_sfx_play(VmContext vm, Value[] args)
        {
            ObjectInstance channelInstance = (ObjectInstance)args[0].internalValue;
            ObjectInstance resourceInstance = (ObjectInstance)args[1].internalValue;
            channelInstance.nativeData = new object[1];
            object nativeResource = resourceInstance.nativeData[0];
            double vol = (double)args[2].internalValue;
            double pan = (double)args[3].internalValue;
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, lib_audio_sfx_launch(nativeResource, channelInstance.nativeData, vol, pan));
        }

        public static Value lib_audio_sfx_resume(VmContext vm, Value[] args)
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

        public static Value lib_audio_sfx_set_pan(VmContext vm, Value[] args)
        {
            ObjectInstance channel = (ObjectInstance)args[0].internalValue;
            object nativeChannel = channel.nativeData[0];
            ObjectInstance resource = (ObjectInstance)args[1].internalValue;
            object nativeResource = resource.nativeData[0];
            lib_audio_sfx_set_panImpl(nativeChannel, nativeResource, (double)args[2].internalValue);
            return vm.globals.valueNull;
        }

        public static int lib_audio_sfx_set_panImpl(object channel, object sfxResource, double pan)
        {
            return 0;
        }

        public static Value lib_audio_sfx_set_volume(VmContext vm, Value[] args)
        {
            ObjectInstance channel = (ObjectInstance)args[0].internalValue;
            object nativeChannel = channel.nativeData[0];
            ObjectInstance resource = (ObjectInstance)args[1].internalValue;
            object nativeResource = resource.nativeData[0];
            lib_audio_sfx_set_volumeImpl(nativeChannel, nativeResource, (double)args[2].internalValue);
            return vm.globals.valueNull;
        }

        public static int lib_audio_sfx_set_volumeImpl(object channel, object sfxResource, double volume)
        {
            return 0;
        }

        public static Value lib_audio_sfx_stop(VmContext vm, Value[] args)
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

        public static Value lib_game_clock_tick(VmContext vm, Value[] args)
        {
            AlwaysTrue();
            Interpreter.Vm.CrayonWrapper.vm_suspend(vm, 1);
            return vm.globalNull;
        }

        public static Value lib_game_gamepad_current_device_count(VmContext vm, Value[] args)
        {
            int total = 0;
            if ((false && AlwaysTrue()))
            {
                total = Libraries.Game.GamepadTranslationHelper.GetCurrentDeviceCount();
            }
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, total);
        }

        public static Value lib_game_gamepad_get_axis_1d_state(VmContext vm, Value[] args)
        {
            return vm.globals.floatZero;
        }

        public static Value lib_game_gamepad_get_axis_2d_state(VmContext vm, Value[] args)
        {
            Value arg1 = args[0];
            Value arg2 = args[1];
            Value arg3 = args[2];
            return vm.globalNull;
        }

        public static Value lib_game_gamepad_get_button_state(VmContext vm, Value[] args)
        {
            return vm.globalTrue;
        }

        public static Value lib_game_gamepad_get_save_file_path(VmContext vm, Value[] args)
        {
            return vm.globalNull;
        }

        public static Value lib_game_gamepad_getPlatform(VmContext vm, Value[] args)
        {
            return vm.globalNull;
        }

        public static Value lib_game_gamepad_initialize_device(VmContext vm, Value[] args)
        {
            return vm.globalNull;
        }

        public static Value lib_game_gamepad_is_supported(VmContext vm, Value[] args)
        {
            if ((false && AlwaysTrue()))
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
            return vm.globalNull;
        }

        public static Value lib_game_gamepad_poll_universe(VmContext vm, Value[] args)
        {
            return vm.globalNull;
        }

        public static Value lib_game_gamepad_refresh_devices(VmContext vm, Value[] args)
        {
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
            ExecutionContext ec = Interpreter.Vm.CrayonWrapper.getExecutionContext(vm, Interpreter.Vm.CrayonWrapper.vm_getCurrentExecutionContextId(vm));
            GameWindow.InitializeScreen((int)args[0].internalValue, (int)args[1].internalValue, (int)args[2].internalValue, (int)args[3].internalValue, Interpreter.Vm.CrayonWrapper.vm_getCurrentExecutionContextId(vm));
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
            Dictionary<string, Func<object[], object>> functionLookup = GameWindow.GetCallbackFunctions();
            string[] names = functionLookup.Keys.ToArray();
            int i = 0;
            while ((i < names.Length))
            {
                Func<object[], object> fn = null;
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

        public static Value lib_gamepad_platform_requires_refresh(VmContext vm, Value[] args)
        {
            if ((false && AlwaysFalse()))
            {
                return vm.globalTrue;
            }
            return vm.globalFalse;
        }
    }
}