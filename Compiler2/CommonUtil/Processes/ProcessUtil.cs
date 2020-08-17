namespace CommonUtil.Process
{
    public static class ProcessUtil
    {
        public static int GetCurrentProcessId()
        {
            return System.Diagnostics.Process.GetCurrentProcess().Id;
        }
    }
}
