﻿public final class ResourceInfo {
  public String userPath;
  public String internalPath;
  public boolean isText;
  public String type;
  public String manifestParam;
  public static final ResourceInfo[] EMPTY_ARRAY = new ResourceInfo[0];

  public ResourceInfo(String userPath, String internalPath, boolean isText, String type, String manifestParam) {
    this.userPath = userPath;
    this.internalPath = internalPath;
    this.isText = isText;
    this.type = type;
    this.manifestParam = manifestParam;
  }
}