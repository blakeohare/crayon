import UIKit
import WebKit

class ViewController: UIViewController, WKScriptMessageHandler, UIScrollViewDelegate {

    var webView : WKWebView?;
    var clickCatcher : ClickCatcher?;
    
    override func viewDidLoad() {
        super.viewDidLoad()
        let userController:WKUserContentController = WKUserContentController();
        userController.add(self, name: "interop"); // TODO: split this out into specific verbs.
        userController.add(self, name: "viewType");
        let webViewConfig = WKWebViewConfiguration();
        webViewConfig.userContentController = userController;
        let webView = WKWebView(frame: self.view.frame, configuration: webViewConfig);
        webView.scrollView.bounces = false;
        webView.scrollView.isScrollEnabled = false;
        webView.autoresizingMask = UIViewAutoresizing.flexibleWidth.union(UIViewAutoresizing.flexibleHeight);
        let htmlPath = Bundle.main.url(forResource: "index", withExtension: "html", subdirectory: "jsres");
        let request = URLRequest(url: htmlPath!);
        webView.load(request);
        self.view.addSubview(webView);
        let cc = ClickCatcher(frame: self.view.frame, webView: webView);
        self.view.addSubview(cc);

        webView.scrollView.delegate = self;
        self.webView = webView;
        self.clickCatcher = cc;
    }
    
    override func didRotate(from fromInterfaceOrientation: UIInterfaceOrientation) {
        self.clickCatcher!.frame = self.view.frame;
    }

    func scrollViewWillBeginZooming(_ scrollView: UIScrollView, with view: UIView?) {
        scrollView.pinchGestureRecognizer?.isEnabled = false
    }

    func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
        let name = message.name;
        let body = String(describing: message.body);
        if (name == "interop") {
            print(body);
        } else if (name == "viewType") {
            if (body == "natural-web-view") {
                self.clickCatcher!.frame = CGRect(x: 0, y: 0, width: 0, height: 0);
            }
        }
    }
    
    class ClickCatcher : UIView {
        
        var webView : WKWebView?;
        
        init(frame: CGRect, webView : WKWebView) {
            super.init(frame: frame)
            self.webView = webView;
        }

        required init?(coder: NSCoder) {
            super.init(coder: coder);
        }
        
        override func touchesBegan(_ touches: Set<UITouch>, with event: UIEvent?) {
            invokeJsForMouseEvent(0, touches: touches);
        }
        
        override func touchesMoved(_ touches: Set<UITouch>, with event: UIEvent?) {
            invokeJsForMouseEvent(2, touches: touches);
        }
        
        override func touchesEnded(_ touches: Set<UITouch>, with event: UIEvent?) {
            invokeJsForMouseEvent(1, touches: touches);
        }
        
        func invokeJsForMouseEvent(_ type : Int, touches: Set<UITouch>) {
            let touch = touches.first! as UITouch;
            let currentPoint = touch.location(in: self);
            let xratio = currentPoint.x / self.frame.width;
            let yratio = currentPoint.y / self.frame.height;
            let js = "C$ios$invokePointer(\(type), \(xratio), \(yratio));";
            webView!.evaluateJavaScript(js, completionHandler: nil);
        }
    }
}
