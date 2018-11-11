public final class PlatformRelayObject {
  public int type;
  public int iarg1;
  public int iarg2;
  public int iarg3;
  public double farg1;
  public String sarg1;
  public static final PlatformRelayObject[] EMPTY_ARRAY = new PlatformRelayObject[0];

  public PlatformRelayObject(int type, int iarg1, int iarg2, int iarg3, double farg1, String sarg1) {
    this.type = type;
    this.iarg1 = iarg1;
    this.iarg2 = iarg2;
    this.iarg3 = iarg3;
    this.farg1 = farg1;
    this.sarg1 = sarg1;
  }
}