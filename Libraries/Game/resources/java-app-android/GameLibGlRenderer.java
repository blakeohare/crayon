package org.crayonlang.libraries.game;

import android.opengl.GLES20;
import android.opengl.GLSurfaceView;

import org.crayonlang.interpreter.Interpreter;
import org.crayonlang.interpreter.structs.InterpreterResult;

import javax.microedition.khronos.egl.EGLConfig;
import javax.microedition.khronos.opengles.GL10;

public class GameLibGlRenderer implements GLSurfaceView.Renderer {

    private int executionContextId;

    public void setExecutionContextId(int executionContextId) {
        this.executionContextId = executionContextId;
    }

    public void onSurfaceCreated(GL10 unused, EGLConfig config) {
        GLES20.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
    }

    public void onDrawFrame(GL10 unused) {
        InterpreterResult ir = Interpreter.v_interpret(this.executionContextId);
        switch (ir.status) {
            case 3:
                throw new RuntimeException(ir.errorMessage);
        }
        GLES20.glClear(GLES20.GL_COLOR_BUFFER_BIT);
        GameLibView.INSTANCE.render();
    }

    public void onSurfaceChanged(GL10 unused, int width, int height) {
        GLES20.glViewport(0, 0, width, height);
    }

    public void doRenderEventQueue(int[] events, int eventsLength, Object[][] imageNativeData) {
        int left;
        int top;
        int right;
        int bottom;
        int startX;
        int startY;
        int endX;
        int endY;
        int ax;
        int ay;
        int bx;
        int by;
        int cx;
        int cy;
        int dx;
        int dy;

        int mask;
        int lineWidth;
        boolean rotated;

        int red;
        int green;
        int blue;
        int alpha;

        for (int i = 0; i < eventsLength; i += 16) {
            switch (events[i]) {

                // Rectangle
                case 1:
                    left = events[i | 1];
                    top = events[i | 2];
                    right = events[i | 3] + left;
                    bottom = events[i | 4] + top;
                    red = events[i | 5];
                    green = events[i | 6];
                    blue = events[i | 7];
                    alpha = events[i | 8];
                    // TODO: draw rectangle
                    break;

                // Ellipse
                case 2:
                    left = events[i | 1];
                    top = events[i | 2];
                    right = events[i | 3] + left;
                    bottom = events[i | 4] + top;
                    red = events[i | 5];
                    green = events[i | 6];
                    blue = events[i | 7];
                    alpha = events[i | 8];
                    // TODO: draw ellipse
                    break;

                // Line
                case 3:
                    startX = events[i | 1];
                    startY = events[i | 2];
                    endX = events[i | 3];
                    endY = events[i | 4];
                    lineWidth = events[i | 5];
                    red = events[i | 6];
                    green = events[i | 7];
                    blue = events[i | 8];
                    alpha = events[i | 9];
                    // TODO: render line
                    break;

                // Triangle
                case 4:
                    ax = events[i | 1];
                    ay = events[i | 2];
                    bx = events[i | 3];
                    by = events[i | 4];
                    cx = events[i | 5];
                    cy = events[i | 6];
                    red = events[i | 7];
                    green = events[i | 8];
                    blue = events[i | 9];
                    alpha = events[i | 10];
                    // TODO: render triangle
                    break;

                // Quadrilateral
                case 5:
                    ax = events[i | 1];
                    ay = events[i | 2];
                    bx = events[i | 3];
                    by = events[i | 4];
                    cx = events[i | 5];
                    cy = events[i | 6];
                    dx = events[i | 7];
                    dy = events[i | 8];
                    red = events[i | 9];
                    green = events[i | 10];
                    blue = events[i | 11];
                    alpha = events[i | 12];
                    // TODO: render quadrilateral
                    break;

                // Image
                case 6:
                    mask = events[i | 1];
                    rotated = (mask & 4) != 0;
                    alpha = (mask & 8) != 0 ? events[i | 11] : 255;

                    // TODO: render image
                    break;

                // Tinted Image
                case 8:
                    break;

                default:
                    throw new RuntimeException("Render event ID " + events[i] + " not implemented.");
            }
        }
    }
}
