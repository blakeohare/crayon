package org.crayonlang.libraries.game;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.view.View;

import org.crayonlang.crayonsampleapp.app.MainActivity;
import org.crayonlang.interpreter.AndroidTranslationHelper;
import org.crayonlang.interpreter.structs.PlatformRelayObject;

import java.util.ArrayList;

public class GameLibView extends View {

    public static GameLibView INSTANCE = null;

    public GameLibView(Context context) {
        super(context);
        INSTANCE = this;
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
        // fps is ignored for now.
        AndroidTranslationHelper.switchToView(new GameLibView(AndroidTranslationHelper.getMainActivity()));
    }

    public void initializeScreen(int logicalWidth, int logicalHeight, int screenWidthIgnored, int screenHeightIgnored) {
        // These are ignored, too, for now.
    }

    public void setTitle(String title) {
        // Yup, this too.
    }

    public ArrayList<PlatformRelayObject> getEventsRawList() {
        // For now, just pretend there are no events...
        return new ArrayList<PlatformRelayObject>();
    }
}
