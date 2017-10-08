package org.crayonlang.libraries.game;

import android.opengl.GLES20;
import android.opengl.GLSurfaceView;

import org.crayonlang.interpreter.Interpreter;

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
        GLES20.glClear(GLES20.GL_COLOR_BUFFER_BIT);
        Interpreter.v_interpret(this.executionContextId);
    }

    public void onSurfaceChanged(GL10 unused, int width, int height) {
        GLES20.glViewport(0, 0, width, height);
    }
}
