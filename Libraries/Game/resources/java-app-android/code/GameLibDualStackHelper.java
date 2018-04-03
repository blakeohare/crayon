package org.crayonlang.libraries.game;

import android.util.Log;
import android.view.MotionEvent;

import org.crayonlang.interpreter.AndroidTranslationHelper;
import org.crayonlang.interpreter.structs.PlatformRelayObject;

import java.util.ArrayList;

public class GameLibDualStackHelper {

    // Change this to use the OpenGL graphics stack.
    // Once the GL Stack is fully implemented, then the web view can safely be removed.
    public static boolean IS_OPEN_GL = false;

    private static ArrayList<PlatformRelayObject> events = new ArrayList<PlatformRelayObject>();

    public static double getCurrentTime() {
        return System.currentTimeMillis() / 1000.0;
    }

    public static GameLibGlView GL_VIEW = null;
    public static GameLibWebView WEB_VIEW = null;

    public static void initializeGame(double fps) {
        if (IS_OPEN_GL) {
            AndroidTranslationHelper.switchToView(new GameLibGlView(fps, AndroidTranslationHelper.getMainActivity()));
        } else {
            AndroidTranslationHelper.switchToView(new GameLibWebView(fps, AndroidTranslationHelper.getMainActivity()));
        }
    }

    public static void clockTick() {
        if (IS_OPEN_GL) {
            GL_VIEW.clockTick();
        } else {
            WEB_VIEW.clockTick();
        }
    }

    public static int[] eventList = new int[0];
    public static int eventsLength = 0;
    public static Object[][] imagesNativeData = null;
    public static ArrayList<Integer> textChars = null;

    public static void setRenderQueues(
            int[] eventList,
            int eventListLength,
            Object[][] imagesNativeData,
            ArrayList<Integer> textChars) {

        GameLibDualStackHelper.eventList = eventList;
        GameLibDualStackHelper.eventsLength = eventListLength;
        GameLibDualStackHelper.imagesNativeData = imagesNativeData;
        GameLibDualStackHelper.textChars = textChars;
    }

    public static void setTitle(String title) {
        if (IS_OPEN_GL) {
            // TODO: implement this.
        } else {
            // TODO: implement this.
        }
    }

    public static ArrayList<PlatformRelayObject> getEventsRawList() {
        ArrayList<PlatformRelayObject> output = new ArrayList<PlatformRelayObject>(events);
        events.clear();
        return output;
    }

    public static void initializeScreen(int width, int height, int screenWidth, int screenHeight, int executionContextId) {
        if (IS_OPEN_GL) {
            GL_VIEW.initializeScreen(width, height, screenWidth, screenHeight, executionContextId);
        } else {
            WEB_VIEW.setExecutionContextId(executionContextId);
            WEB_VIEW.initializeScreen(width, height);
        }
    }

    public static boolean getScreenInfo(int[] output) {
        int[] size = AndroidTranslationHelper.getSize();
        output[1] = 1;
        output[2] = size[0];
        output[3] = size[1];
        return true;
    }


    private static final MotionEvent.PointerCoords POINTER_COORDS_OUT = new MotionEvent.PointerCoords();

    public static boolean onTouchEvent(MotionEvent e, int logicalWidth, int logicalHeight, int screenWidth, int screenHeight) {
        switch (e.getAction()) {
            case MotionEvent.ACTION_MOVE:
                e.getPointerCoords(0, POINTER_COORDS_OUT);
                handleMouseEventImpl(true, false, POINTER_COORDS_OUT.x, POINTER_COORDS_OUT.y, logicalWidth, logicalHeight, screenWidth, screenHeight);
                break;
            case MotionEvent.ACTION_DOWN:
                e.getPointerCoords(0, POINTER_COORDS_OUT);
                handleMouseEventImpl(false, true, POINTER_COORDS_OUT.x, POINTER_COORDS_OUT.y, logicalWidth, logicalHeight, screenWidth, screenHeight);
                break;
            case MotionEvent.ACTION_UP:
                e.getPointerCoords(0, POINTER_COORDS_OUT);
                handleMouseEventImpl(false, false, POINTER_COORDS_OUT.x, POINTER_COORDS_OUT.y, logicalWidth, logicalHeight, screenWidth, screenHeight);
                break;
        }
        return true;
    }

    private static void handleMouseEventImpl(boolean isMove, boolean isDown, float x, float y, int logicalWidth, int logicalHeight, int screenWidth, int screenHeight) {
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
}
