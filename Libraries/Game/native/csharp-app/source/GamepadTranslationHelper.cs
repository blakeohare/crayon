using OpenTK.Input;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Game
{
    public static class GamepadTranslationHelper
    {
        private class CrayonGamepad
        {
            private enum ButtonTypes
            {
                A, B, X, Y,
                LEFT_SHOULDER, RIGHT_SHOULDER,
                LEFT_STICK, RIGHT_STICK,
                BACK,
                BIG_BUTTON,
                START,
            }

            private enum AxisTypes
            {
                LEFT_THUMB_X,
                LEFT_THUMB_Y,
                RIGHT_THUMB_X,
                RIGHT_THUMB_Y,
                LEFT_TRIGGER,
                RIGHT_TRIGGER,
            }

            private GamePadState state;
            private readonly ButtonTypes[] buttonNames;
            private readonly AxisTypes[] axisNames;
            private readonly int index;

            public CrayonGamepad(int index)
            {
                this.index = index;
                GamePadCapabilities cpbl = GamePad.GetCapabilities(index);
                this.Name = GamePad.GetName(index);
                List<ButtonTypes> buttons = new List<ButtonTypes>();
                if (cpbl.HasAButton) buttons.Add(ButtonTypes.A);
                if (cpbl.HasBButton) buttons.Add(ButtonTypes.B);
                if (cpbl.HasXButton) buttons.Add(ButtonTypes.X);
                if (cpbl.HasYButton) buttons.Add(ButtonTypes.Y);
                if (cpbl.HasLeftShoulderButton) buttons.Add(ButtonTypes.LEFT_SHOULDER);
                if (cpbl.HasRightShoulderButton) buttons.Add(ButtonTypes.RIGHT_SHOULDER);
                if (cpbl.HasLeftStickButton) buttons.Add(ButtonTypes.LEFT_STICK);
                if (cpbl.HasRightStickButton) buttons.Add(ButtonTypes.RIGHT_STICK);
                if (cpbl.HasBackButton) buttons.Add(ButtonTypes.BACK);
                if (cpbl.HasBigButton) buttons.Add(ButtonTypes.BIG_BUTTON);
                if (cpbl.HasStartButton) buttons.Add(ButtonTypes.START);
                this.buttonNames = buttons.ToArray();

                this.DPadCount = cpbl.HasDPadDownButton ? 1 : 0;

                List<AxisTypes> axes = new List<AxisTypes>();
                if (cpbl.HasLeftXThumbStick) { axes.Add(AxisTypes.LEFT_THUMB_X); axes.Add(AxisTypes.LEFT_THUMB_Y); }
                if (cpbl.HasRightXThumbStick) { axes.Add(AxisTypes.RIGHT_THUMB_X); axes.Add(AxisTypes.RIGHT_THUMB_Y); }
                if (cpbl.HasLeftTrigger) axes.Add(AxisTypes.LEFT_TRIGGER);
                if (cpbl.HasRightTrigger) axes.Add(AxisTypes.RIGHT_TRIGGER);
                this.axisNames = axes.ToArray();
                this.AxisCount = this.axisNames.Length;
            }

            public void RefreshState()
            {
                this.state = GamePad.GetState(this.index);
            }

            public string Name { get; private set; }

            public int DPadCount { get; private set; }
            public int[] GetDPad(int dpadIndex)
            {
                if (dpadIndex >= this.DPadCount) throw new System.IndexOutOfRangeException();
                GamePadDPad dpad = this.state.DPad;
                return new int[] {
                    dpad.Left == ButtonState.Pressed ? -1 : (dpad.Right == ButtonState.Pressed ? 1 : 0),
                    dpad.Up == ButtonState.Pressed ? -1 : (dpad.Down == ButtonState.Pressed ? 1 : 0),
                };
            }

            public int AxisCount { get; private set; }
            public double GetAxis(int axisIndex)
            {
                switch (this.axisNames[axisIndex])
                {
                    case AxisTypes.LEFT_THUMB_X: return this.state.ThumbSticks.Left.X;
                    case AxisTypes.LEFT_THUMB_Y: return this.state.ThumbSticks.Left.Y;
                    case AxisTypes.RIGHT_THUMB_X: return this.state.ThumbSticks.Right.X;
                    case AxisTypes.RIGHT_THUMB_Y: return this.state.ThumbSticks.Right.Y;
                    case AxisTypes.LEFT_TRIGGER: return this.state.Triggers.Left;
                    case AxisTypes.RIGHT_TRIGGER: return this.state.Triggers.Right;
                    default: throw new System.NotImplementedException();
                }
            }

            public int ButtonCount { get { return this.buttonNames.Length; } }
            private ButtonState GetButtonState(int btnIndex)
            {
                GamePadButtons b = this.state.Buttons;
                switch (this.buttonNames[btnIndex])
                {
                    case ButtonTypes.A: return b.A;
                    case ButtonTypes.B: return b.B;
                    case ButtonTypes.BACK: return b.Back;
                    case ButtonTypes.BIG_BUTTON: return b.BigButton;
                    case ButtonTypes.LEFT_SHOULDER: return b.LeftShoulder;
                    case ButtonTypes.LEFT_STICK: return b.LeftStick;
                    case ButtonTypes.RIGHT_SHOULDER: return b.RightShoulder;
                    case ButtonTypes.RIGHT_STICK: return b.RightStick;
                    case ButtonTypes.START: return b.Start;
                    case ButtonTypes.X: return b.X;
                    case ButtonTypes.Y: return b.Y;
                    default: throw new System.NotImplementedException();
                }
            }

            public bool GetButton(int index)
            {
                return this.GetButtonState(index) == ButtonState.Pressed;
            }
        }

        private static CrayonGamepad[] gamepads = null;

        private static void InitializeJoysticks()
        {
            if (GamepadTranslationHelper.gamepads == null)
            {
                List<CrayonGamepad> gamepads = new List<CrayonGamepad>();
                for (int i = 0; i < 256; ++i)
                {
                    GamePadCapabilities gpc = GamePad.GetCapabilities(i);
                    if (!gpc.IsMapped)
                    {
                        break;
                    }
                    gamepads.Add(new CrayonGamepad(i));
                }
                GamepadTranslationHelper.gamepads = gamepads.ToArray();
            }
        }

        public static void Initialize(object nativeDevice)
        {
            // no initialization necessary
        }

        public static void Poll()
        {
            foreach (CrayonGamepad cgp in gamepads)
            {
                cgp.RefreshState();
            }
        }

        public static int GetCurrentDeviceCount()
        {
            InitializeJoysticks();

            return gamepads.Length;
        }

        public static string GetDeviceName(object nativeDevice)
        {
            return ((CrayonGamepad)nativeDevice).Name;
        }

        public static object GetDeviceReference(int index)
        {
            InitializeJoysticks();

            return gamepads[index];
        }

        public static int GetDeviceButtonCount(object nativeDevice)
        {
            return ((CrayonGamepad)nativeDevice).ButtonCount;
        }

        public static bool GetDeviceButtonState(object nativeDevice, int buttonIndex)
        {
            return ((CrayonGamepad)nativeDevice).GetButton(buttonIndex);
        }

        public static int GetDeviceAxis1dCount(object nativeDevice)
        {
            return ((CrayonGamepad)nativeDevice).AxisCount;
        }

        public static double GetDeviceAxis1dState(object nativeDevice, int axisIndex)
        {
            return ((CrayonGamepad)nativeDevice).GetAxis(axisIndex);
        }

        public static int GetDeviceAxis2dCount(object nativeDevice)
        {
            return ((CrayonGamepad)nativeDevice).DPadCount;
        }

        public static void GetDeviceAxis2dState(object nativeDevice, int axisIndex, int[] intBuffer)
        {
            int[] state = ((CrayonGamepad)nativeDevice).GetDPad(axisIndex);
            intBuffer[0] = state[0];
            intBuffer[1] = state[1];
        }
    }
}
