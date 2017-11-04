import os
import array

class FormatStyle:
  def __init__(self):
    self.newline_char = '\n'
    self.tab_char = '    '
    self.should_rtrim = True
    self.should_trim = True
    self.should_end_with_newline = True
    self.canonicalize_csproj_tools_version = False

  def tabs(self, tab_char):
    self.tab_char = tab_char
    return self

  def newline(self, newline_char):
    self.newline_char = newline_char
    return self

  def rtrim(self, should_rtrim):
    self.should_rtrim = should_rtrim
    return self

  def disableEndNewline(self):
    self.should_end_with_newline = False
    return self

  def enableCanonicalizeCsprojToolsVersion(self):
    self.canonicalize_csproj_tools_version = True
    return self

  def apply(self, text):
    if self.should_trim:
      text = text.strip()

    if self.canonicalize_csproj_tools_version:
      if 'ToolsVersion="' in text and not 'ToolsVersion="14.0"' in text:
        tools_version_loc = text.find('ToolsVersion=')
        first_quote = text.find('"', tools_version_loc)
        close_quote = text.find('"', first_quote + 1)
        text = text[:first_quote] + '"14.0' + text[close_quote:]

    lines = text.replace('\r\n', '\n').split('\n')

    if self.should_rtrim:
      lines = map(lambda x:x.rstrip(), lines)

    new_lines = []
    for line in lines:
      tabs = 0
      trimming = True
      while trimming:
        if len(line) > 0:
          if line[0] == '\t':
            line = line[1:]
            tabs += 1
          elif line.startswith(self.tab_char):
            line = line[len(self.tab_char):]
            tabs += 1
          else:
            trimming = False
        else:
          trimming = False
      if tabs > 0:
        new_lines.append((self.tab_char * tabs) + line)
      else:
        new_lines.append(line)

    if self.should_end_with_newline:
      new_lines.append('')
    
    text = '\n'.join(new_lines)

    return text

CSHARP_STYLE = FormatStyle().tabs(' ' * 4).newline('\r\n')
CRAYON_STYLE = FormatStyle().tabs(' ' * 4).newline('\n')
CSPROJ_STYLE = FormatStyle().tabs(' ' * 2).newline('\r\n').disableEndNewline().enableCanonicalizeCsprojToolsVersion()
PASTEL_STYLE = FormatStyle().tabs(' ' * 4).newline('\n')
PYTHON_STYLE_2_SPACES = FormatStyle().tabs(' ' * 2).newline('\n')
PYTHON_STYLE_4_SPACES = FormatStyle().tabs(' ' * 4).newline('\n')

MATCHERS = [
  # C#
  ('Build/*.cs', CSHARP_STYLE),
  ('Common/*.cs', CSHARP_STYLE),
  ('Compiler/*.cs', CSHARP_STYLE),
  ('Interpreter/*.cs', CSHARP_STYLE),
  ('Localization/*.cs', CSHARP_STYLE),
  ('Parser/*.cs', CSHARP_STYLE),
  ('Pastel/*.cs', CSHARP_STYLE),
  ('Platform/*.cs', CSHARP_STYLE),
  ('Platforms/*.cs', CSHARP_STYLE),
  ('Build/*.csproj', CSPROJ_STYLE),
  ('Common/*.csproj', CSPROJ_STYLE),
  ('Compiler/*.csproj', CSPROJ_STYLE),
  ('Interpreter/*.csproj', CSPROJ_STYLE),
  ('Localization/*.csproj', CSPROJ_STYLE),
  ('Parser/*.csproj', CSPROJ_STYLE),
  ('Pastel/*.csproj', CSPROJ_STYLE),
  ('Platform/*.csproj', CSPROJ_STYLE),
  ('Platforms/*.csproj', CSPROJ_STYLE),

  # Pastel
  ('Interpreter/*.pst', PASTEL_STYLE),

  # Demos
  ('Demos/*.cry', CRAYON_STYLE),

  # Libraries
  ('Libraries/*.pst', PASTEL_STYLE),
  ('Libraries/*.cry', CRAYON_STYLE),

  # Clean Thyself
  ('style-fixer.py', PYTHON_STYLE_2_SPACES),
]

def get_all_files():
  output = []
  get_all_files_impl('.', output)
  return output

def get_all_files_impl(path, output):
  for file in os.listdir(path):
    full_path = path + os.sep + file
    if os.path.isdir(full_path):
      if not ('/obj/Debug' in full_path) and not ('/obj/Release' in full_path):
        get_all_files_impl(full_path, output)
    else:
      output.append(full_path[2:])

def main():
  all_files = get_all_files()
  for pattern, matcher in MATCHERS:
    if '*' in pattern:
      prefix, ext = pattern.split('*')
    else:
      parts = pattern.split('.')
      prefix = '.'.join(parts[:-1])
      ext = parts[-1]
    for file in all_files:
      canonical_file = file.replace('\\', '/')
      if canonical_file .startswith(prefix) and canonical_file .endswith(ext):
        text = read_text(file)
        text = matcher.apply(text)
        write_text(file, text, matcher.newline_char)

def read_text(path):
  c = open(path, 'rt')
  text = c.read()
  c.close()
  return text

def write_text(path, text, newline_char):

  bytes = array.array('B', text)
  if newline_char == '\n':
    new_bytes = bytes
  else:
    new_bytes = []
    for byte in bytes:
      if byte == 10:
        new_bytes.append(13)
        new_bytes.append(10)
      else:
        new_bytes.append(byte)

  c = open(path, 'rb')
  original = c.read()
  c.close()

  old_bytes = array.array('B', original)
  update = False
  if len(old_bytes) != len(new_bytes):
    update = True
  elif len(old_bytes) > 0:
    if old_bytes[-1] != new_bytes[-1]:
      update = True
    else:
      for old, new in zip(old_bytes, new_bytes):
        if old != new:
          update = True
          break

  if update:
    print("Updating: " + path)
    c = open(path, 'wb')
    c.write(bytearray(new_bytes))
    c.close()

main()
