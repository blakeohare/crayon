using System;

namespace Common
{
    public class OrientationParser
    {
        public bool SupportsPortrait { get; private set; }
        public bool SupportsUpsideDown { get; private set; }
        public bool SupportsLandscapeLeft { get; private set; }
        public bool SupportsLandscapeRight { get; private set; }

        public OrientationParser(Options options)
        {
            string rawValue = options.GetStringOrEmpty(ExportOptionKey.SUPPORTED_ORIENTATION).Trim();
            bool down = false;
            bool up = false;
            bool left = false;
            bool right = false;
            if (rawValue.Length == 0)
            {
                down = true;
            }
            else
            {
                foreach (string orientation in rawValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    switch (orientation.Trim())
                    {
                        case "portrait":
                            down = true;
                            break;
                        case "upsidedown":
                            up = true;
                            break;
                        case "landscape":
                            left = true;
                            right = true;
                            break;
                        case "landscapeleft":
                            left = true;
                            break;
                        case "landscaperight":
                            right = true;
                            break;
                        case "all":
                            down = true;
                            up = true;
                            left = true;
                            right = true;
                            break;
                        default:
                            throw new InvalidOperationException("Unrecognized screen orientation: '" + orientation + "'");
                    }
                }
            }

            this.SupportsPortrait = down;
            this.SupportsUpsideDown = up;
            this.SupportsLandscapeLeft = left;
            this.SupportsLandscapeRight = right;
        }
    }
}
