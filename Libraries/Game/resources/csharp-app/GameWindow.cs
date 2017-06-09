using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using Interpreter.Structs;
using Interpreter.Vm;

namespace Interpreter.Libraries.Game
{
    public class GameWindow : OpenTK.GameWindow
    {
        private const int MOUSE_EVENT = 0;
        private const int KEY_EVENT = 1;
        private const int EXIT = 2;

        private const int MOUSE_LEFT = 0;
        private const int MOUSE_RIGHT = 1;
        private const int MOUSE_MOVE = 2;

        private const int EXIT_ALT_F4 = 1;
        private const int EXIT_CLOSE_BUTTON = 2;

        private Queue<int> events = new Queue<int>();
        private bool altPressed = false;

        private int gameWidth;
        private int gameHeight;
        private int screenWidth;
        private int screenHeight;

        private int executionContextId;

        private static double fps = 60;
        private static GameWindow instance = null;
        public static GameWindow Instance { get { return instance; } }

        public static double FPS
        {
            get { return fps; }
            set { fps = value; }
        }

        private GameWindow(double fps, int gameWidth, int gameHeight, int screenWidth, int screenHeight, int executionContextId)
            : base(screenWidth, screenHeight)
        {
            GameWindow.instance = this;
            this.executionContextId = executionContextId;

            this.gameWidth = gameWidth;
            this.gameHeight = gameHeight;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.X = 50;
            this.Y = 50;

            this.TargetRenderFrequency = fps;

            this.UpdateFrame += (sender, e) => this.Update();
            this.RenderFrame += (sender, e) => this.Render();
            this.Load += (sender, e) => this.Startup();
            this.Resize += (sender, e) => this.Resizing();

            this.Mouse.Move += (sender, e) => this.MouseMove(e.X, e.Y);
            this.Mouse.ButtonDown += (sender, e) => this.MouseButton(e.Button, e.X, e.Y, true);
            this.Mouse.ButtonUp += (sender, e) => this.MouseButton(e.Button, e.X, e.Y, false);
            this.Keyboard.KeyDown += (sender, e) => this.KeyEvent(e.Key, true);
            this.Keyboard.KeyUp += (sender, e) => this.KeyEvent(e.Key, false);

            if (UniversalBitmap.IconSupported)
            {
                UniversalBitmap bmp = ResourceReader.ReadIconResource("icon.ico");
                if (bmp != null)
                {
                    this.Icon = bmp.GenerateIcon();
                }
            }
        }

        public static bool InitializeScreen(int gameWidth, int gameHeight, int screenWidth, int screenHeight, int executionContextId)
        {
            GameWindow gw = new GameWindow(fps, gameWidth, gameHeight, screenWidth, screenHeight, executionContextId);
            gw.Run(GameWindow.FPS, GameWindow.FPS);
            return false;
        }

        private void MouseMove(int x, int y)
        {
            events.Enqueue(MOUSE_EVENT);
            events.Enqueue(x * this.gameWidth / this.screenWidth);
            events.Enqueue(y * this.gameHeight / this.screenHeight);
            events.Enqueue(MOUSE_MOVE);
        }

        private void MouseButton(OpenTK.Input.MouseButton button, int x, int y, bool down)
        {
            // drop events that aren't the left or right buttons. For now.
            bool left = button == OpenTK.Input.MouseButton.Left;
            if (!left && button != OpenTK.Input.MouseButton.Right)
            {
                return;
            }

            events.Enqueue(MOUSE_EVENT);
            events.Enqueue(x * this.gameWidth / this.screenWidth);
            events.Enqueue(y * this.gameHeight / this.screenHeight);
            events.Enqueue(left ? MOUSE_LEFT : MOUSE_RIGHT);
            events.Enqueue(down ? 1 : 0);
        }

        private const int KEY_CODE_A = (int)OpenTK.Input.Key.A;
        private const int KEY_CODE_Z = (int)OpenTK.Input.Key.Z;
        private const int KEY_CODE_F1 = (int)OpenTK.Input.Key.F1;
        private const int KEY_CODE_F12 = (int)OpenTK.Input.Key.F12;
        private const int KEY_CODE_0 = (int)OpenTK.Input.Key.Number0;
        private const int KEY_CODE_9 = (int)OpenTK.Input.Key.Number9;

