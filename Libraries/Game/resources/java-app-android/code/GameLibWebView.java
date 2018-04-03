package org.crayonlang.libraries.game;

import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.net.Uri;
import android.opengl.GLES20;
import android.util.Base64;
import android.util.Log;
import android.view.MotionEvent;
import android.webkit.ConsoleMessage;
import android.webkit.JavascriptInterface;
import android.webkit.WebChromeClient;
import android.webkit.WebView;
import android.webkit.WebViewClient;

import org.crayonlang.interpreter.AndroidTranslationHelper;
import org.crayonlang.interpreter.Interpreter;
import org.crayonlang.interpreter.structs.InterpreterResult;
import org.crayonlang.libraries.imageresources.CrayonBitmap;

import java.io.ByteArrayOutputStream;
import java.util.ArrayList;
import java.util.HashMap;

import javax.microedition.khronos.opengles.GL10;

public class GameLibWebView extends WebView {

    private int executionContextId;
    private boolean readyToReceiveMessages = false;

    // Use positive values so that if events happen to sneak through before initialization is
    // complete, they won't cause division by 0 errors.
    private int logicalWidth = 100;
    private int logicalHeight = 100;

    public GameLibWebView(double fps, Context context) {
        super(context);

        GameLibDualStackHelper.WEB_VIEW = this;

        this.getSettings().setJavaScriptEnabled(true);

        this.setWebViewClient(new GameLibWebViewClient());
        this.setWebChromeClient(new WebChromeClient() {
            @Override
            public boolean onConsoleMessage(ConsoleMessage consoleMessage) {
                String filename = consoleMessage.sourceId();
                Log.d("JS", "<" + filename + ":" + consoleMessage.lineNumber() + "> " + consoleMessage.message());
                return super.onConsoleMessage(consoleMessage);
            }
        });
        this.addJavascriptInterface(new JavaScriptBridge(this), "JavaScriptBridge");
        this.loadUrl("file:///android_asset/webview_index.html");
    }

    public static void initializeGame(double fps) {
        AndroidTranslationHelper.switchToView(new GameLibWebView(fps, AndroidTranslationHelper.getMainActivity()));
    }

    @Override
    public boolean onTouchEvent(MotionEvent e) {
        return GameLibDualStackHelper.onTouchEvent(e, this.logicalWidth, this.logicalHeight, this.getWidth(), this.getHeight());
    }

    public void setExecutionContextId(int executionContextId) {
        this.executionContextId = executionContextId;
    }

    public void initializeScreen(int gameWidth, int gameHeight) {
        this.logicalWidth = gameWidth;
        this.logicalHeight = gameHeight;
        this.sendMessage("screen-size", gameWidth + "," + gameHeight, false);
    }

    private static int imageId = 1;

    private int loadTexture(Long jsImageLookupKey, CrayonBitmap cbmp, int x, int y, int width, int height) {
        int jsImageId = imageId++;
        ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
        CrayonBitmap subImage = new CrayonBitmap(width, height);
        subImage.blit(cbmp, 0, 0, -x, -y, width, height);
        subImage.getNativeBitmap().compress(Bitmap.CompressFormat.PNG, 100, byteArrayOutputStream);
        byte[] byteArray = byteArrayOutputStream.toByteArray();
        String base64Data = Base64.encodeToString(byteArray, Base64.DEFAULT);
        this.sendMessage("load-image", jsImageId + "," + base64Data, true);
        jsImageIds.put(jsImageLookupKey, jsImageId);
        return jsImageId;
    }

    // This maps JS image IDs from a lookup key
    // The lookup key is the ID of the crayon bitmap (incrementally assigned and stashed into the GL texture ID of the sheet), the x, and the y coordinate.
    // key = ((texture ID * 1024) + x) * 1024 + y
    private HashMap<Long, Integer> jsImageIds = new HashMap<>();
    private int textureId = 0;

