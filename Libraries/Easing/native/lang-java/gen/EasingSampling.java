package org.crayonlang.libraries.easing;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class EasingSampling {
  public int sampleCount;
  public double[] samples;
  public static final EasingSampling[] EMPTY_ARRAY = new EasingSampling[0];

  public EasingSampling(int sampleCount, double[] samples) {
    this.sampleCount = sampleCount;
    this.samples = samples;
  }
}
