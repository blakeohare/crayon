# Crayon version 0.2.0 (Alpha)
==============================================

Please go to http://crayonlang.org for documentation, tutorials, demos, and other resources.

[![Build Status](https://travis-ci.org/blakeohare/crayon.svg?branch=master)](https://travis-ci.org/blakeohare/crayon)

# Copyright
Copyright 2016 crayonlang.org, All rights reserved.

# Licenses
Crayon is released under the MIT license.
OpenTK & Tao license information can be found at http://www.opentk.com/project/license

See the LICENSE.txt file included with this release for further information.

# Reporting Bugs
Please report any issues you may find to the GitHub issue tracker located at https://github.com/blakeohare/crayon/issues
It may be helpful to check the IRC channel first to make sure any issue you find is actually a bug or for workarounds.

# Community
The official Crayon IRC channel is #crayon on FreeNode.net. Feel free to ask any questions there. 
Google Mailing list/forum: https://groups.google.com/forum/#!forum/crayon-lang
Use the stackoverflow tag "crayon" for any issues. This tag is monitored.

# New in 0.2.0

## Large fundamental changes in the language:
* Namespaces have been added.
* Loose code is no longer allowed. All code must exist within functions or classes.
* Classes can now have static members, methods, and constructors.
* Fields must be explicitly declared.
* Execution begins in a function called main, which may optionally take in a list of arguments.
* System functions with $ syntax have been eliminated. All libraries are imported and contains code that looks like normal code.
* File imports are no longer used. All code files in a project are automatically detected and included in compilation.
* References to classes, namespaces, functions, constants, and enums are resolved the same way regardless of the order in which they are declared.
* It is now theoretically possible to create third-party libraries for Crayon, although this will likely drastically evolve over the course of the next few versions and as a result, is not documented at all.

## Primitive method changes:
* list.add can take any number of arguments and will append all those items to the end of the list.
* list.sort can optionally take in a function pointer to generate a string or numeric key to sort items by
* dictionary.get can either take 1 or 2 parameters. The 1-parameter version uses null as a default value when the key is not found

## Other fixes:
* Fixed bug where an infinite loop would be generated in certain specific cases when multiple controllers are being initialized.
* Fixed bug where you could not assign to a negative list index in the same way you could reference a negative index.
* Enums now have an implicit field called .length, which returns the number of items in it.

## Included Libraries
This is a current list of the included libraries, and a brief description of which $-prefixed system 
functions from 0.1 it replaces. For usage details, see the library documentation at http://crayonlang.org/docs/0.2.0
* Core - Basic system functions ($print, $assert, $chr, $parse_int, etc)
* Easing - Easing functions (completely new)
* FileIO - All functions beginning with $io and $user_data
* Game - Game-window/loop/event related tasks. $clock_tick, $game_initialize_screen, $game_pump_events, etc.
* Gamepad - All $gamepad functionality
* GFX - All $gfx drawing functionality
* HTTP - $http_request
* JSON - $parse_json
* Math - All math functions, except $random()
* Random - Series of new random functions
* Resources - $resource_read_text
* SFX - All sound and music functions
* Web - $launch_browser
