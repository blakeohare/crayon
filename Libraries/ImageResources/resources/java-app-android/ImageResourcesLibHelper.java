package org.crayonlang.libraries.imageresources;

import java.util.ArrayList;
import org.crayonlang.interpreter.structs.Value;

public final class ImageResourcesLibHelper {

    private ImageResourcesLibHelper() { }

    public static void imageResourceBlitImage(Object imageDst, Object imageSrc, int a, int b, int c, int d, int e, int f) {
        throw new RuntimeException();
    }

    public static void checkLoaderIsDone(Object obj1, Object obj2, ArrayList<Value> valueList) {
        throw new RuntimeException();
    }

    public static String getImageResourceManifestString() {
        return org.crayonlang.interpreter.AndroidTranslationHelper.getTextAsset("imagesheetmanifest.txt");
    }

    public static void imageLoadAsync(String str1, Object[] objArray1, Object[] objArray2) {
        throw new RuntimeException();
    }

    public static void imageLoadSync(String str1, Object[] objArray1, ArrayList<Value> list) {
        throw new RuntimeException();
    }

    public static Object generateNativeBitmapOfSize(int width, int height) {
        throw new RuntimeException();
    }
}
