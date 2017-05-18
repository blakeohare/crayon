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

    public GameLibView(double fps, Context context) {
        super(context);
        INSTANCE = this;
        lastClockTimestamp = getCurrentTime();
        this.fps = fps;
    }

    private Paint paint = new Paint();

    @Override
    public void onDraw(Canvas canvas) {
        super.onDraw(canvas);
        paint.setColor(Color.BLUE);
        int width = canvas.getWidth();
        int height = canvas.getHeight();
        canvas.drawRect(width / 4, height / 4, width / 2, height / 2, paint);
        invalidate();
    }

    public static void initializeGame(double fps) {
        AndroidTranslationHelper.switchToView(new GameLibView(fps, AndroidTranslationHelper.getMainActivity()));
    }

    public void initializeScreen(int logicalWidth, int logicalHeight, int screenWidthIgnored, int screenHeightIgnored, int executionContextId) {
        this.executionContextId = executionContextId;
        // size is ignored for now.


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
}
