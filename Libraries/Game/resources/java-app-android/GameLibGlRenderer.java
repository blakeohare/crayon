package org.crayonlang.libraries.game;

import android.opengl.GLES20;
import android.opengl.GLSurfaceView;
import android.opengl.GLUtils;

import org.crayonlang.interpreter.Interpreter;
import org.crayonlang.interpreter.structs.InterpreterResult;
import org.crayonlang.libraries.imageresources.CrayonBitmap;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.FloatBuffer;

import javax.microedition.khronos.egl.EGLConfig;
import javax.microedition.khronos.opengles.GL10;

import static android.R.attr.bitmap;

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
        GameLibView.INSTANCE.updateScreenSize(width, height);
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

    private static float[] conversion256;

    private int loadTexture(CrayonBitmap cbmp) {

        int[] integerPointer = new int[1];
        GLES20.glGenTextures(1, integerPointer, 0);
        int textureId = integerPointer[0];
        if (textureId == 0) throw new RuntimeException();
        GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, textureId);
        GLES20.glTexParameteri(GLES20.GL_TEXTURE_2D, GLES20.GL_TEXTURE_MIN_FILTER, GLES20.GL_NEAREST);
        GLES20.glTexParameteri(GLES20.GL_TEXTURE_2D, GLES20.GL_TEXTURE_MAG_FILTER, GLES20.GL_NEAREST);
        GLUtils.texImage2D(GLES20.GL_TEXTURE_2D, 0, cbmp.getNativeBitmap(), 0);
        GLES20.glDisable(GLES20.GL_TEXTURE_2D);
        return textureId;
    }

    public void doRenderEventQueue(int[] events, int eventsLength, Object[][] imageNativeData, int logicalWidth, int logicalHeight) {

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

            // fast conversion from 0-255 to 0f-1f
            conversion256 = new float[256];
            for (int i = 0; i < 256; ++i) {
                conversion256[i] = i / 255f;
            }

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

        int cropWidth;
        int cropHeight;
        float croppedLeft;
        float croppedRight;
        float croppedTop;
        float croppedBottom;

        float textureLeft;
        float textureRight;
        float textureTop;
        float textureBottom;
        float textureWidth;
        float textureHeight;
        int textureResourceWidth;
        int textureResourceHeight;

        int mask;
        int lineWidth;
        boolean rotated;
        int textureId;

        float[] color = new float[4];
        Object[] textureNativeData;
        Object[] textureResourceNativeData;

        float[] positioningMatrix = new float[16];
        for (int i = 0; i < 16; ++i) {
            positioningMatrix[i] = i % 5 == 0 ? 1 : 0;
        }

        // TODO: These are hardcoded for FireDodge. Get the actual values from the view.
        float VW = logicalWidth;
        float VH = logicalHeight;
        float vwHalf = VW / 2;
        float vhHalf = VH / 2;

        GLES20.glClear(GLES20.GL_COLOR_BUFFER_BIT | GLES20.GL_DEPTH_BUFFER_BIT);

        int mPositionHandle = GLES20.glGetAttribLocation(programId, "vPosition");
        int mColorHandle = GLES20.glGetUniformLocation(programId, "vColor");
        int mMVPMatrixHandle = GLES20.glGetUniformLocation(programId, "uMVPMatrix");

        int imageIndex = 0;

        float[] conversion256 = this.conversion256;

        int activeTextureId = -1;

        for (int i = 0; i < eventsLength; i += 16) {
            switch (events[i]) {

                // Rectangle
                case 1:
                    if (activeTextureId != -1) {
                        activeTextureId = -1;
                        GLES20.glDisable(GLES20.GL_TEXTURE_2D);
                    }

                    left = events[i | 1];
                    top = events[i | 2];
                    width = events[i | 3];
                    height = events[i | 4];

                    positioningMatrix[0] = width / vwHalf;
                    positioningMatrix[5] = -height / vhHalf;
                    positioningMatrix[10] = 1;
                    positioningMatrix[15] = 1;
                    positioningMatrix[12] = (left - vwHalf) / vwHalf;
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
                    textureNativeData = imageNativeData[imageIndex++];
                    textureResourceNativeData = (Object[]) textureNativeData[0];

                    if (!(boolean) textureResourceNativeData[1]) {
                        textureResourceNativeData[2] = loadTexture((CrayonBitmap) textureResourceNativeData[3]);
                        textureResourceNativeData[1] = true;
                    }

                    mask = events[i | 1];
                    rotated = (mask & 4) != 0;
                    color[3] = (mask & 8) != 0 ? conversion256[events[i | 11]] : 1f;
                    textureId = (int)textureResourceNativeData[2];

                    if (activeTextureId != textureId) {
                        activeTextureId = textureId;
                        GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, textureId);
                    }

                    startX = events[i | 8]; // left
                    startY = events[i | 9]; // top

                    textureLeft = (float)(double)textureNativeData[1];
                    textureTop = (float)(double)textureNativeData[2];
                    textureRight = (float)(double)textureNativeData[3];
                    textureBottom = (float)(double)textureNativeData[4];
                    width = (int)textureNativeData[5];
                    height = (int)textureNativeData[6];

                    textureWidth = textureRight - textureLeft;
                    textureHeight = textureBottom - textureTop;
                    textureResourceWidth = (int)textureResourceNativeData[4];
                    textureResourceHeight = (int)textureResourceNativeData[5];

                    // slice
                    if ((mask & 1) != 0)
                    {
                        ax = events[i | 2];
                        ay = events[i | 3];
                        cropWidth = events[i | 4];
                        cropHeight = events[i | 5];

                        croppedLeft = textureLeft + textureWidth * ax / width;
                        croppedRight = textureLeft + textureWidth * (ax + cropWidth) / width;
                        croppedTop = textureTop + textureHeight * ay / height;
                        croppedBottom = textureTop + textureHeight * (ay + cropHeight) / height;

                        textureLeft = croppedLeft;
                        textureRight = croppedRight;
                        textureTop = croppedTop;
                        textureBottom = croppedBottom;
                        width = cropWidth;
                        height = cropHeight;
                    }

                    // stretch
                    if ((mask & 2) != 0)
                    {
                        width = events[i | 6];
                        height = events[i | 7];
                    }

                    if (rotated) {
                        throw new RuntimeException();
                    } else {
                        positioningMatrix[0] = width / vwHalf;
                        positioningMatrix[5] = -height / vhHalf;
                        positioningMatrix[10] = 1;
                        positioningMatrix[15] = 1;
                        positioningMatrix[12] = (startX - vwHalf) / vwHalf;
                        positioningMatrix[13] = 1f - startY / vhHalf; // map 0 to height ---> 1 to -1

                        // white for now.
                        color[0] = 1;
                        color[1] = 1;
                        color[2] = 1;

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

                    }

                    break;

                // Tinted Image
                case 8:
                    break;

                default:
                    throw new RuntimeException("Render event ID " + events[i] + " not implemented.");
            }
        }

        GLES20.glDisable(GLES20.GL_TEXTURE_2D);
    }
}
