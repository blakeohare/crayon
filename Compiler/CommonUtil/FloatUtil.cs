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
    }
}
