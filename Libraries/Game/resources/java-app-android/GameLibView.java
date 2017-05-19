package org.crayonlang.libraries.game;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.os.Handler;
import android.view.View;

import org.crayonlang.interpreter.AndroidTranslationHelper;
import org.crayonlang.interpreter.Interpreter;
import org.crayonlang.interpreter.structs.PlatformRelayObject;

import java.util.ArrayList;

public class GameLibView extends View {

    public static GameLibView INSTANCE = null;

    private double lastClockTimestamp = 0.0;
    private double fps = 60.0;
    private int executionContextId = -1;
    private int logicalWidth;
    private int logicalHeight;

    public GameLibView(double fps, Context context) {
        super(context);
        INSTANCE = this;
        lastClockTimestamp = getCurrentTime();
        this.fps = fps;
    }

    private Paint paint = new Paint();

    @Override
    public void onDraw(Canvas canvas) {
        // TODO: draw on a temporary canvas (which will likely be smaller) and then draw that canvas onto the final region.
        super.onDraw(canvas);
        paint.setColor(Color.BLUE);
        int screenWidth = canvas.getWidth();
        int screenHeight = canvas.getHeight();
        int logicalWidth = this.logicalWidth;
        int logicalHeight = this.logicalHeight;

        int left;
        int right;
        int top;
        int bottom;
        int alpha;
        int red;
        int green;
        int blue;

        for (int i = 0; i < this.eventLength; i += 16) {
            switch (this.eventList[i]) {
                case 1: // rectangle
                    left = eventList[i | 1];
                    top = eventList[i | 2];
                    right = left + eventList[i | 3];
                    bottom = top + eventList[i | 4];
                    red = eventList[i | 5];
                    green = eventList[i | 6];
                    blue = eventList[i | 7];
                    alpha = eventList[i | 8];

                    if (red > 255 || red < 0) red = red > 255 ? 255 : 0;
                    if (green > 255 || green < 0) green = green > 255 ? 255 : 0;
                    if (blue > 255 || blue < 0) blue = blue > 255 ? 255 : 0;
                    if (alpha > 255 || alpha < 0) alpha = alpha > 255 ? 255 : 0;
                    paint.setColor(Color.argb(alpha, red, green, blue));
                    canvas.drawRect(
                        left * screenWidth / logicalWidth,
                        top * screenHeight / logicalHeight,
                        right * screenWidth / logicalWidth,
                        bottom * screenHeight / logicalHeight,
                        paint);

                    break;
                default:
                    // TODO: this
                    break;
            }
        }

        invalidate();
    }

    public static void initializeGame(double fps) {
        AndroidTranslationHelper.switchToView(new GameLibView(fps, AndroidTranslationHelper.getMainActivity()));
    }

    public void initializeScreen(int logicalWidth, int logicalHeight, int screenWidthIgnored, int screenHeightIgnored, int executionContextId) {
        this.executionContextId = executionContextId;
        this.logicalWidth = logicalWidth;
        this.logicalHeight = logicalHeight;

        // most platforms open a blocking window and so most platforms assume this call
        // will re-invoke the interpreter.
        enqueueNextFrame();
    }

    public void setTitle(String title) {
        // Ignore this too.
    }

    public ArrayList<PlatformRelayObject> getEventsRawList() {
        // For now, just pretend there are no events...
        return new ArrayList<PlatformRelayObject>();
    }

    private double getCurrentTime() {
        return System.currentTimeMillis() / 1000.0;
    }

    public void clockTick() {
        enqueueNextFrame();
    }

    private void enqueueNextFrame() {
        double now = getCurrentTime();
        double diff = now - lastClockTimestamp;
        double spf = fps > 0 ? (1.0 / fps) : 1.0;
        double delay = spf - diff;
        if (delay <= 0.001) {
            delay = 0.001;
        }

        handler.postDelayed(nextFrameRunner, (int) (delay * 1000));
    }

    private final Handler handler = new Handler();
    private final Runnable nextFrameRunner = new Runnable() {
        public void run() {
            beginNextFrame();
        }
    };

    public void beginNextFrame() {
        lastClockTimestamp = getCurrentTime();
        Interpreter.v_runInterpreter(executionContextId);
    }

    private int[] eventList = new int[0];
    private int eventLength = 0;
    private Object[][] imagesNativeData = null;
    private ArrayList<Integer> textChars = null;

    public void setRenderQueues(
        int[] eventList,
        int eventListLength,
        Object[][] imagesNativeData,
        ArrayList<Integer> textChars) {

        this.eventList = eventList;
        this.eventLength = eventListLength;
        this.imagesNativeData = imagesNativeData;
        this.textChars = textChars;
    }
}
