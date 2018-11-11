public final class ResourceDB {
  public HashMap<String, String[]> filesPerDirectory;
  public HashMap<String, ResourceInfo> fileInfo;
  public ArrayList<Value> dataList;
  public static final ResourceDB[] EMPTY_ARRAY = new ResourceDB[0];

  public ResourceDB(HashMap<String, String[]> filesPerDirectory, HashMap<String, ResourceInfo> fileInfo, ArrayList<Value> dataList) {
    this.filesPerDirectory = filesPerDirectory;
    this.fileInfo = fileInfo;
    this.dataList = dataList;
  }
}