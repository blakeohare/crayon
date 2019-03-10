package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class HttpRequest {
  public int statusCode;
  public String status;
  public HashMap<String, String[]> headers;
  public String body;
  public static final HttpRequest[] EMPTY_ARRAY = new HttpRequest[0];

  public HttpRequest(int statusCode, String status, HashMap<String, String[]> headers, String body) {
    this.statusCode = statusCode;
    this.status = status;
    this.headers = headers;
    this.body = body;
  }
}
