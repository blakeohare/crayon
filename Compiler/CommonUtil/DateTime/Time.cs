namespace CommonUtil.DateTime
{
    public static class Time
    {
        private static readonly System.DateTime EPOCH = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        private static System.TimeSpan UnixEpochAsTimespan
        {
            get { return System.DateTime.UtcNow.Subtract(EPOCH); }
        }

        public static int UnixTimeNow { get { return (int)UnixEpochAsTimespan.TotalSeconds; } }
        public static int UnixTimeNowMillis { get { return (int)UnixEpochAsTimespan.TotalMilliseconds; } }
        public static double UnixTimeFloat { get { return UnixEpochAsTimespan.TotalMilliseconds / 1000.0; } }

        public static int GetCurrentYear()
        {
            return System.DateTime.Now.Year;
        }
    }
}
