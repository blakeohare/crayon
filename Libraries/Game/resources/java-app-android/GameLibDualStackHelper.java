package org.crayonlang.libraries.game;

import android.util.Log;

import org.crayonlang.interpreter.AndroidTranslationHelper;
import org.crayonlang.interpreter.structs.PlatformRelayObject;

import java.util.ArrayList;

public class GameLibDualStackHelper {

    // Change this to use the OpenGL graphics stack.
    // Once the GL Stack is fully implemented, then the web view can safely be removed.
    public static boolean IS_OPEN_GL = false;

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
            Log.d("TODO", "TODO: CLOCK TICK");
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
        if (IS_OPEN_GL) {
            return GL_VIEW.getEventsRawList();
        } else {
            Log.d("TODO", "TODO: GET EVENTS");
            return new ArrayList<>();
        }
    }

    public static void initializeScreen(int width, int height, int screenWidth, int screenHeight, int executionContextId) {
        if (IS_OPEN_GL) {
            GL_VIEW.initializeScreen(width, height, screenWidth, screenHeight, executionContextId);
        } else {
            Log.d("TODO", "TODO: INIT SCREEN");
        }
    }

    public static boolean getScreenInfo(int[] output) {
        int[] size = AndroidTranslationHelper.getSize();
        output[1] = 1;
        output[2] = size[0];
        output[3] = size[1];
        return true;
    }
}
