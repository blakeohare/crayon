package org.crayonlang.libraries.imageresources;

import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.RenderingHints;
import java.awt.image.BufferedImage;
import java.util.ArrayList;
import org.crayonlang.interpreter.ResourceReader;
import org.crayonlang.interpreter.structs.*;

public final class ImageResourcesHelper {

  private ImageResourcesHelper() {}

  public static void imageLoadAsync(String genFileName, Object[] nativeImageDataNativeData, Object[] imageLoaderNativeData) {
    // TODO: implement this for real and add mutex checks to checkLoaderIsDone
    boolean loaded = imageLoadSync(genFileName, nativeImageDataNativeData, null);
    imageLoaderNativeData[2] = loaded ? 1 : 2;
  }

  public static boolean imageLoadSync(String genFileName, Object[] nativeImageDataNativeData, ArrayList<Value> outStatusCheesy) {
    BufferedImage image = ResourceReader.loadImageFromLocalFile("images/" + genFileName);
    if (image != null) {
      if (outStatusCheesy != null) {
        java.util.Collections.reverse(outStatusCheesy);
      }
      nativeImageDataNativeData[0] = image;
      nativeImageDataNativeData[1] = image.getWidth();
      nativeImageDataNativeData[2] = image.getHeight();
      return true;
    }
    return false;
  }

  public static int checkLoaderIsDone(
    Object[] imageLoaderNativeData,
    Object[] nativeImageDataNativeData) {

    // TODO: add mutex checks when imageLoadAsync is implemented
    int status = (int)imageLoaderNativeData[2];
    // TODO: release mutex

    return status;
  }

  public static void blitImage(
    Object target, Object source,
    int targetX, int targetY,
    int sourceX, int sourceY,
    int width, int height) {

    Graphics g = ((BufferedImage) target).getGraphics();
    g.drawImage(
      (BufferedImage) source,
      targetX, targetY,
      targetX + width, targetY + height,
      sourceX, sourceY,
      sourceX + width, sourceY + height,
      null);
    g.dispose();
  }

  public static BufferedImage generateNativeBitmapOfSize(int width, int height) {
    return new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB);
  }

  public static String getImageResourceManifestString() {
    return ResourceReader.readFileText("resources/imagesheetmanifest.txt");
  }
}
