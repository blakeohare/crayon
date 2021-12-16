#include <stdio.h>
#include "webview.h"

extern "C" {
#define MODULE_API

int show_window(const char* title, int width, int height, const char* dataUri);

}

int show_window(const char* title, int width, int height, const char* dataUri) {
	webview_t w = webview_create(0, NULL);
	webview_set_title(w, title);
	webview_set_size(w, width, height, WEBVIEW_HINT_NONE);
	webview_navigate(w, dataUri);
	webview_run(w);
	webview_destroy(w);

	return 1;
}
