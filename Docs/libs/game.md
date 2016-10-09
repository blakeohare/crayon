# Game Library

Basic functionality for opening a game window and dealing with input.

# Class: GameWindow

### GameWindow Constructor

`new GameWindow(title, fps, width, height, screenWidth = null, screenHeight = null)`

Creates _and_ displays a window with the given width, height, and title. 
The game loop will attempt to run at the given FPS but may potentially be restricted by platform constraints.
If the screenWidth and screenHeight, the window will appear at that width and height, but all graphical operations will treat the 
window as having a size of the regular `width` and `height` parameters and stretch the display to match the physical width and height.

For mobile platforms, screenWidth and screenHeight are entirely ignored and the system will define its own size.

| Argument | Type | Description |
| --- | --- | --- |
| **title** | _string_ | Title for the window chrome if present. |
| **fps** | _integer_ | Target frames per second to run the game loop at. This may possibly be ignored by the platform if the platform requires a certain FPS value. |
| **width** | _integer_ | Logical width of the game window screen. Graphical operations will use this value. |
| **height** | _integer_ | Logical height of the game window screen. Graphical operations will use this value. |
| **screenWidth** | _integer_ | Physical width of the screen. Will stretch the logical screen to this size. |
| **screenHeight** | _integer_ | Physical height of the screen. Will stretch the logical screen to this size. |

---

## GameWindow Methods

### clockTick

`gameWindow.clockTick()`

Indicates the end of one iteration of a game loop. 
The clockTick method will pause execution long enough such that the target frame rate is maintained. 
It will also potentially yield the main thread to the platform if necessary and pump the event queue, therefore it is not 
equivalent to calling `Core.sleep(t)`. If clockTick is not invoked, the window will become unresponsive. 

**Return value**: nothing

---

### pumpEvents

`gameWindow.pumpEvents()`

Returns a list of events that have occurred since the last time this method was invoked. 
Events are instances of (EventType)[#class-eventtype]

---

### setTitle

`gameWindow.setTitle(title)`

Sets the title of the window to a new value. 

| Argument | Type | Description |
| --- | --- | --- |
| **title** | _string_ | String to set the window title to. |

---

# Class: Event

This is an abstract class for various types of event objects.

## Fields

### event.type

`event.type`

Field indicating what kind of event this is. The value is from the enum (EventType)[#enum-eventtype].

---

# Class: QuitEvent

This is an event indicating one of various quit events.

## Fields

### quitEvent.quitType

`event.quitType`

Field indicating what kind of quit event this is. The value is from the enum (EventQuitType)[#enum-eventquittype].

---

# Class: KeyEvent

This is an event indicating some sort of keyboard event. The `.type` field will be either KEY_DOWN or KEY_UP.

## Fields

### keyEvent.key

`event.key`

This is the key that is pressed. This is an enum value from [KeyboardKey](#enum-keyboardkey), NOT a string value.

---

### keyEvent.down

`event.down`

This is a boolean indicating whether the key was pushed down. 
This is redundant with checking if the .type field is `EventType.KEY_DOWN` and exists as a convenience.

---

# Enum: EventType

Enum defining the various types of events.

| Value | Description |
| --- | --- |
| **QUIT** | A quit-related event. Such as pressing the window's close button. |
| **KEY_DOWN** | The user pressed a key. |
| **KEY_UP** | The user released a key. |
| **MOUSE_MOVE** | The user moved the mouse. |
| **MOUSE_LEFT_DOWN** | The user pressed the left mouse button. |
| **MOUSE_LEFT_UP** | The user released the left mouse button. |
| **MOUSE_RIGHT_DOWN** | The user pressed the right mouse button. |
| **MOUSE_RIGHT_UP** | The user released the right mouse button. |
| **MOUSE_SCROLL** | The user scrolled the mouse wheel. |
| **GAMEPAD_HARDWARE** | A gamepad button or axis was pushed. |
| **GAMEPAD** | A gamepad button or axis that has been configured was pushed. |

See also: (event.type)[#event-type]

---

# Enum: EventQuitType

Enum defining the various types of quit events.

See also: (QuitEvent)[#class-quitevent]

| Value | Description |
| --- | --- |
| **ALT_F4** | User pressed Alt + F4. |
| **CLOSE_BUTTON** | User pressed the close button on the window. |
| **BACK_BUTTON** | User pressed the back button on a mobile device. |

# Enum: KeyboardKey

Enum defining all possible keyboard keys that will occur in keyboard events.

Note that certain keys ought to be avoided for maximum compatibility. 
For example, the OS_COMMAND/CONTEXT_MENU keys will not be available on older keyboards and certain keys such as F1-F12 may 
have special meaning in browsers and will cause inintended side effects if the project is exported to JavaScript.

| Value | Description |
| --- | --- |
| **BACKSPACE** | |
| **TAB** | |
| **ENTER** | |
| **SHIFT** | Both left and right shift. |
| **CTRL** | Both left and right ctrl. |
| **ALT** | Both left and right alt. |
| **PAUSE** | |
| **CAPS_LOCK** | |
| **ESCAPE** | |
| **SPACE** | |
| **PAGE_UP** | |
| **PAGE_DOWN** | |
| **END** | |
| **HOME** | |
| **LEFT** | |
| **UP** | |
| **RIGHT** | |
| **DOWN** | |
| **PRINT_SCREEN** | |
| **INSERT** | |
| **DELETE** | |
| **NUM_0** | The following enum values correspond to both the numbers at the top of the keyboard and the num pad. No distinction is made. |
| **NUM_1** | |
| **NUM_2** | |
| **NUM_3** | |
| **NUM_4** | |
| **NUM_5** | |
| **NUM_6** | |
| **NUM_7** | |
| **NUM_8** | |
| **NUM_9** | |
| **A** | |
| **B** | |
| **C** | |
| **D** | |
| **E** | |
| **F** | |
| **G** | |
| **H** | |
| **I** | |
| **J** | |
| **K** | |
| **L** | |
| **M** | |
| **N** | |
| **O** | |
| **P** | |
| **Q** | |
| **R** | |
| **S** | |
| **T** | |
| **U** | |
| **V** | |
| **W** | |
| **X** | |
| **Y** | |
| **Z** | |
| **OS_COMMAND** | e.g. The Windows key. |
| **CONTEXT_MENU** | |
| **F1** | |
| **F2** | |
| **F3** | |
| **F4** | |
| **F5** | |
| **F6** | |
| **F7** | |
| **F8** | |
| **F9** | |
| **F10** | |
| **F11** | |
| **F12** | |
| **NUM_LOCK** | |
| **SCROLL_LOCK** | |
| **SEMICOLON** | |
| **EQUALS** | |
| **COMMA** | |
| **HYPHEN** | |
| **PERIOD** | |
| **SLASH** | |
| **BACKTICK** | |
| **OPEN_BRAKET** | |
| **BACKSLASH** | |
| **CLOSE_BRACKET** | |
| **APOSTROPHE** | |

