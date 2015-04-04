Crayon version 0.1.3 (Alpha)
==============================================

Please go to http://crayonlang.org for documentation, tutorials, demos, and other resources.

COPYRIGHT
---------
Copyright 2015 crayonlang.org, All rights reserved.

LICENSES
--------
Crayon is released under the MIT license.
OpenTK & Tao license information can be found at http://www.opentk.com/project/license

See the LICENSE.txt file included with this release for further information.

REPORTING BUGS
--------------
Please report any issues you may find to the GitHub issue tracker located at https://github.com/blakeohare/crayon/issues
It may be helpful to check the IRC channel first to make sure any issue you find is actually a bug or for workarounds.

COMMUNITY
---------
The official Crayon IRC channel is #crayon on irc.esper.net. Feel free to ask any questions there. 
Google Mailing list/forum: https://groups.google.com/forum/#!forum/crayon-lang
Use the stackoverflow tag "crayon" for any issues. This tag is monitored.

NEW IN THIS RELEASE
-------------------
Changes since 0.1.2:
* Fix string.split method in Java where some results were inconsistent with other platforms, despite regex escaping.
* Show a compile error instead of crashing the compiler when an import statement references a missing file. 
* JavaScript drawing functions now support alpha values.
* Fix event.is_mouse readonly field on game event objects which was previously returning false, always.
