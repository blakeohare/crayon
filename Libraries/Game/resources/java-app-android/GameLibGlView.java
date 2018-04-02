package org.crayonlang.libraries.game;

import android.content.Context;
import android.opengl.GLSurfaceView;
import android.view.MotionEvent;

import org.crayonlang.interpreter.Interpreter;
import org.crayonlang.interpreter.structs.PlatformRelayObject;

import java.util.ArrayList;

public class GameLibGlView extends GLSurfaceView {

    private double lastClockTimestamp = 0.0;
    private double fps = 60.0;
    private int executionContextId = -1;
    private int logicalWidth;
    private int logicalHeight;
    private int screenWidth = -1;
    private int screenHeight = -1;
    private ArrayList<PlatformRelayObject> events = new ArrayList<PlatformRelayObject>();
    private final GameLibGlRenderer renderer;

    public GameLibGlView(double fps, Context context) {
        super(context);
        lastClockTimestamp = GameLibDualStackHelper.getCurrentTime();
        this.fps = fps;
        setEGLContextClientVersion(2);
        this.renderer = new GameLibGlRenderer(this);
        this.setRenderer(this.renderer);
        GameLibDualStackHelper.GL_VIEW = this;
    }

    private void handleMouseEventImpl(boolean isMove, boolean isDown, float x, float y) {
        int px = 0;
        int py = 0;
        if (screenWidth != -1) {
            px = (int) (x * logicalWidth / screenWidth);
            py = (int) (y * logicalHeight / screenHeight);
        }

        PlatformRelayObject pro;
        if (isMove) {
            pro = new PlatformRelayObject(32, px, py, 0, 0, "");
        } else {
            if (isDown) {
                pro = new PlatformRelayObject(33, px, py, 0, 0, "");
            } else {
                pro = new PlatformRelayObject(34, px, py, 0, 0, "");
            }
        }
        events.add(pro);
    }

    private static final MotionEvent.PointerCoords POINTER_COORDS_OUT = new MotionEvent.PointerCoords();

    void updateScreenSize(int width, int height) {
        this.screenWidth = width;
        this.screenHeight = height;
    }

    @Override
    public boolean onTouchEvent(MotionEvent e) {
        switch (e.getAction()) {
            case MotionEvent.ACTION_MOVE:
                e.getPointerCoords(0, POINTER_COORDS_OUT);
                handleMouseEventImpl(true, false, POINTER_COORDS_OUT.x, POINTER_COORDS_OUT.y);
                break;
            case MotionEvent.ACTION_DOWN:
                e.getPointerCoords(0, POINTER_COORDS_OUT);
                handleMouseEventImpl(false, true, POINTER_COORDS_OUT.x, POINTER_COORDS_OUT.y);
                break;
            case MotionEvent.ACTION_UP:
                e.getPointerCoords(0, POINTER_COORDS_OUT);
                handleMouseEventImpl(false, false, POINTER_COORDS_OUT.x, POINTER_COORDS_OUT.y);
                break;
        }
        return true;
    }

    public void initializeScreen(int logicalWidth, int logicalHeight, int screenWidthIgnored, int screenHeightIgnored, int executionContextId) {
        this.executionContextId = executionContextId;
        this.logicalWidth = logicalWidth;
        this.logicalHeight = logicalHeight;

        this.renderer.setExecutionContextId(executionContextId);
    }

    public void setTitle(String title) {
        // Ignore this too.
    }

    public ArrayList<PlatformRelayObject> getEventsRawList() {
        ArrayList<PlatformRelayObject> output = new ArrayList<PlatformRelayObject>(events);
        events.clear();
        return output;
    }

    public void clockTick() {
        //enqueueNextFrame();
    }

    public void beginNextFrame() {
        lastClockTimestamp = GameLibDualStackHelper.getCurrentTime();
        Interpreter.v_runInterpreter(executionContextId);
    }

    public void render() {
        this.renderer.doRenderEventQueue(
            GameLibDualStackHelper.eventList,
            GameLibDualStackHelper.eventsLength,
            GameLibDualStackHelper.imagesNativeData,
            this.logicalWidth, this.logicalHeight);
    }
}
