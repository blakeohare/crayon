#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/socket.h>
#include <sys/un.h>
#include <unistd.h>
#include <errno.h>
#include "u3util.h"

typedef struct _U3SocketClient {
  int client_socket;
  const char* err;

} U3SocketClient;

typedef struct _U3SocketServer {
  int listen_socket;
  int downstream_socket;
  const char* err;
  List* message_queue;
  StringBuilder* message_buffer;
} U3SocketServer;

int start_receiving(U3SocketServer* ss, void (*on_message)(const char* msg), int only_one) {
  if (ss->downstream_socket == 0) {
    struct sockaddr remote_addr;
    socklen_t addr_len = sizeof(remote_addr);
    ss->downstream_socket = accept(ss->listen_socket, &remote_addr, &addr_len);
    if (ss->downstream_socket == -1) {
      ss->err = "Could not accept client socket";
      return 0;
    }
  }

  StringBuilder* message_builder = NULL;

  int keep_going = 1;
  int is_int_parse_mode = 1;
  int payload_size = 0;
  char buffer[100];
  while (keep_going) {
    int bytes_read = recv(ss->downstream_socket, buffer, 100, 0);
    if (bytes_read == -1) {
      ss->err = "Error reading downstream bytes";
      return 1;
    }
    if (bytes_read == 0) {
      return 0;
    }

    for (int i = 0; i < bytes_read; i++) {
      char c = buffer[i];
      if (is_int_parse_mode) {
        if (c == '@') {
          is_int_parse_mode = 0;
          message_builder = new_string_builder();
          if (payload_size == 0) {
            is_int_parse_mode = 1;
            char* msg = StringBuilder_to_string_and_free(message_builder);
            on_message(msg);
            free(msg);
            message_builder = NULL;
          }
        } else if (c < '0' || c > '9') {
          // Ignoring out of range characters
        } else {
          payload_size = payload_size * 10 + (c - '0');
        }
      } else {
        StringBuilder_append_char(message_builder, c);
        if (message_builder->size == payload_size) {
          char* msg = StringBuilder_to_string_and_free(message_builder);
          char* msg_decoded = from_base64(msg);
          free(msg);
          on_message(msg_decoded);
          free(msg_decoded);
          message_builder = NULL;
          is_int_parse_mode = 1;
          if (only_one) return 1;
          payload_size = 0;
        }
      }
    }
  }

  return 1;
}

int init_server_socket(U3SocketServer* ss, const char* path) {

  ss->downstream_socket = 0;

  struct sockaddr_un local;
  char str[100];

  local.sun_family = AF_UNIX;
  strcpy(local.sun_path, path);
  unlink(local.sun_path);

  ss->listen_socket = socket(AF_UNIX, SOCK_STREAM, 0);
  if (ss->listen_socket == -1) {
    ss->err = "Could not establish unix socket server.";
    return 0;
  }

  int len = (offsetof (struct sockaddr_un, sun_path) + strlen (local.sun_path) + 1);
  if (bind(ss->listen_socket, (struct sockaddr*)&local, len) == -1) {
    printf("bind error\n");
    return 0;
  }

  if (listen(ss->listen_socket, 5) == -1) {
    printf("listen error\n");
    return 0;
  }

  return 1;
}

int init_client_socket(U3SocketClient* cs, const char* path) {
  cs->err = NULL;
  cs->client_socket = -1;
  
  cs->client_socket = socket(AF_UNIX, SOCK_STREAM, 0);
  if (cs->client_socket == -1) {
    cs->err = "Error establishing client socket";
    return 0;
  }

  struct sockaddr_un addr;
  addr.sun_family = AF_UNIX;
  strcpy(addr.sun_path, path);
  int len = (offsetof (struct sockaddr_un, sun_path) + strlen (addr.sun_path) + 1);

  int err = connect(cs->client_socket, (struct sockaddr*)&addr, len);
  if (err == -1) {
    cs->err = "client socket error connecting";
    return 0;
  }

  return 1;
}

int client_send_string(U3SocketClient* cs, const char* msg) {

  char* b64_msg = to_base64(msg);
  StringBuilder* sb = new_string_builder();
  int len = strlen(b64_msg);
  StringBuilder_append_int(sb, len);
  StringBuilder_append_char(sb, '@');
  StringBuilder_append_chars(sb, b64_msg);
  int encoded_msg_len = sb->size;
  char* encoded_msg = StringBuilder_to_string_and_free(sb);
  int err = send(cs->client_socket, encoded_msg, encoded_msg_len, 0);
  if (err == -1) {
    cs->err = "Client error while sending to server.";
    return 0;
  }
  return 1;
}