        private static readonly string[] LETTERS = "a b c d e f g h i j k l m n o p q r s t u v w x y z".Split(' ');
        private static readonly string[] F_KEYS = "f1 f2 f3 f4 f5 f6 f7 f8 f9 f10 f11 f12".Split(' ');
        private void KeyEvent(OpenTK.Input.Key key, bool down)
        {
            int keyCode = (int)key;
            int cc;
            if (keyCode >= KEY_CODE_A && keyCode <= KEY_CODE_Z)
            {
                cc = keyCode - KEY_CODE_A + 65;
            }
            else if (keyCode >= KEY_CODE_F1 && keyCode <= KEY_CODE_F12)
            {
                if (this.altPressed && keyCode == KEY_CODE_F1 + 3)
                {
                    events.Enqueue(EXIT);
                    events.Enqueue(EXIT_ALT_F4);
                }
                cc = keyCode - KEY_CODE_F1 + 112;
            }
            else if (keyCode >= KEY_CODE_0 && keyCode <= KEY_CODE_9)
            {
                cc = keyCode - KEY_CODE_0 + 48;
            }
            else
            {
                switch (key)
                {
                    case OpenTK.Input.Key.Space: cc = 32; break;
                    case OpenTK.Input.Key.Enter:
                    case OpenTK.Input.Key.KeypadEnter: cc = 13; break;
                    case OpenTK.Input.Key.Tab: cc = 9; break;
                    case OpenTK.Input.Key.Escape: cc = 27; break;

                    case OpenTK.Input.Key.Left: cc = 37; break;
                    case OpenTK.Input.Key.Right: cc = 39; break;
                    case OpenTK.Input.Key.Up: cc = 38; break;
                    case OpenTK.Input.Key.Down: cc = 40; break;

                    case OpenTK.Input.Key.Comma: cc = 188; break;
                    case OpenTK.Input.Key.Period: cc = 190; break;
                    case OpenTK.Input.Key.Semicolon: cc = 186; break;
                    case OpenTK.Input.Key.Quote: cc = 222; break;
                    case OpenTK.Input.Key.Slash: cc = 191; break;
                    case OpenTK.Input.Key.BackSlash: cc = 220; break;
                    case OpenTK.Input.Key.BracketLeft: cc = 219; break;
                    case OpenTK.Input.Key.BracketRight: cc = 221; break;
                    case OpenTK.Input.Key.Minus: cc = 189; break;
                    case OpenTK.Input.Key.Plus: cc = 187; break;
                    case OpenTK.Input.Key.Tilde: cc = 192; break;

                    case OpenTK.Input.Key.ControlLeft:
                    case OpenTK.Input.Key.ControlRight: cc = 17; break;
                    case OpenTK.Input.Key.ShiftLeft:
                    case OpenTK.Input.Key.ShiftRight: cc = 16; break;
                    case OpenTK.Input.Key.AltLeft:
                    case OpenTK.Input.Key.AltRight: cc = 18; this.altPressed = down; break;

                    case OpenTK.Input.Key.PageUp: cc = 33; break;
                    case OpenTK.Input.Key.PageDown: cc = 34; break;
                    case OpenTK.Input.Key.Home: cc = 36; break;
                    case OpenTK.Input.Key.End: cc = 35; break;
                    case OpenTK.Input.Key.Delete: cc = 46; break;
                    case OpenTK.Input.Key.Insert: cc = 45; break;
                    case OpenTK.Input.Key.BackSpace: cc = 8; break;
                    case OpenTK.Input.Key.PrintScreen: cc = 44; break;
                    case OpenTK.Input.Key.Pause: cc = 19; break;
                    case OpenTK.Input.Key.WinLeft:
                    case OpenTK.Input.Key.WinRight: cc = 91; break;
                    case OpenTK.Input.Key.Menu: cc = 93; break;
                    case OpenTK.Input.Key.CapsLock: cc = 20; break;
                    case OpenTK.Input.Key.ScrollLock: cc = 145; break;
                    case OpenTK.Input.Key.NumLock: cc = 144; break;

                    default: return;
                }
            }
            events.Enqueue(KEY_EVENT);
            events.Enqueue(down ? 1 : 0);
            events.Enqueue(cc);
        }

