package org.crayonlang.libraries.game;

import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.util.Log;
import android.webkit.ConsoleMessage;
import android.webkit.JavascriptInterface;
import android.webkit.WebChromeClient;
import android.webkit.WebView;
import android.webkit.WebViewClient;

import org.crayonlang.interpreter.AndroidTranslationHelper;
import org.crayonlang.interpreter.Interpreter;
import org.crayonlang.interpreter.structs.InterpreterResult;

import java.util.ArrayList;

public class GameLibWebView extends WebView {

    private boolean readyToReceiveMessages = false;

    public static void initializeGame(double fps) {
        AndroidTranslationHelper.switchToView(new GameLibWebView(fps, AndroidTranslationHelper.getMainActivity()));
    }

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

    private int executionContextId;
    public void setExecutionContextId(int executionContextId) {
        this.executionContextId = executionContextId;
    }

    public void initializeScreen(int gameWidth, int gameHeight) {
        this.sendMessage("screen-size", gameWidth + "," + gameHeight);
    }

    public void clockTick() {
        int[] events = GameLibDualStackHelper.eventList;
        int length = GameLibDualStackHelper.eventsLength;
        String msg = "1 0 0 100 100 0 0 0 255 0 0 0 0 0 0 0";
        if (length > 0) {
            StringBuilder sb = new StringBuilder();
            sb.append(events[0]);
            for (int i = 1; i < length; ++i) {
                sb.append(' ');
                sb.append(events[i]);
            }
            msg = sb.toString();
        }
        this.sendMessage("clock-tick", msg);
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
    private void sendMessage(String type, String msg) {
        if (this.readyToReceiveMessages) {
            // type = urlEncode(type);
            // msg = urlEncode(type);
            final String js = "javascript:receiveMessage('" + type + "', '" + msg + "')";
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
                    this.sendMessage(messageQueue.get(i), messageQueue.get(i + 1));
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
        Log.d(type, msg);
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

        @Override
        public void onPageFinished(WebView view, String url)
        {
        }
    }
}
