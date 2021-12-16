#include <stdio.h>
#include <stdlib.h>
#include "webview.h"

typedef struct _WindowDb {
	webview_t* windows;
	int size;
	int capacity;
	int next_id;
} WindowDb;

extern "C" {
#define MODULE_API

void* initialize_window_db() {
	WindowDb* db = (WindowDb*)malloc(sizeof(WindowDb));
	db->capacity = 4;
	db->windows = (webview_t*)malloc(sizeof(webview_t) * db->capacity);
	db->size = 0;
	return (void*) db;
}

void* create_window(void* _db) {
	WindowDb* db = (WindowDb*) _db;
	if (db->capacity == db->size) {
		int new_capacity = db->capacity * 2;
		webview_t* new_windows = (webview_t*)malloc(sizeof(webview_t) * new_capacity);
		for (int i = 0; i < db->capacity; i++) {
			new_windows[i] = db->windows[i];
		}
		free(db->windows);
		db->windows = new_windows;
		db->capacity = new_capacity;
	}
	webview_t win = webview_create(0, NULL);
	db->windows[db->size++] = win;
	return win;
}

int set_window_title(void* win, const char* title) {
	webview_set_title((webview_t)win, title);
	return 1;
}

int set_window_size(void* win, int width, int height) {
	webview_set_size((webview_t)win, width, height, WEBVIEW_HINT_NONE);
	return 1;
}

int set_window_uri(void* win, const char* uri) {
	webview_navigate((webview_t)win, uri);
	return 1;
}

int run_window_till_closed(void* win) {
	webview_run((webview_t)win);
	return 1;
}

void release_window(void* _db, void* win) {
	WindowDb* db = (WindowDb*)_db;
	
	for (int i = 0; i < db->size; i++) {
		if (db->windows[i] == win) {
			db->windows[i] = db->windows[db->size - 1];
			break;
		}
	}
	db->size--;
	webview_destroy((webview_t)win);
}

}
