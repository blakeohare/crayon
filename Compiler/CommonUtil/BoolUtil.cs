namespace CommonUtil
{
    public static class BoolUtil
    {
        public static bool Parse(string value)
        {
            if (value == null) return false;

            switch (value.ToLower())
            {
                case "true":
                case "t":
                case "1":
                case "yes":
                case "y":
                    return true;
                default:
                    return false;
            }
        }

    }
}
