package org.crayonlang.libraries.graphics2d;

import org.crayonlang.libraries.imageresources.CrayonBitmap;

public final class Graphics2DLibHelper {

    private Graphics2DLibHelper() { }

    public static Object flipImage(Object nativeImage, boolean flipHorizontal, boolean flipVertical) {
        return ((CrayonBitmap) nativeImage).createFlippedCopy(flipHorizontal, flipVertical);
    }

    public static Object scaleImage(Object nativeImage, int newWidth, int newHeight) {
        return ((CrayonBitmap) nativeImage).createScaledCopy(newWidth, newHeight);
    }
}
