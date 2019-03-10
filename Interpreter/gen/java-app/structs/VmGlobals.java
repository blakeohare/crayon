package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class VmGlobals {
  public Value valueNull;
  public Value boolTrue;
  public Value boolFalse;
  public Value intZero;
  public Value intOne;
  public Value intNegativeOne;
  public Value floatZero;
  public Value floatOne;
  public Value stringEmpty;
  public Value[] positiveIntegers;
  public Value[] negativeIntegers;
  public HashMap<String, Value> commonStrings;
  public int[] booleanType;
  public int[] intType;
  public int[] stringType;
  public int[] floatType;
  public int[] classType;
  public int[] anyInstanceType;
  public static final VmGlobals[] EMPTY_ARRAY = new VmGlobals[0];

  public VmGlobals(Value valueNull, Value boolTrue, Value boolFalse, Value intZero, Value intOne, Value intNegativeOne, Value floatZero, Value floatOne, Value stringEmpty, Value[] positiveIntegers, Value[] negativeIntegers, HashMap<String, Value> commonStrings, int[] booleanType, int[] intType, int[] stringType, int[] floatType, int[] classType, int[] anyInstanceType) {
    this.valueNull = valueNull;
    this.boolTrue = boolTrue;
    this.boolFalse = boolFalse;
    this.intZero = intZero;
    this.intOne = intOne;
    this.intNegativeOne = intNegativeOne;
    this.floatZero = floatZero;
    this.floatOne = floatOne;
    this.stringEmpty = stringEmpty;
    this.positiveIntegers = positiveIntegers;
    this.negativeIntegers = negativeIntegers;
    this.commonStrings = commonStrings;
    this.booleanType = booleanType;
    this.intType = intType;
    this.stringType = stringType;
    this.floatType = floatType;
    this.classType = classType;
    this.anyInstanceType = anyInstanceType;
  }
}
