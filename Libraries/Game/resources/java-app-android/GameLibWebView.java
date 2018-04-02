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

public class GameLibWebView extends WebView {

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
        //this.loadUrl("file:///android_asset/index.html");
        this.loadData("<html><body>Hello, World! (direct string) </body></html>", "text/html", "UTF-8");
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

    private void sendMessage(String type, String msg) {
        this.loadUrl("javascript:receiveMessage('" + urlEncode(type) + "', '" + urlEncode(msg) + "')");
    }

    void receiveMessage(String type, String msg) {
        Log.d(type, msg);
    }

    public class JavaScriptBridge {

        private GameLibWebView activity;

        public JavaScriptBridge(GameLibWebView activity) {
            this.activity = activity;
        }

        @JavascriptInterface
        public void onSendNativeMessage(String type, String rawValue) {
            this.activity.receiveMessage(type, rawValue);
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
