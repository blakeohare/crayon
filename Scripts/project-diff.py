import os
import sys

def main(args):
  if len(args) != 2:
    print("Usage: python project-diff.py [path-to-project-1] [path-to-project-2]")
    return

  dir1 = args[0]
  dir2 = args[1]

  project1 = collect_text_files(dir1)
  project2 = collect_text_files(dir2)

  files_only_in_1 = []
  files_only_in_2 = []
  files_in_both = []
  perform_venn_analysis(set(project1.keys()), set(project2.keys()), files_only_in_1, files_only_in_2, files_in_both)

  if len(files_only_in_1) > 0:
    print("The following files are only in Project 1:")
    for file in files_only_in_1:
      print("  " + file)
    print("")

  if len(files_only_in_2) > 0:
    print("The following files are only in Project 2:")
    for file in files_only_in_2:
      print("  " + file)
    print("")

  print(str(len(files_in_both)) + " files in both projects.")
  print("")

  files_in_both.sort()
  files_with_diffs = []

  for file in files_in_both:
    text_1 = project1[file]
    text_2 = project2[file]
    diff = perform_diff(text_1, text_2)
    if len(diff) > 0:
      files_with_diffs.append(file)
      print("There's a difference in " + file)
      print("\n".join(diff))
      print("")

  if len(files_with_diffs) == 0:
    print("No files with text differences.")
  else:
    print("Diffs were in the following files:")
    print("\n".join(files_with_diffs))

  print("")


def perform_venn_analysis(set_a, set_b, only_in_a_out, only_in_b_out, in_both_out):
  for item in set_a:
    if item not in set_b:
      only_in_a_out.append(item)
    else:
      in_both_out.append(item)
  for item in set_b:
    if item not in set_a:
      only_in_b_out.append(item)


def collect_text_files(root):
  output = {}

  root = root.replace('\\', '/')
  if root.endswith('/'):
    root = root[:-1]

  collect_text_files_impl(root, '', output)

  return output

def get_file_extension(file):
  if '.' in file:
    return file.split('.')[-1].lower()
  return ''

FILE_EXTENSION_IGNORE_LIST = set([
  'png', 'jpg',
  'xcuserstate',
])

def is_text_file(path):
  ext = get_file_extension(path)
  return ext not in FILE_EXTENSION_IGNORE_LIST

def collect_text_files_impl(root, current_dir, output):

  full_dir = root

  if current_dir != '':
    full_dir += '/' + current_dir

  for file in os.listdir(full_dir.replace('/', os.sep)):
    full_file = full_dir + '/' + file
    if os.path.isdir(full_file.replace('/', os.sep)):
      next_cd = file if current_dir == '' else (current_dir + '/' + file)
      collect_text_files_impl(root, next_cd, output)
    else:
      rel_file = file if current_dir == '' else (current_dir + '/' + file)
      if is_text_file(rel_file):
        c = open(full_file.replace('/', os.sep), 'rt')
        text = c.read()
        c.close()
        output[rel_file] = text
      else:

        output[rel_file] = '\n'.join([
          "Binary file:",
          "size X", # TODO: get file size
          "first 20 bytes: ...", # TODO: this
          "last 20 bytes: ...", # TODO: do this as well
        ])

def perform_diff(text_1, text_2):
  if text_1 == text_2:
    return []

  lines_1 = text_1.split('\n')
  lines_2 = text_2.split('\n')

  trimmed_front = 0
  trimmed_back = 0

  # Remove identical lines at the beginning and end of the file
  while len(lines_1) > trimmed_front and len(lines_2) > trimmed_front and lines_1[trimmed_front] == lines_2[trimmed_front]:
    trimmed_front += 1
  lines_1 = lines_1[trimmed_front:]
  lines_2 = lines_2[trimmed_front:]

  while len(lines_1) > trimmed_back and len(lines_2) > trimmed_back and lines_1[-1 - trimmed_back] == lines_2[-1 - trimmed_back]:
    trimmed_back += 1
  lines_1 = lines_1[:-trimmed_back]
  lines_2 = lines_2[:-trimmed_back]

  length_1 = len(lines_1)
  length_2 = len(lines_2)

  grid = []

  for x in range(length_2 + 1):
    column = []
    for y in range(length_1 + 1):
      column.append(None)
    grid.append(column)

  # Perform levenshtein difference
  # each grid cell will consist of a tuple: (diff-size, previous-path: up|left|diag)

  # Each step to the right indicates taking a line from lines 2
  # Each step downwards indicates taking a line from lines 1

  # Prepopulate the left and top rows indicating starting the diff by removing all
  # lines from lines 1 and adding all lines from lines 2.
  for x in range(length_2 + 1):
    grid[x][0] = (x, 'left')
  for y in range(length_1 + 1):
    grid[0][y] = (y, 'up')
  grid[0][0] = (0, 'diag')

  # Populate the grid. Figure out the minimum diff to get to each point.
  for y in range(1, length_1 + 1):
    for x in range(1, length_2 + 1):
      if lines_1[y - 1] == lines_2[x - 1]:
        grid[x][y] = (grid[x - 1][y - 1][0], 'diag')
      elif (grid[x - 1][y][0] <= grid[x][y - 1][0]):
        grid[x][y] = (grid[x - 1][y][0] + 1, 'left')
      else:
        grid[x][y] = (grid[x][y - 1][0] + 1, 'up')

  # Start from the bottom right corner and walk backwards to the origin
  x = length_2
  y = length_1
  diff_chain = []
  ellipsis_used = False
  while x != 0 and y != 0:
    node = grid[x][y]
    if node[1] == 'diag':
      if not ellipsis_used:
        diff_chain.append('...')
        ellipsis_used = True
      x -= 1
      y -= 1
    elif node[1] == 'left':
      diff_chain.append('+ [' + str(trimmed_front + x) + '] ' + lines_2[x - 1])
      x -= 1
      ellipsis_used = False
    else:
      diff_chain.append('- [' + str(trimmed_front + y) + '] ' + lines_1[y - 1])
      y -= 1
      ellipsis_used = False

  diff_chain.reverse()

  return diff_chain

main(sys.argv[1:])
