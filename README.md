# Crayon version 0.1.6 (Alpha)
==============================================

Please go to http://crayonlang.org for documentation, tutorials, demos, and other resources.

# Copyright
Copyright 2015 crayonlang.org, All rights reserved.

# Licenses
Crayon is released under the MIT license.
OpenTK & Tao license information can be found at http://www.opentk.com/project/license

See the LICENSE.txt file included with this release for further information.

# Reporting Bugs
Please report any issues you may find to the GitHub issue tracker located at https://github.com/blakeohare/crayon/issues
It may be helpful to check the IRC channel first to make sure any issue you find is actually a bug or for workarounds.

# Community
The official Crayon IRC channel is #crayon on irc.esper.net. Feel free to ask any questions there. 
Google Mailing list/forum: https://groups.google.com/forum/#!forum/crayon-lang
Use the stackoverflow tag "crayon" for any issues. This tag is monitored.

# New in 0.1.6

## Major features:
* Limited music support in Python, JavaScript, and C# projects.
* Image scaling support.
* Added support for drawing arbitrary triangles.
* Fixed inconsistencies with keycodes across all platforms. 
* Added a null coallescing operator (C# style).
* Added a $launch_browser function.
* Added a primitive method to concatenate one list to the end of another.
* Negative indexes are valid in lists and strings (Python style).
* Added a .clone() method for dictionaries.
* Added default toString behavior for user object instances.
* Empty sprite sheets are no longer valid.

## Improvements and Bug Fixes:
* event.button field was causing crashes on Python and JavaScript platforms.
* Fixed bug where list slicing would return full list if start index was after end of the list.
* OpenTK projects were not sending close button and Alt-F4 quit events.
* Fixed bug where $gfx_fill_screen wasn't working on OpenGL based platforms.
* Fixed bug where the window would not close on Java projects that experience user-fault crashes.
* Fixed bug where $typeof was returning 'unknown_type' for booleans.
* Fixed sprite sheet bug that would create invalid sheets if an image touched exactly the edge of a y coordinate that was a multiple of 256.
* Fixed crash in JavaScript that occurred while writing to the mock disk.
* Fixed compiler crash if the source or output folder was missing.
* Fixed compiler crash if compiling while a file is in use by the OS.
* Fixed compiler crash when project ID was missing.
* Throw a compile error if subclass was referenced before it was declared (as opposed to VM runtime crash)
* Fixed base.method() support. Invoke the implementation in the subclass as one would expect.
* Fixed bug where alpha value was being ignored when drawing ellipses in Python.
* Added SDL.dll and SDL_mixer.dll to C# project output as this is required to play music.
* Fix bug where images in sprite sheets were getting drawn twice into the top left tile that they occupy, which is noticeable for images that have opacity.
* Fixed missing $parse_float implementation in JavaScript and Python.
