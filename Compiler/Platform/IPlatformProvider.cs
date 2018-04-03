namespace Platform
{
    public interface IPlatformProvider
    {
        AbstractPlatform GetPlatform(string name);
    }
}
