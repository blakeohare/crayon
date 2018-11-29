package org.crayonlang.libraries.game;

import java.awt.image.BufferedImage;
import java.awt.Graphics2D;
import java.awt.RenderingHints;

public final class Graphics2DHelper {

  private Graphics2DHelper() {}

  public static BufferedImage flipImage(Object original, boolean flipX, boolean flipY) {
    BufferedImage originalImage = (BufferedImage) original;
    if (!flipX && !flipY) return originalImage;
    int x = 0;
    int y = 0;
    int width = originalImage.getWidth();
    int height = originalImage.getHeight();
    BufferedImage flippedImage = new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB);
    Graphics2D g = flippedImage.createGraphics();
    if (flipX) {
      x = width;
      width = -width;
    }
    if (flipY) {
      y = height;
      height = -height;
    }
    g.drawImage(originalImage, x, y, width, height, null);
    g.dispose();
    return flippedImage;
  }

  public static BufferedImage scaleImage(Object imageObj, int width, int height) {
    BufferedImage image = (BufferedImage) imageObj;
    BufferedImage newImage = new BufferedImage(width, height, image.getType());
    Graphics2D g = newImage.createGraphics();
    g.setRenderingHint(RenderingHints.KEY_INTERPOLATION, RenderingHints.VALUE_INTERPOLATION_NEAREST_NEIGHBOR);
    g.drawImage(image, 0, 0, width, height, 0, 0, image.getWidth(), image.getHeight(), null);
    g.dispose();
    return newImage;
  }
}
