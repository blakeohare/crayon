package %%%PACKAGE%%%;

class LibraryWebBrowserLauncher {
    public static void launchBrowser(String url) {
        if(java.awt.Desktop.isDesktopSupported())
        {
            try {
                java.awt.Desktop.getDesktop().browse(new java.net.URI(url));
            } catch (java.io.IOException | java.net.URISyntaxException e) {
            }
        }
    }
}
