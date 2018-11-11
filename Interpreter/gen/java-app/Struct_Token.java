public final class Token {
  public int lineIndex;
  public int colIndex;
  public int fileId;
  public static final Token[] EMPTY_ARRAY = new Token[0];

  public Token(int lineIndex, int colIndex, int fileId) {
    this.lineIndex = lineIndex;
    this.colIndex = colIndex;
    this.fileId = fileId;
  }
}