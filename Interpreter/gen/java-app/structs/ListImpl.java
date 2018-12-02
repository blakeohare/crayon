package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;;

public final class ListImpl {
  public int[] type;
  public int size;
  public int capacity;
  public Value[] array;
  public static final ListImpl[] EMPTY_ARRAY = new ListImpl[0];

  public ListImpl(int[] type, int size, int capacity, Value[] array) {
    this.type = type;
    this.size = size;
    this.capacity = capacity;
    this.array = array;
  }
}
