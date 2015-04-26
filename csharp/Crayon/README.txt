Crayon version 0.1.4 (Alpha)
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
Changes since 0.1.3:
* Fix bug where stack info sometimes doesn't show in crashes.
* Fix bug where int % float and float % int weren't implemented in the op table.
* Strings are now allowed as iterable collections for foreach loops. 
* Fix error message wording that appears when you index into an unindexable type and it showed the type's enum value instead of the human-readable name.
* Catch common string method errors at compile time instead of runtime. Such as string.join(list). 
* Add string.indexOf(string) method.
* Add list.clone() method.
* Fix key bindings in all languages.
* Fix bug where you could read from image sheets if you hadn't checked to see if it was loaded yet. This created cross-platform problems in JavaScript.
* Fix egregious error where images didn't get populated into the image sheets correctly if they were above a certain size. 
* Fix == operator on objects.
* Key not found error message now shows the string value of the key.
* Add simple File IO system functions for C#, Java, and Python. 
* Introduce build file variables. 
* Implement an image blit function that supports scaling and cropping. 
* Fix typo in Python that was using "false" instead of "False" and causing an undeclared variable error.
