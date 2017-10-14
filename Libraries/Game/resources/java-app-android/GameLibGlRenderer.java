package org.crayonlang.libraries.game;

import android.opengl.GLES20;
import android.opengl.GLSurfaceView;

import org.crayonlang.interpreter.Interpreter;
import org.crayonlang.interpreter.structs.InterpreterResult;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.FloatBuffer;

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

    private static FloatBuffer squareBuffer = null;

    private static final String vertexShaderCode =
        "uniform mat4 uMVPMatrix;" +
        "attribute vec4 vPosition;" +

        "void main() {" +
        "  gl_Position = uMVPMatrix * vPosition;" +
        //"  gl_Position = vPosition;" +

        "}";

    private static final String fragmentShaderCode =
        "precision mediump float;" +
        "uniform vec4 vColor;" +

        "void main() {" +
        "  gl_FragColor = vColor;" +
        "}";

    public static int loadShader(int type, String shaderCode){

        // create a vertex shader type (GLES20.GL_VERTEX_SHADER)
        // or a fragment shader type (GLES20.GL_FRAGMENT_SHADER)
        int shader = GLES20.glCreateShader(type);

        // add the source code to the shader and compile it
        GLES20.glShaderSource(shader, shaderCode);
        GLES20.glCompileShader(shader);

        return shader;
    }

    private static int rectVertexShaderId = -1;
    private static int rectFragmentShaderId = -1;
    private static int programId = -1;

    private static boolean initialized = false;

    private static final int COORDS_PER_VERTEX = 3;

    public void doRenderEventQueue(int[] events, int eventsLength, Object[][] imageNativeData) {

        if (!initialized) {
            ByteBuffer bb = ByteBuffer.allocateDirect(
                    4 /* vertex count */
                    * COORDS_PER_VERTEX
                    * 4 /* sizeof(float) */);
            bb.order(ByteOrder.nativeOrder());
            squareBuffer = bb.asFloatBuffer();
            squareBuffer.put(new float[] {
                    0f, 0f, 0f,
                    0f, 1f, 0f,
                    1f, 0f, 0f,
                    1f, 1f, 0f,
            });
            squareBuffer.position(0);

            rectVertexShaderId = loadShader(GLES20.GL_VERTEX_SHADER, vertexShaderCode);
            rectFragmentShaderId = loadShader(GLES20.GL_FRAGMENT_SHADER, fragmentShaderCode);
            programId = GLES20.glCreateProgram();
            GLES20.glAttachShader(programId, rectVertexShaderId);
            GLES20.glAttachShader(programId, rectFragmentShaderId);
            GLES20.glLinkProgram(programId);

            initialized = true;
        }

        int left;
        int top;
        int width;
        int height;
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

        float[] color = new float[4];

        // fast conversion from 0-255 to 0f-1f
        float[] conversion256 = new float[256];
        for (int i = 0; i < 256; ++i) {
            conversion256[i] = i / 255f;
        }

        float[] positioningMatrix = new float[16];
        for (int i = 0; i < 16; ++i) {
            positioningMatrix[i] = i % 5 == 0 ? 1 : 0;
        }

        // TODO: These are hardcoded for FireDodge. Get the actual values from the view.
        float VW = 800f;
        float VH = 600f;
        float vwHalf = VW / 2;
        float vhHalf = VH / 2;

        GLES20.glEnable(GLES20.GL_DEPTH_TEST);
        GLES20.glClear(GLES20.GL_COLOR_BUFFER_BIT | GLES20.GL_DEPTH_BUFFER_BIT);

        int mPositionHandle = GLES20.glGetAttribLocation(programId, "vPosition");
        int mColorHandle = GLES20.glGetUniformLocation(programId, "vColor");
        int mMVPMatrixHandle = GLES20.glGetUniformLocation(programId, "uMVPMatrix");

        for (int i = 0; i < eventsLength; i += 16) {
            switch (events[i]) {

                // Rectangle
                case 1:
                    left = events[i | 1];
                    top = events[i | 2];
                    width = events[i | 3];
                    height = events[i | 4];

                    positioningMatrix[0] = width / vwHalf;
                    positioningMatrix[5] = -height / vhHalf;
                    positioningMatrix[10] = 1;
                    positioningMatrix[15] = 1;
                    positioningMatrix[12] = (left - vwHalf) / vwHalf;;
                    positioningMatrix[13] = 1f - top / vhHalf; // map 0 to height ---> 1 to -1

                    color[0] = conversion256[events[i | 5]];
                    color[1] = conversion256[events[i | 6]];
                    color[2] = conversion256[events[i | 7]];
                    color[3] = conversion256[events[i | 8]];

                    GLES20.glUseProgram(programId);
                    GLES20.glUniformMatrix4fv(mMVPMatrixHandle, 1, false, positioningMatrix, 0);
                    GLES20.glUniform4fv(mColorHandle, 1, color, 0);
                    GLES20.glEnableVertexAttribArray(mPositionHandle);
                    GLES20.glVertexAttribPointer(
                        mPositionHandle,
                        COORDS_PER_VERTEX,
                        GLES20.GL_FLOAT,
                        false,
                        COORDS_PER_VERTEX * 4 /* stride */,
                        squareBuffer);
                    GLES20.glDrawArrays(GLES20.GL_TRIANGLE_STRIP, 0, 4 /* vertex count */);
                    GLES20.glDisableVertexAttribArray(mPositionHandle);
                    break;

                // Ellipse
                case 2:
                    left = events[i | 1];
                    top = events[i | 2];
                    width = events[i | 3];
                    height = events[i | 4];
                    color[0] = conversion256[events[i | 5]];
                    color[1] = conversion256[events[i | 6]];
                    color[2] = conversion256[events[i | 7]];
                    color[3] = conversion256[events[i | 8]];
                    // TODO: draw ellipse
                    break;

                // Line
                case 3:
                    startX = events[i | 1];
                    startY = events[i | 2];
                    endX = events[i | 3];
                    endY = events[i | 4];
                    lineWidth = events[i | 5];
                    color[0] = conversion256[events[i | 6]];
                    color[1] = conversion256[events[i | 7]];
                    color[2] = conversion256[events[i | 8]];
                    color[3] = conversion256[events[i | 9]];
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
                    color[0] = conversion256[events[i | 7]];
                    color[1] = conversion256[events[i | 8]];
                    color[2] = conversion256[events[i | 9]];
                    color[3] = conversion256[events[i | 10]];
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
                    color[0] = conversion256[events[i | 9]];
                    color[1] = conversion256[events[i | 10]];
                    color[2] = conversion256[events[i | 11]];
                    color[3] = conversion256[events[i | 12]];
                    // TODO: render quadrilateral
                    break;

                // Image
                case 6:
                    mask = events[i | 1];
                    rotated = (mask & 4) != 0;
                    color[3] = (mask & 8) != 0 ? events[i | 11] : 255;

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
