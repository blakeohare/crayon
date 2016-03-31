using System;
using System.Collections.Generic;
using LibraryConfig;

namespace Gamepad
{
	public class Config : ILibraryConfig
	{
		private static string ReadFile(string path)
		{
			return LibraryUtil.ReadEmbeddedTextResource(typeof(Config).Assembly, path);
		}

		public string GetEmbeddedCode()
		{
			string code = ReadFile("embed.cry");

			// TODO: somehow pass an IPlatform into this function and do replacements
			// Would like to avoid needing to modify this each time I add a platform that DOESN'T support gamepad.

			return code;
		}

		public string GetTranslationCode(IPlatform platform, string functionName)
		{
			return ReadFile("Translation/" + functionName + ".cry");
		}

		public string TranslateNativeInvocation(IPlatform translator, string functionName, object[] args)
		{
			switch (translator.PlatformId)
			{
				case PlatformId.CSHARP_OPENTK:
					switch (functionName)
					{
						case "$_lib_gamepad_is_supported":
							return "TranslationHelper.AlwaysTrue()";

						case "$_lib_gamepad_platform_requires_refresh":
							return "TranslationHelper.AlwaysFalse()";

						case "$_lib_gamepad_get_current_device_count":
							return "GamepadTranslationHelper.GetCurrentDeviceCount()";

						case "$_lib_gamepad_get_device_reference":
							return "GamepadTranslationHelper.GetDeviceReference(" + translator.Translate(args[0]) + ")";

						case "$_lib_gamepad_get_name":
							return "GamepadTranslationHelper.GetDeviceName(" + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_button_count":
							return "GamepadTranslationHelper.GetDeviceButtonCount(" + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_axis_1d_count":
							return "GamepadTranslationHelper.GetDeviceAxis1dCount(" + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_axis_2d_count":
							return "GamepadTranslationHelper.GetDeviceAxis2dCount(" + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_button_state":
							return "GamepadTranslationHelper.GetDeviceButtonState(" + translator.Translate(args[0]) + ", " + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_axis_1d_state":
							return "GamepadTranslationHelper.GetDeviceAxis1dState(" + translator.Translate(args[0]) + ", " + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_axis_2d_state":
							return "GamepadTranslationHelper.GetDeviceAxis2dState(" + translator.Translate(args[0]) + ", " + translator.Translate(args[1]) + ", " + translator.Translate(args[2]) + ")";

						case "$_lib_gamepad_poll_universe":
							return "GamepadTranslationHelper.Poll()";

						default:
							throw new ArgumentException();
					}

				case PlatformId.PYTHON_PYGAME:
					switch (functionName)
					{
						case "$_lib_gamepad_is_supported":
							return "_always_true()";

						case "$_lib_gamepad_platform_requires_refresh":
							return "_always_false()";

						case "$_lib_gamepad_get_current_device_count":
							return "_lib_gamepad_get_current_joystick_count()";

						case "$_lib_gamepad_get_device_reference":
							return "_lib_gamepad_get_joystick(" + translator.Translate(args[0]) + ")";

						case "$_lib_gamepad_get_name":
							return "_lib_gamepad_get_joystick_name(" + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_button_count":
							return "_lib_gamepad_get_joystick_button_count(" + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_axis_1d_count":
							return "_lib_gamepad_get_joystick_axis_1d_count(" + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_axis_2d_count":
							return "_lib_gamepad_get_joystick_axis_2d_count(" + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_button_state":
							return "_lib_gamepad_get_joystick_button_state(" + translator.Translate(args[0]) + ", " + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_axis_1d_state":
							return "_lib_gamepad_get_joystick_axis_1d_state(" + translator.Translate(args[0]) + ", " + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_axis_2d_state":
							return "_lib_gamepad_get_joystick_axis_2d_state(" + translator.Translate(args[0]) + ", " + translator.Translate(args[1]) + ", " + translator.Translate(args[2]) + ")";

						case "$_lib_gamepad_poll_universe":
							return "_always_true()"; // no equivalent. Hardware refresh happens in real-time in PyGame without explicitly invoking it.

						default:
							throw new ArgumentException();
					}

				case PlatformId.JAVASCRIPT_CANVAS:
					switch (functionName)
					{
						case "$_lib_gamepad_is_supported":
							return "R.gamepad.isSupported()";

						case "$_lib_gamepad_platform_requires_refresh":
							return "R.gamepad.isSupported()";

						case "$_lib_gamepad_get_current_device_count":
							return "R.gamepad.getDeviceCount()";

						case "$_lib_gamepad_get_device_reference":
							return "R.gamepad.getDevice(" + translator.Translate(args[0]) + ")";

						case "$_lib_gamepad_get_name":
							return "R.gamepad.getName(" + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_button_count":
							return "R.gamepad.getButtonCount(" + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_axis_1d_count":
							return "R.gamepad.getAxisCount(" + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_axis_2d_count":
							return "0";

						case "$_lib_gamepad_get_button_state":
							return "R.gamepad.getButtonState(" + translator.Translate(args[0]) + ", " + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_axis_1d_state":
							return "R.gamepad.getAxisState(" + translator.Translate(args[0]) + ", " + translator.Translate(args[1]) + ")";

						case "$_lib_gamepad_get_axis_2d_state":
							return "0"; // translated but never used.

						case "$_lib_gamepad_poll_universe":
							return "R.alwaysTrue()"; // no equivalent. Hardware refresh happens in real-time in PyGame without explicitly invoking it.

						default:
							throw new ArgumentException();
					}

				default:
					// %%%LIB_GAMEPAD_ENABLED%%% should have hidden away all code that would have called this.
					throw new InvalidOperationException();
			}
			throw new Exception();
		}

		public Dictionary<string, string> GetSupplementalTranslatedCode()
		{
			return new Dictionary<string, string>();
		}
	}
}
