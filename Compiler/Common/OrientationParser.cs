using CommonUtil;
using System;

namespace Common
{
    public class OrientationParserOld
    {
        public bool SupportsPortrait { get; private set; }
        public bool SupportsUpsideDown { get; private set; }
        public bool SupportsLandscapeLeft { get; private set; }
        public bool SupportsLandscapeRight { get; private set; }

        public OrientationParserOld(Wax.ExportProperties exportProperties)
        {
            string rawValue = "";
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
                foreach (string orientation in StringUtil.SplitRemoveEmpty(rawValue, ","))
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
