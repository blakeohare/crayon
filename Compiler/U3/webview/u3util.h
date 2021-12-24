#include <stdlib.h>
#include <string.h>

typedef struct _StringBuilder {
  char* str;
  int size;
  int capacity;
  int queued_bits;
  int queued_bit_count;
} StringBuilder;

typedef struct _List {
  void** items;
  int size;
  int capacity;
  int start;
} List;

StringBuilder* new_string_builder() {
  StringBuilder* sb = (StringBuilder*) malloc(sizeof(StringBuilder));
  sb->size = 0;
  sb->capacity = 16;
  sb->queued_bit_count = 0;
  sb->str = (char*) malloc(sizeof(char) * sb->capacity);
  return sb;
}

void StringBuilder_append_char(StringBuilder* sb, char c) {
  if (sb->size == sb->capacity) {
    int new_capacity = sb->capacity * 2;
    char* new_str = (char*) malloc(sizeof(char) * new_capacity);
    for (int i = 0; i < sb->size; i++) {
      new_str[i] = sb->str[i];
    }
    free(sb->str);
    sb->str = new_str;
    sb->capacity = new_capacity;
  }
  sb->str[sb->size++] = c;
}

void StringBuilder_append_b64_char(StringBuilder* sb, char c) {
  int value = 0;
  if (c >= 'A' && c <= 'Z') {
    value = c - 'A';
  } else if (c >= 'a' && c <= 'z') {
    value = c - 'a' + 26;
  } else if (c >= '0' && c <= '9') {
    value = c - '0' + 52;
  } else if (c == '+') {
    value = 62;
  } else if (c == '/') {
    value = 63;
  } else if (c == '=') {
    sb->queued_bit_count = 0;
    return;
  }

  int pairs[3];
  pairs[0] = (value >> 4) & 3;
  pairs[1] = (value >> 2) & 3;
  pairs[2] = value & 3;

  for (int i = 0; i < 3; i++) {
    sb->queued_bits = (sb->queued_bits << 2) | pairs[i];
    sb->queued_bit_count += 2;
    if (sb->queued_bit_count == 8) {
      StringBuilder_append_char(sb, (char)sb->queued_bits);
      sb->queued_bits = 0;
      sb->queued_bit_count = 0;
    }
  }
}

void StringBuilder_append_chars(StringBuilder* sb, const char* str) {
  char* cptr = (char*)(void*)str;
  while (cptr[0] != '\0') {
    StringBuilder_append_char(sb, cptr[0]);
    cptr++;
  }
}

void StringBuilder_append_int(StringBuilder* sb, int value) {
  if (value == 0) {
    StringBuilder_append_char(sb, '0');
    return;
  }

  if (value < 0) {
    StringBuilder_append_char(sb, '-');
    value *= -1;
  }

  char buffer[20];
  int i = 19;
  buffer[i] = '\0';
  while (value > 0) {
    int digit = value % 10;
    buffer[--i] = digit + '0';
    value = value / 10;
  }
  StringBuilder_append_chars(sb, buffer + i);
}

char* StringBuilder_to_string_and_free(StringBuilder* sb) {
  StringBuilder_append_char(sb, '\0');
  // TODO: should snip this to the appropriate size, but this is fine for now.
  char* str = sb->str;
  free(sb);
  return str;
}

List* new_list() {
  List* list = (List*) malloc(sizeof(List));
  list->size = 0;
  list->start = 0;
  list->capacity = 10;
  list->items = (void**) malloc(sizeof(void*) * list->capacity);
  return list;
}

void list_add(List* list, void* item) {
  if (list->size == list->capacity) {
    int new_capacity = list->capacity * 2 + 1;
    void** new_items = (void**) malloc(sizeof(void*) * new_capacity);
    for (int i = 0; i < list->size; i++) {
      new_items[i] = list->items[(i + list->start) % list->capacity];
    }
    free(list->items);
    list->items = new_items;
    list->capacity = new_capacity;
    list->start = 0;
  }
  list->items[(list->start + list->size++) % list->capacity] = item;
}

void* list_get(List* list, int index) {
  if (list->size == 0) return NULL;
  void* item = list->items[(list->start + index) % list->capacity];
  return item;
}

void* list_dequeue(List* list) {
  void* item = list_get(list, 0);
  if (item == NULL) return NULL;
  list->size--;
  list->start = (list->start + 1) % list->capacity;
  return item;
}

void* list_pop(List* list) {
  if (list->size == 0) return NULL;
  void* item = list_get(list, list->size - 1);
  list->size--;
  return item;
}

char* to_base64(const char* text) {
  int excess = 0;
  int excess_bits = 0;
  StringBuilder* sb = new_string_builder();
  const char* b64alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
  int p[4];
  for (int i = 0; text[i] != '\0'; i++) {
    int c = text[i];
    p[0] = (c >> 6) & 3;
    p[1] = (c >> 4) & 3;
    p[2] = (c >> 2) & 3;
    p[3] = c & 3;
    for (int j = 0; j < 4; j++) {
      switch (excess_bits) {
        case 0:
          excess = p[j] << 4;
          excess_bits += 2;
          break;
        case 2:
          excess |= (p[j] << 2);
          excess_bits += 2;
          break;
        case 4:
          excess |= p[j];
          excess_bits = 0;
          StringBuilder_append_char(sb, b64alphabet[excess]);
          break;
      }
    }
  }

  if (excess_bits != 0) {
    StringBuilder_append_char(sb, b64alphabet[excess]);
  }
  while (sb->size % 4 != 0) {
    StringBuilder_append_char(sb, '=');
  }
  return StringBuilder_to_string_and_free(sb);
}

char* from_base64(char* b64) {
  StringBuilder* sb = new_string_builder();
  for (int i = 0; b64[i] != '\0'; i++) {
    StringBuilder_append_b64_char(sb, b64[i]);
  }
  return StringBuilder_to_string_and_free(sb);
}

void string_split_once(const char* str, char sep, char** out1, char** out2) {
  char* part1 = NULL;
  char* part2 = NULL;

  *out2 = NULL;
  StringBuilder* sb = new_string_builder();
  for (int i = 0; str[i] != '\0'; i++) {
    char c = str[i];
    if (c == sep && part1 == NULL) {
      part1 = StringBuilder_to_string_and_free(sb);
      sb = new_string_builder();
    } else {
      StringBuilder_append_char(sb, c);
    }
  }
  if (part1 == NULL) {
    part1 = StringBuilder_to_string_and_free(sb);
  } else {
    part2 = StringBuilder_to_string_and_free(sb);
  }

  *out1 = part1;
  *out2 = part2;
}
