using Common;

namespace Build.BuildParseNodes
{
    public class Size
    {
        public NullableInteger Width { get; set; }

        public NullableInteger Height { get; set; }

        public static Size Merge(Size primary, Size secondary)
        {
            if (primary == null) return secondary;
            if (secondary == null) return primary;
            return new Size()
            {
                Width = primary.Width ?? secondary.Width,
                Height = primary.Height ?? secondary.Height,
            };
        }
    }
}