    public void clockTick() {
        int[] events = GameLibDualStackHelper.eventList;
        int length = GameLibDualStackHelper.eventsLength;
        Object[][] imageNativeData = GameLibDualStackHelper.imagesNativeData;
        Object[] textureResourceNativeData;
        int imageIndex = 0;
        String msg = "1 0 0 100 100 0 0 0 255 0 0 0 0 0 0 0,0";
        StringBuilder imageIds = new StringBuilder();
        long jsImageIdKey;
        int x;
        int y;
        int jsImageId;
        if (length > 0) {
            Object[] textureNativeData;
            StringBuilder sb = new StringBuilder();
            int j;
            for (int i = 0; i < length; i += 16) {
                if (events[i] == 6) {

                    textureNativeData = imageNativeData[imageIndex++];
                    textureResourceNativeData = (Object[]) textureNativeData[0];

                    if (!(boolean) textureResourceNativeData[1]) {
                        textureResourceNativeData[2] = ++textureId;
                        textureResourceNativeData[1] = true;
                    }
                    x = (int)((double) textureNativeData[1] * (int) textureNativeData[5]);
                    y = (int)((double) textureNativeData[2] * (int) textureNativeData[6]);
                    jsImageIdKey = ((((int) textureResourceNativeData[2]) * 4096L) + x) * 4096L + y;
                    if (!jsImageIds.containsKey(jsImageIdKey)) {
                        int right = (int)((double) textureNativeData[3] * (int) textureNativeData[5]);
                        int bottom = (int)((double) textureNativeData[4] * (int) textureNativeData[5]);
                        jsImageId = loadTexture(
                                jsImageIdKey,
                                (CrayonBitmap) textureResourceNativeData[3],
                                x,
                                y,
                                right - x,
                                bottom - y);
                    } else {
                        jsImageId = jsImageIds.get(jsImageIdKey);
                    }
                    imageIds.append(jsImageId);
                    imageIds.append(' ');
                }
                for (j = 0; j < 16; ++j) {
                    sb.append(' ');
                    sb.append(events[i + j]);
                }
            }
            sb.append(',');
            sb.append(imageIds);
            sb.append('0');
            msg = sb.toString().trim();
        }
        this.sendMessage("clock-tick", msg, false);
    }

    private String urlEncode(String value) {
        int length = value.length();
        if (length == 0) return "";
        StringBuilder sb = new StringBuilder();
        sb.append((int)value.charAt(0));
        for (int i = 1; i < length; ++i) {
            sb.append(' ');
            sb.append((int)value.charAt(i));
        }
        return sb.toString();
    }

    private ArrayList<String> messageQueue = new ArrayList<>();
    private void sendMessage(String type, String msg, boolean useUrlEncode) {
        if (this.readyToReceiveMessages) {
            if (useUrlEncode) {
                type = urlEncode(type);
                msg = urlEncode(msg);
            }
            final String js = "javascript:receiveMessage('" + type + "', '" + msg + "', " + (useUrlEncode ? "true" : "false") + ")";
            final WebView wv = this;
            this.post(new Runnable() {
                @Override
                public void run() {
                    wv.loadUrl(js);
                }
            });
        } else {
            messageQueue.add(type);
            messageQueue.add(msg);
        }
    }

    void receiveMessage(String type, String msg) {
        switch (type) {

            case "ready":
                this.readyToReceiveMessages = true;
                for (int i = 0; i < messageQueue.size(); i += 2) {
                    this.sendMessage(messageQueue.get(i), messageQueue.get(i + 1), false);
                }
                messageQueue.clear();
                clockTick();
                break;

            case "trigger-next-frame":
                InterpreterResult ir = Interpreter.v_interpret(this.executionContextId);

                switch (ir.status) {
                    case 3:
                        throw new RuntimeException(ir.errorMessage);
                }
                break;

            default:
                throw new RuntimeException("Unknown message: " + type);
        }
    }

    public class JavaScriptBridge {

        private GameLibWebView webView;

        public JavaScriptBridge(GameLibWebView webView) {
            this.webView = webView;
        }

        @JavascriptInterface
        public void onSendNativeMessage(String type, String rawValue) {
            this.webView.receiveMessage(type, rawValue);
        }
    }

    private class GameLibWebViewClient extends WebViewClient {

        @Override
        public boolean shouldOverrideUrlLoading(WebView view, String url) {

            if(Uri.parse(url).getHost().length() == 0) {
                return false;
            }

            Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
            view.getContext().startActivity(intent);
            return true;
        }
    }
}
