Crayon version 0.1.5 (Alpha)
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
Changes since 0.1.4:

Major features:
* Added ability to make HTTP requests. See $http_request in documentation for details.
* Implement some File IO. Including ability to write to the default user data folder:
  - Windows: %APPDATA%\Project
  - Linux/Mac: ~/.Project
  - JavaScript: virtualized disk backed by localStorage
* Added list and string slicing, using the Python syntax e.g. list[start:end], list[start:end:step].
* $ord and $chr to convert to and from ASCII values and characters.
* $parse_float has been added.
* Added Ant build.xml file to Java output.
* Added support for multiplying strings by integers.

Improvements and Fixes:
* Improved ellipse rendering
* Fix bug so that ++ and -- can be used on indexed expressions.
* Fix bug where continue statements in for loops would skip the increment/step code.
* Change JavaScript output to use a black background instead of white.
* Fix bug where bit operators were not working (& | ^ << and >>)
* Added some basic unit testing.
* Fixed various compiler bugs and quirks (see commit history for details)
