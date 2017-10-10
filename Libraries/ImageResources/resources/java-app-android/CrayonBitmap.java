package org.crayonlang.libraries.imageresources;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Rect;

public final class CrayonBitmap {
    private Bitmap internalBitmap;
    private int width;
    private int height;

    public CrayonBitmap(int width, int height) {
        this.width = width;
        this.height = height;
        this.internalBitmap = Bitmap.createBitmap(width, height, Bitmap.Config.ARGB_8888);
    }

    public CrayonBitmap(Bitmap bitmap) {
        this.internalBitmap = bitmap;
        this.width = bitmap.getWidth();
        this.height = bitmap.getHeight();
    }

    public CrayonBitmap(CrayonBitmap toClone) {
        this.internalBitmap = Bitmap.createBitmap(toClone.internalBitmap);
        this.width = toClone.width;
        this.height = toClone.height;
    }

    public int getWidth() {
        return this.width;
    }

    public int getHeight() {
        return this.height;
    }

    public void blit(CrayonBitmap bitmap, int dstX, int dstY, int srcX, int srcY, int width, int height) {
        Canvas canvas = new Canvas(this.internalBitmap);
        canvas.drawBitmap(
            bitmap.internalBitmap,
            new Rect(srcX, srcY, width, height),
            new Rect(dstX, dstY, width, height),
            null);
    }

    public CrayonBitmap createFlippedCopy(boolean horizontal, boolean vertical) {
        CrayonBitmap clone = new CrayonBitmap(this);
        Canvas canvas = new Canvas(clone.internalBitmap);
        canvas.scale(horizontal ? -1 : 1, vertical ? -1 : 1, this.width / 2f, this.height / 2f);
        return clone;
    }

    public CrayonBitmap createScaledCopy(int newWidth, int newHeight) {
        CrayonBitmap newBitmap = new CrayonBitmap(newWidth, newHeight);
        newBitmap.blit(this, 0, 0, 0, 0, newWidth, newHeight);
        return newBitmap;
    }
}
