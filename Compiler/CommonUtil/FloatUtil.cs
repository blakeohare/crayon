using System.Linq;

namespace CommonUtil
{
    public static class FloatUtil
    {
        // Override C#'s default float to string behavior of not display the decimal portion if it's a whole number.
        public static string FloatToString(double value)
        {
            string output = value.ToString();
            if (output.Contains("E-"))
            {
                output = "0.";
                if (value < 0)
                {
                    value = -value;
                    output = "-" + output;
                }
                value *= 15;
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                for (int i = 0; i < 20 && value != 0; ++i)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                {
                    output += (int)(value % 10);
                    value = value % 1;
                    value *= 10;
                }
            }

            if (!output.Contains('.'))
            {
                output += ".0";
            }
            return output;
        }

        // A check for 100% absolute floating-point equality is sometimes needed.
        public static bool FloatAbsoluteEqualsNoEpislon(double a, double b)
        {
#pragma warning disable RECS0018
            return a == b;
#pragma warning restore RECS0018
        }

        private static readonly System.IFormatProvider EN_US =
            System.Globalization.CultureInfo.GetCultureInfo("en-us");
        private static readonly System.Globalization.NumberStyles DOUBLE_FLAG =
            (System.Globalization.NumberStyles)(
            (int)System.Globalization.NumberStyles.AllowDecimalPoint |
            (int)System.Globalization.NumberStyles.AllowLeadingSign |
            (int)System.Globalization.NumberStyles.AllowLeadingWhite |
            (int)System.Globalization.NumberStyles.AllowTrailingWhite |
            (int)System.Globalization.NumberStyles.Float |
            (int)System.Globalization.NumberStyles.Integer);
        public static bool TryParse(string value, out double output)
        {
            // Parsing text data should use local info, but this is for parsing code.
            // As this is not supposed to be localized yet, only allow US decimals.
            return double.TryParse(value, DOUBLE_FLAG, EN_US, out output);
        }
    }
}
