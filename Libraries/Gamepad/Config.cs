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
					throw new ArgumentException();

				case PlatformId.JAVASCRIPT_CANVAS:
					break;

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
