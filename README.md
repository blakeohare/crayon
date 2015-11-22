# Crayon version 0.1.7 (Alpha)
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

# New in 0.1.7

## All platforms:
* Added gamepad support (available in C# and Python only, although JavaScript support is planned for 0.1.8)
* Added sound support (except for Java projects).
* Added ability to draw images to the screen with an overall opacity applied to it.
* Using a variable that was never declared is now a compile-time error.
* Various performance improvements.
* Fixed bugs with switch statement. Fallthroughs are compile time errors.
* Fixed do-while loops.
* Fixed bug where $gfx_image_sheet_loaded was failing with weird random behavior if you didn't pass in a constant value.
* Fixed base.method() invocations.
* Fix $arctan compiler crash.
* Fix compiler crash when curly brace was not properly closed.

### C# Projects
* C# export is perfect. No bug fixes were necessary.

### JavaScript Projects
* Added a built-in JavaScript minifier.
* Semicolons were not being picked up properly in Chrome.
* Suppress key hold-n-repeat events.

### Java Projects
* Suppress key hold-n-repeat events.
* image flipping stopped working at some point. fixed now.
* list.sort() crash fixed.

### Python Projects
* Not calling $game_event_pump() should not cause window to hang.
* The error message generator for bad argument counts in primitive methods was causing the VM to crash.
