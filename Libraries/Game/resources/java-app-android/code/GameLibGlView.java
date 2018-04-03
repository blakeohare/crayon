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

    void updateScreenSize(int width, int height) {
        this.screenWidth = width;
        this.screenHeight = height;
    }

    @Override
    public boolean onTouchEvent(MotionEvent e) {
        return GameLibDualStackHelper.onTouchEvent(e, this.logicalWidth, this.logicalHeight, this.screenWidth, this.screenHeight);
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
