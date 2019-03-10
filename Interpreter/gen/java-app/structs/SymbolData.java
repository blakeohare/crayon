package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class SymbolData {
  public ArrayList<Token>[] tokenData;
  public String[] sourceCode;
  public ArrayList<String> sourceCodeBuilder;
  public String[] fileNameById;
  public HashMap<String, Integer> fileIdByName;
  public HashMap<Integer, ArrayList<String>> localVarNamesById;
  public HashMap<Integer, ArrayList<String>> closureVarNamesById;
  public static final SymbolData[] EMPTY_ARRAY = new SymbolData[0];

  public SymbolData(ArrayList<Token>[] tokenData, String[] sourceCode, ArrayList<String> sourceCodeBuilder, String[] fileNameById, HashMap<String, Integer> fileIdByName, HashMap<Integer, ArrayList<String>> localVarNamesById, HashMap<Integer, ArrayList<String>> closureVarNamesById) {
    this.tokenData = tokenData;
    this.sourceCode = sourceCode;
    this.sourceCodeBuilder = sourceCodeBuilder;
    this.fileNameById = fileNameById;
    this.fileIdByName = fileIdByName;
    this.localVarNamesById = localVarNamesById;
    this.closureVarNamesById = closureVarNamesById;
  }
}
