import UIKit
import WebKit

class ViewController: UIViewController, WKUIDelegate, WKScriptMessageHandler {

    var webView: WKWebView!
    
    func handleMessageFromJavaScript(msgType: String, msgValue: String) {
        switch msgType {
        case "READY":
            print("The webview has finished loading")
        case "STDOUT":
            print("\(msgValue)")
        default:
            print("Unknown message type: \(msgType)")
        }
    }
    
    func sendMessageToWebview(msgType: String, msgValue: String?) {
        let msgTypeB64 = stringToB64(msgType)
        let msgValueB64 = msgValue != nil ? stringToB64(msgValue) : ""
        let js = "sendStringToJavaScript('\(msgTypeB64):\(msgValueB64)')"
        webView.evaluateJavaScript(js, completionHandler: nil)
    }
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        let myURL = Bundle.main.url(forResource: "index", withExtension: "html", subdirectory: "jsres")
        let myRequest = URLRequest(url: myURL!)
        webView.load(myRequest)
    }

    override func loadView() {
        let webConfiguration = WKWebViewConfiguration()
        let userController = WKUserContentController();
        userController.add(self, name: "interop");
        webConfiguration.userContentController = userController;
        webView = WKWebView(frame: .zero, configuration: webConfiguration)
        webView.uiDelegate = self
        view = webView
    }
    
    func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
        let body = String(describing: message.body);
        let parts = body.split(separator: ":")
        let msgTypeB64 = "\(parts[0])";
        let msgValueB64 = "\(parts.count == 1 ? "" : parts[1])";
        let msgType = b64ToString(msgTypeB64)
        let msgValue = b64ToString(msgValueB64)
        handleMessageFromJavaScript(msgType: msgType, msgValue: msgValue)
    }
    
    func b64ToString(_ original: String!) -> String {
        if let base64Decoded = Data(base64Encoded: original, options: Data.Base64DecodingOptions(rawValue: 0))
            .map({ String(data: $0, encoding: .utf8) }) {
            return "\(base64Decoded ?? "")"
        }
        return ""
    }
    
    func stringToB64(_ original: String!) -> String {
        let utf8Str = original.data(using: .utf8)
        let base64Encoded = utf8Str?.base64EncodedString(options: Data.Base64EncodingOptions(rawValue: 0))
        return base64Encoded!
    }
}
