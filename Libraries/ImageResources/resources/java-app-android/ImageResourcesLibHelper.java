package org.crayonlang.libraries.imageresources;

import android.graphics.Bitmap;

import java.util.ArrayList;
import org.crayonlang.interpreter.structs.Value;

public final class ImageResourcesLibHelper {

    private ImageResourcesLibHelper() { }

    public static void imageResourceBlitImage(
            Object imageDstObj,
            Object imageSrcObj,
            int dstX, int dstY,
            int srcX, int srcY,
            int width, int height) {
        CrayonBitmap imageDst = (CrayonBitmap) imageDstObj;
        CrayonBitmap imageSrc = (CrayonBitmap) imageSrcObj;
        imageDst.blit(imageSrc, dstX, dstY, srcX, srcY, width, height);
    }

    public static void checkLoaderIsDone(Object[] imageLoaderNativeData, Object[] nativeImageDataNativeData, ArrayList<Value> output) {
        // TODO: this will have to have mutex locking when _imageLoadAsync is implemented for real.
        output.set(0, org.crayonlang.interpreter.Interpreter.v_buildInteger((int)imageLoaderNativeData[2]));
    }

    public static String getImageResourceManifestString() {
        return org.crayonlang.interpreter.AndroidTranslationHelper.getTextAsset("imagesheetmanifest.txt");
    }

    public static void imageLoadAsync(String imagePath, Object[] nativeImageDataNativeData, Object[] imageLoaderNativeData) {
        // TODO: make this asynchronous
        boolean loaded = imageLoadSync(imagePath, nativeImageDataNativeData, null);
        imageLoaderNativeData[2] = loaded ? 1 : 2;
    }

    public static boolean imageLoadSync(
            String imagePath,
            Object[] nativeImageDataNativeData,
            ArrayList<Value> statusOutCheesey) {
        Bitmap bitmap = org.crayonlang.interpreter.AndroidTranslationHelper.getImageAsset(imagePath);
        if (bitmap == null) {
            return false;
        }
        CrayonBitmap cb = new CrayonBitmap(bitmap);
        nativeImageDataNativeData[0] = cb;
        nativeImageDataNativeData[1] = cb.getWidth();
        nativeImageDataNativeData[2] = cb.getHeight();

        if (statusOutCheesey != null) {
            statusOutCheesey.set(0, statusOutCheesey.get(1));
        }

        return true;
    }

    public static CrayonBitmap generateNativeBitmapOfSize(int width, int height) {
        return new CrayonBitmap(width, height);
    }
}
