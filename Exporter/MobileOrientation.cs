namespace Crayon
{
    public enum MobileOrientation
    {
        AUTO,
        PORTRAIT,
        PORTRAITS,
        LANDSCAPE,
        LANDSCAPES,
    }

    public static class MobileOrientationUtil
    {
        public static MobileOrientation Parse(string value)
        {
            value = (value ?? "").Trim().ToLower();
            switch (value)
            {
                case "portrait": return MobileOrientation.PORTRAIT;
                case "portraits": return MobileOrientation.PORTRAITS;
                case "landscape": return MobileOrientation.LANDSCAPE;
                case "landscapes": return MobileOrientation.LANDSCAPES;
                case "auto": return MobileOrientation.AUTO;
                case "": return MobileOrientation.AUTO;
                default:
                    throw new System.InvalidOperationException("Unknown mobile orientation value");
            }
        }
    }
}