        public List<PlatformRelayObject> GetEvents()
        {
            List<PlatformRelayObject> output = new List<PlatformRelayObject>();
            Queue<int> events = instance.events;
            int type;
            int x, y, keyCode;
            bool isDown, isLeft;

            while (events.Count > 0)
            {
                switch (events.Dequeue())
                {
                    case MOUSE_EVENT:
                        x = events.Dequeue();
                        y = events.Dequeue();
                        type = events.Dequeue();
                        if (type == MOUSE_MOVE)
                        {
                            output.Add(new PlatformRelayObject(32, x, y, 0, 0.0, null));
                        }
                        else
                        {
                            isLeft = type == MOUSE_LEFT;
                            isDown = events.Dequeue() == 1;
                            output.Add(new PlatformRelayObject(33 + (isLeft ? 0 : 2) + (isDown ? 0 : 1), x, y, 0, 0.0, null));
                        }
                        // TODO: mouse scroll
                        break;
                    case KEY_EVENT:
                        isDown = events.Dequeue() == 1;
                        keyCode = events.Dequeue();
                        output.Add(new PlatformRelayObject(isDown ? 16 : 17, keyCode, 0, 0, 0.0, null));
                        break;
                    case EXIT:
                        type = events.Dequeue();
                        output.Add(new PlatformRelayObject(1, type == EXIT_ALT_F4 ? 0 : 1, 0, 0, 0.0, null));
                        break;
                    default:
                        break;
                }
            }

            return output;
        }

        private void Startup()
        {
            GL.ClearColor(1f, 1f, 1f, 1f);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.ColorMaterial);
        }

        private void Resizing()
        {
            this.screenWidth = this.Width;
            this.screenHeight = this.Height;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, this.screenWidth, this.screenHeight, 0, 10000, -10000);
            GL.Viewport(0, 0, this.screenWidth, this.screenHeight);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            events.Enqueue(EXIT);
            events.Enqueue(EXIT_CLOSE_BUTTON);
            base.OnClosing(e);
        }

        public void SetTitle(string value)
        {
            this.Title = value;
        }

        private void Update()
        {
            int vmStatus = CrayonWrapper.v_runInterpreter(this.executionContextId);

            if (vmStatus == 1 || // Finished
                vmStatus == 3) // Error
            {
                // Because sometimes once isn't enough.
                this.Close();
                this.Exit();
                System.Environment.Exit(0);
            }
        }

        // defaults are valid and empty
        private static int[] renderEvents = new int[0];
        private static int renderEventsLength = 0;
        private static object[][] imagesNativeData = null;
        private static List<int> textChars = null;

        public static void SetRenderData(int[] events, int eventsLength, object[][] imagesNativeData, List<int> textChars)
        {
            GameWindow.renderEvents = events;
            GameWindow.renderEventsLength = eventsLength;
            GameWindow.imagesNativeData = imagesNativeData;
            GameWindow.textChars = textChars;
        }

        private void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            OpenTkRenderer.render(
                GameWindow.renderEvents,
                GameWindow.renderEventsLength,
                GameWindow.imagesNativeData,
                this.gameWidth, this.gameHeight, this.screenWidth, this.screenHeight);

            this.SwapBuffers();
        }

        internal int GetGamepadCount()
        {
            return this.Joysticks.Count;
        }

        internal object GetGamepadRawDevice(int index)
        {
            return this.Joysticks[index];
        }

        internal void GamepadEnableDevice(OpenTK.Input.JoystickDevice device)
        {
            // automatically enabled on OpenTK.
        }

        internal int GetGamepadAxisCount(OpenTK.Input.JoystickDevice device)
        {
            return device.Axis.Count;
        }

        internal int GetGamepadButtonCount(OpenTK.Input.JoystickDevice device)
        {
            return device.Button.Count;
        }

        internal int GetGamepadHatCount(OpenTK.Input.JoystickDevice device)
        {
            return 0;
        }

        internal string GetGamepadDeviceName(OpenTK.Input.JoystickDevice device)
        {
            return device.Description;
        }

        internal bool IsGamepadButtonPushed(OpenTK.Input.JoystickDevice device, int buttonIndex)
        {
            return device.Button[buttonIndex];
        }

        internal double GetGamepadAxisValue(OpenTK.Input.JoystickDevice device, int axisIndex)
        {
            return device.Axis[axisIndex];
        }
    }
}
