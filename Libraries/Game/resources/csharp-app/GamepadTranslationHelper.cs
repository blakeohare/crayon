using OpenTK.Input;
using System;
using System.Linq;

namespace Interpreter.Libraries.Game
{
	public static class GamepadTranslationHelper
	{
		private static bool isInitialized = false;
		private static JoystickDevice[] joysticks = null;

		private static void InitializeJoysticks()
		{
			if (!isInitialized)
			{
				joysticks = GameWindow.Instance.Joysticks.ToArray();
			}
		}

		public static void Initialize(object nativeDevice)
		{
			// no initialization necessary
		}
		
		public static void Poll()
		{
#pragma warning disable 612,618
			GameWindow.Instance.InputDriver.Poll();
#pragma warning restore 612,618
		}

		public static int GetCurrentDeviceCount()
		{
			InitializeJoysticks();

			return joysticks.Length;
		}

		public static string GetDeviceName(object nativeDevice)
		{
			JoystickDevice jsDevice = (JoystickDevice)nativeDevice;
			string rawDescription = jsDevice.Description;
			if (rawDescription == null)
			{
				rawDescription = GetDeviceButtonCount(jsDevice) + " Buttons/" + GetDeviceAxis1dCount(jsDevice) + " Axes";
			}
			int ignoreThrough = rawDescription.IndexOf('#');
			if (ignoreThrough != -1)
			{
				string newDescription = rawDescription.Substring(ignoreThrough + 2).Trim();
				if (newDescription.Length != 0)
				{
					rawDescription = "Joystick " + newDescription;
				}
			}
			return rawDescription;
		}

		public static object GetDeviceReference(int index)
		{
			InitializeJoysticks();

			return joysticks[index];
		}

		public static int GetDeviceButtonCount(object nativeDevice)
		{
			return ((JoystickDevice)nativeDevice).Button.Count;
		}

		public static bool GetDeviceButtonState(object nativeDevice, int buttonIndex)
		{
			return ((JoystickDevice)nativeDevice).Button[buttonIndex];
		}

		public static int GetDeviceAxis1dCount(object nativeDevice)
		{
			return ((JoystickDevice)nativeDevice).Axis.Count;
		}

		public static double GetDeviceAxis1dState(object nativeDevice, int axisIndex)
		{
			return ((JoystickDevice)nativeDevice).Axis[axisIndex];
		}

		public static int GetDeviceAxis2dCount(object nativeDevice)
		{
			return 0;
		}

		public static void GetDeviceAxis2dState(object nativeDevice, int axisIndex, object ignored) { }
	}
}
