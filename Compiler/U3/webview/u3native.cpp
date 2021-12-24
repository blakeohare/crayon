/*
  C# launches socket server (token_us)
  C# launches u3native process
  u3native launches socket server (token_ds)
  u3native launches socket client (token_us)
  u3native sends socket ready message
  C# launches socket client (token_ds)
  C# sends init message including icon, keep aspect ratio, and inital data.

  All socket messages are in length + payload-base64 format delimited by @'s:
  {length n of message payload in decimal}@{payload in base64 with length n}
  once decoded, the mesage is always in JSON. 
*/

#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>
#include <sys/socket.h>
#include <sys/un.h>
#include "webview.h"
#include "u3socket.h"

typedef struct _U3State {
  int parent_pid;
  int width;
  int height;
  const char* title;
  const char* ds_path;
  const char* us_path;
  U3SocketClient* upstream_socket;
  U3SocketServer* downstream_socket;
  int secondary_upstream_socket_started;
} U3State;

static U3State state;

int parse_args(U3State* state, int argc, const char** argv) {
  state->parent_pid = -1;
  state->width = 400;
  state->height = 300;
  state->title = NULL;
  state->ds_path = NULL;
  state->us_path = NULL;
  state->secondary_upstream_socket_started = 0;

  for (int i = 1; i + 1 < argc; i += 2) {
    if (strcmp(argv[i], "pid") == 0) {
      state->parent_pid = atoi(argv[i + 1]);
    } else if (strcmp(argv[i], "width") == 0) {
      state->width = atoi(argv[i + 1]);
    } else if (strcmp(argv[i], "height") == 0) {
      state->height = atoi(argv[i + 1]);
    } else if (strcmp(argv[i], "title") == 0) {
      StringBuilder* sb = new_string_builder();
      const char* title_b64 = argv[i + 1];
      for (int j = 0; title_b64[j] != '\0'; j++) {
        StringBuilder_append_b64_char(sb, title_b64[j]);
      }
      state->title = StringBuilder_to_string_and_free(sb);
    } else if (strcmp(argv[i], "ds") == 0) {
      state->ds_path = argv[i + 1];
    } else if (strcmp(argv[i], "us") == 0) {
      state->us_path = argv[i + 1];
    } else {
      printf("Unrecognized arg: '%s'\n", argv[i]);
      return 0;
    }
  }

  if (state->title == NULL) {
    printf("Missing title\n");
    return 0;
  }

  if (state->ds_path == NULL) {
    printf("Missing downstream path\n");
    return 0;
  }

  if (state->us_path == NULL) {
    printf("Missing upstream path\n");
    return 0;
  }

  return 1;
}

static webview_t u3_window = NULL;

void on_downstream_message(const char* msg);

void* background_upstream_socket_watcher(void* _) {
    start_receiving(state.downstream_socket, &on_downstream_message, 0);
    return NULL;
}

void handle_message_from_js_binding(const char* seq, const char* req, void* arg) {
  // printf("JavaScript is saying this: %s | %s\n", seq, req);
  StringBuilder* sb = new_string_builder();
  StringBuilder_append_chars(sb, "VMJSON:");
  StringBuilder_append_chars(sb, req);
  char* vm_bound_msg = StringBuilder_to_string_and_free(sb);
  client_send_string(state.upstream_socket, vm_bound_msg);
  free(vm_bound_msg);

  if (!state.secondary_upstream_socket_started) {
    state.secondary_upstream_socket_started = 1;
    pthread_t thread_id;
    pthread_create(&thread_id, NULL, background_upstream_socket_watcher, NULL);
  }
}

void on_downstream_message(const char* msg) {
  char* msg_type;
  char* msg_payload;
  string_split_once(msg, ':', &msg_type, &msg_payload);

  char first_char = msg_type[0];
  if (first_char == 'J' && strcmp(msg_type, "JSON") == 0) {
    StringBuilder* sb = new_string_builder();
    StringBuilder_append_chars(sb, "(() => { window.receiveDownstreamData(\"");
    for (int i = 0; msg_payload[i] != '\0'; i++) {
      char c = msg_payload[i];
      switch (c) {
        case '"': StringBuilder_append_chars(sb, "\\\""); break;
        case '\n': StringBuilder_append_chars(sb, "\\n"); break;
        case '\r': StringBuilder_append_chars(sb, "\\r"); break;
        case '\t': StringBuilder_append_chars(sb, "\\t"); break;
        case '\\': StringBuilder_append_chars(sb, "\\\\"); break;
        default: StringBuilder_append_char(sb, c); break;
      }
    }
    StringBuilder_append_chars(sb, "\"); ");
    StringBuilder_append_chars(sb, "console_log('sent a ");
    StringBuilder_append_chars(sb, msg_type);
    StringBuilder_append_chars(sb, "'); ");
    StringBuilder_append_chars(sb, " })();");
    char* js_code = StringBuilder_to_string_and_free(sb);
    webview_eval(u3_window, js_code);
    webview_eval(u3_window, "console_log('sent');");
    free(js_code);
  } else if (first_char == 'S' && strcmp(msg_type, "SRC") == 0) {
    u3_window = webview_create(0, NULL);
    webview_set_title(u3_window, state.title);
    webview_set_size(u3_window, state.width, state.height, WEBVIEW_HINT_NONE);
    StringBuilder* src_uri_sb = new_string_builder();
    StringBuilder_append_chars(src_uri_sb, "data:text/html;base64,");
    char* b64_src = to_base64(msg_payload);
    StringBuilder_append_chars(src_uri_sb, b64_src);
    free(b64_src);
    char* src_uri = StringBuilder_to_string_and_free(src_uri_sb);
    webview_navigate(u3_window, src_uri);
    free(src_uri);
    free(msg_type);
    free(msg_payload);

    webview_bind(u3_window, "downstreamObj", handle_message_from_js_binding, NULL);

	  webview_run(u3_window); // This is a blocking function.
    webview_destroy(u3_window);
    return;
  } else {
    printf("Unrecognized message: %s\n", msg_type);
  }
  free(msg_type);
  if (msg_payload != NULL) free(msg_payload);
}

int main(int argc, const char** argv) {
  U3SocketServer _ds;
  U3SocketClient _us;
  state.downstream_socket = &_ds;
  state.upstream_socket = &_us;
  
  if (!parse_args(&state, argc, argv)) return 0;

  init_server_socket(state.downstream_socket, state.ds_path);
  init_client_socket(state.upstream_socket, state.us_path);
  client_send_string(state.upstream_socket, "READY");
  start_receiving(state.downstream_socket, &on_downstream_message, 1);
}
