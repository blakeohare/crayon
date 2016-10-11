# Gamepad Library

Library for interacting with USB Gamepads.

The Gamepad library is currently supported for the following project types:
- C#/OpenTK
- JavaScript
- Python/PyGame

Projects that use the Gamepad library can still be safely exported to other project types. However, the number of gamepads detected will always be 0. 

Note that JavaScript projects may or may not support the Gamepad library depending on browser.

# Class: GamepadManager

The GamepadManager is a static class with various utility methods for interacting with Gamepads.

## Methods

### isGamepadSupported

`GamepadManager.isGamepadSupported()`

A static method that returns true if Gamepads are supported.

**Return value**: boolean

```
if (GamepadManager.isGamepadSupported()) {
  ...
}
```

---

### platformRequiresRefresh

`GamepadManager.platformRequiresRefresh()`

A static method that returns true if the current platform does not detect gamepads on startup and must be periodically checked.

Notably this returns true in JavaScript projects, because the browser does not report the existence of a gamepad unless you first push a button after the page has loaded.

Technically this method is not funcetionally necessary as it is safe to call refreshDevices multiple times on any platform. But it does allow for better UX i.e. displaying a message in your gamepad config UI that explicitly tells the user "Please push a button for the gamepad to show up".

**Return value**: boolean

---

### refreshDevices

`GamepadManager.refreshDevices()`

A static method that checks to see if any new devices are available. This must be called before accessing any Gamepad methods that interact with devices.

**Return value**: nothing

See also: [GamepadManager.platformRequiresRefresh](#platformrequiresrefresh)

---

### getDeviceCount

`GamepadManager.getDeviceCount()`

A static method that returns the number of devices that are currently available.

**Return value**: integer

---

### getDeviceByIndex

`GamepadManager.getDeviceByIndex(index)`

A static method that returns a GamepadDevice instance by providing its index value.

| Argument | Type | Description |
| --- | --- | --- |
| **index** | _integer_ | The index of the gamepad device as an index value from `0` to `n - 1` where `n` is the number of gamepads as reported by `getDeviceCount()`. |

**Return value**: a GamepadDevice instance

```
Core.assert(GamepadManager.getDeviceCount() >= 2, "2 Gamepads are required");
p1input = GamepadManager.getDeviceByIndex(0);
p2input = GamepadManager.getDeviceByIdnex(1);
```

See also: [GamepadManager.getDeviceCount](#getdevicecount), [GamepadDevice](#class-gamepaddevice)

---

### getDeviceById

`GamepadManager.getDeviceByIndex(id)`

A static method that returns a GamepadDevice instance by providing the ID value previously assigned to it.

| Argument | Type | Description |
| --- | --- | --- |
| **id** | _string or integer_ | The ID that has been assigned to this device. |

**Return value**: a GamepadDevice instance

ID's can either be a string or integer and are assigned to devices in a few possible ways.
- The ID was assigned by loading the previous config file. 
- The ID was assigned to the GamepadDevice via auto-configuring.
- The ID was manually assigned to a GamepadDevice instance.

```
p1input = GamepadManager.getDeviceByIndex("Player1");
p2input = GamepadManager.getDeviceByIdnex("Player2");
```

See also: [GamepadDevice](#class-gamepaddevice)

---

### restoreSettingsFromUserData

`GamepadManager.restoreSettingsFromUserData(deviceIdOrIdList)`

Checks to see if there is a previously saved gamepad configuration in UserData. If so, configure as many gamepad devices as possible and assign ID's accordingly. Return the number of gamepads successfully configured.

| Argument | Type | Description |
| --- | --- | --- |
| **deviceIdOrIdList** | _integer, string, or list of integers/strings_ | ID's to assign to devices. |

**Return value**: integer

See also: [GamepadManager.saveSettingsToUserData](#savesettingstouserdata)

---

### saveSettingsToUserData

`GamepadManager.saveSettingsToUserData()`

Saves the current gamepad configuration to UserData so that it can be re-used the next time this program runs.

**Return value**: nothing

This will overwrite configurations for previous gamepads that have the same hardware fingerprint as those currently configured. But if no gamepads are currently configured, this function will not delete previous configurations.

See also: [GamepadManager.restoreSettingsFromUserData](#restoresettingsfromuserdata)

---

### clearAllIds

`GamepadManager.clearAllIds()`

Clears all the ID's that are currently configured to GamepadDevices.

**Return value**: nothing

---

# Class: GamepadDevice

Represents a physical gamepad device.

## Methods

### getId

`device.getId()`

Returns the user-assigned ID to this device.

**Return value**: integer, string, or `null` if not set

---

### setId

`device.setId(id)`

Sets an ID for this device.

| Argument | Type | Description |
| --- | --- | --- |
| **id** | _integer or string_ | A value that can be used to identify this device. More consistently reliable than simply its device index. |

**Return value**: nothing

---

### clearId

`device.clearId()`

Clears the ID for this device.

**Return value**: nothing

---

### pushAutoConfigure

`device.pushAutoConfigure()`

Pushes a configuration to the GamepadDevice's configuration stack that makes its best guess as to how the buttons ought to be configured.

**Return value**: nothing

Read more about [Configuring Gamepads](#configuring-gamepads)

---

### pushEmptyConfig

`device.pushEmptyConfig()`

Pushes an empty configuration to the GamepadDevice's configuration stack.

**Return value**: nothing

Read more about [Configuring Gamepads](#configuring-gamepads)

---

### popConfig

`device.popConfig()`

Pops the active configuration off of the GamepadDevice's configuration stack.

**Return value**: nothing

Read more about [Configuring Gamepads](#configuring-gamepads)

---

### flattenConfigs

`device.flattenConfigs()`

Removes all items in the GamepadDevice's configuration stack other than the active one.

**Return value**: nothing

Read more about [Configuring Gamepads](#configuring-gamepads)

---

### clearBinding

`device.clearBinding(buttonId)`

Removes the binding for the given button ID in the active configuration.

| Argument | Type | Description |
| --- | --- | --- |
| **buttonId** | _string or integer_ | A button ID previously assigned. |

If the button ID does not exist in the current configuration, nothing will happen and no error is generated.

Read more about [Configuring Gamepads](#configuring-gamepads)

**Return value**: nothing

---

### clearBindings

`device.clearBindings()`

Removes all bindings in the active configuration.

**Return value**: nothing

Read more about [Configuring Gamepads](#configuring-gamepads)

---

### getName

`device.getName()`

Returns the name of this device as reported by the hardware. This is a product string such as "Lodgy-Tek Stick-o-Joy 5000". 

This value should be shown to the user in any configuration UI to help identify specific devices.

**Return value**: string

Note: some platforms are unable to identify the product name and will instead return a description of the device including the number of buttons and axes instead.

---

### getButtonCount

`device.getButtonCount()`

Returns the number of buttons on this device.

**Return value**: integer

This is the number of push-buttons on this device. Axes and directional pads do not count towards this value.

---

### getAxisCount

`device.getAxisCount()`

Returns the number of axes on this device.

**Return value**: integer

This is the number of hardware axes on this device. Note that a single "joystick" axis on a device is generally counted as 2 separate axes. This is because an axis is single-directional that has a positive and negative direction, whereas a joystick axis is 2-dimensional and is treated as two separate axes by the hardware.

---

### getButtonState

`device.getButtonState(index)`

Returns the state of a button.

| Argument | Type | Description |
| --- | --- | --- |
| **index** | _integer_ | The index of the button. This must be between `0` and `n - 1` where `n` is the value returned by `getButtonCount()` |

**Return value**: boolean

---

### getAxisState

`device.getAxisState(index)`

Returns the state of an axis.

| Argument | Type | Description |
| --- | --- | --- |
| **index** | _integer_ | The index of the axis. This must be between `0` and `n - 1` where `n` is the value returned by `getAxisCount()` |

**Return value**: float

---

### getCurrentState

`device.getCurrentState(buttonId)`

Returns the state of the given configured button.

| Argument | Type | Description |
| --- | --- | --- |
| ***buttonId*** | _integer or string_ | The button ID of a button or 1D or 2D axis that has been configured. |

**Return value**: Boolean, float, integer, or 2D vector of floats or ints. 

| Button is configured to... | Return value |
| --- | --- | --- |
| Digital button | Boolean |
| Analog button | Float from 0.0 to 1.0 |
| Digital 1D Axis | Integer from -1 to 1 |
| Analog 1D Axis | Float from -1.0 to 1.0 |
| Digital 2D Axis | List of 2 integers that range from -1 to 1 |
| Analog 2D Axis | List of 2 floats that range from -1.0 to 1.0 |

---

### manualBindButton

***WARNING:*** This method was only meant to be used as an internal API. It's not pretty and will likely change before release.

`device.manualBindButton(buttonId, isDigital, listOfHardwareSources)`

Manually binds a buttonId to a specific hardware source or sources.

The list of hardware sources must have 1, 2, or 4 sources which correspond to a button, 1D axis, or 2D axis.

| Source count | Format |
| --- | --- |
| 1 | Source represents the button. |
| 2 | Sources represent the negative and positive axis respectively. |
| 4 | Sources represent the negative X axis, positive X axis, negative Y axis, and positive Y axis |

A source itself is a list of values. If the source refers to an actual hardware button, it will have two items:

```
source = ['b', buttonIndex];
```

If a source refers to a hardware axis, it will have 3 items:

```
source = ['a', axisIndex, isPositive]
```


---

### bindDigitalButton

`device.bindDigitalButton(buttonId)`

Binds any actively pressed button or axis that is not already bound in the active configuration to the button ID as a digital button.
A digital button returns a boolean when queried. 

- If binding is successful, `true` is returned.
- If a button or axis is pressed, but is already associated with another button ID, nothing will be bound and `false` is returned.
- If no buttons or axes are pressed, `false` is returned.

| Argument | Type | Description |
| --- | --- | --- |
| **buttonId** | _string or integer_ | Button ID to bind. |

See also: [GamepadDevice.bindAnalogButton](#bindanalogbutton)

Read more about [Configuring Gamepads](#configuring-gamepads)

---

### bindAnalogButton

`device.bindAnalogButton(buttonId)`

Binds any actively pressed button or axis that is not already bound in the active configuration to the button ID as an analog button.
An analog button returns a float between 0.0 and 1.0 when queried. 

- If binding is successful, `true` is returned.
- If a button or axis is pressed, but is already associated with another button ID, nothing will be bound and `false` is returned.
- If no buttons or axes are pressed, `false` is returned.

| Argument | Type | Description |
| --- | --- | --- |
| **buttonId** | _string or integer_ | Button ID to bind. |

See also: [GamepadDevice.bindDigitalButton](#binddigitalbutton)

Read more about [Configuring Gamepads](#configuring-gamepads)

---


# Configuring Gamepads

Gamepads are tricky. They might have a left and right joystick, bumpers at the top, and buttons labelled as A, B, X, Y, or have shapes on them. However, all this is cosmetic plastic. From the hardware perspective, essentially only a few pieces of data are available...

- The number of buttons
- The number of axes
- The pushed state of any button at any given moment
- The position of any axis at any given moment
- Sometimes the name of the device is reported by the platform

The buttons and axes are given a numbered order. The names of these buttons and axes are not known on a hardware level. There are no standards as to what order buttons should appear in and it is entirely up to the hardware manufacturer. Generally they appear in a certain order but this order is not guaranteed. Especially with older or uniquely shaped devices.

The Crayon Gamepad library provides methods to access any gamepad device that is currently connected. Furthermore you can access any button and axis and poll its current value. This isn't particularly helpful on its own since nothing has context.
Configuring the gamepad is all about assigning friendly ID's to gamepads and their individual buttons and axes. 

## Gamepad IDs

Each GamepadDevice has an ID that can be accessed or set. This ID is used in various places. You can get a GamepadDevice reference from the GamepadManager by its ID. Also gamepad events that come in through the event queue are tagged with the ID of the device it came from.

## Configuration IDs and Pushy Types

Each Gamepad can have a mapping of friendly ID's to buttons/axes. Furthermore, and ID can map to a set of buttons or axes that behave like a single button. For example, consider a 2D joystick. This is comprised of two separate single-dimensional axes. However, logically, you will likely want to treat this as a single button that returns a two dimensional vector when queried. Further consider a retro-style platformer where the the perceived controller is a 4-directional pad that does not take into consideration how far a joystick is pushed in a direction. Just simply that it is "pushed in that direction or isn't". In that case, the button ID should be mapped to 2 axes, but return a 2 dimensional vector of integers where each dimension is one of { -1, 0, 1 }. 
All of these scenarios are supported.

Imagine an NES style game that is configured using a XBox 360 style controller. The configuration mapping of buttons to phsyical controls may look something like this...

| Physical Button/Axis | Button ID |
| --- | --- |
| Button 0 | 'A' |
| Button 1 | |
| Button 2 | 'B' |
| Button 3 | | 
| Button 4 | | 
| Button 5 | | 
| Button 6 | | 
| Button 7 | | 
| Button 8 | | 
| Button 9 | 'START' | 
| Button 10 | 'SELECT' | 
| Axis 1- | 'DIRECTION' (X-) |
| Axis 1+ | 'DIRECTION' (X+) |
| Axis 2- | 'DIRECTION' (Y-) |
| Axis 2+ | 'DIRECTION' (Y+) |
| Axis 3- | |
| Axis 3+ | |
| Axis 4- | |
| Axis 4+ | |
| Axis 5- | |
| Axis 5+ | |
| Axis 6- | |
| Axis 6+ | |
| Axis 7- | |
| Axis 7+ | |

Alternatively, suppose you plug in a very old controller that only has hardware buttons. For exmaple, it may actually be a gamepad modelled after the original NES controller and not report any axes at all (even though the D-Pad looks like an axis, it is simply digital buttons). Then the configuration may look something like this...

| Physical Button/Axis | Button ID |
| --- | --- |
| Button 0 | 'DIRECTION' (X-) |
| Button 1 | 'DIRECTION' (Y-) |
| Button 2 | 'DIRECTION' (X+) |
| Button 3 | 'DIRECTION' (Y+) |
| Button 4 | 'B' |
| Button 5 | 'A' |
| Button 6 | 'START' |
| Button 7 | 'SELECT' |

By configuring these two very different devices, the game code can agnostically interpret their usage identically as it can now query ID's instead of having knowledge of the specs of the device.

```
vector = device.getCurrentState('DIRECTION') // e.g. [-1, 1] <-- user is pushing down+left
jump = device.getCurrentState('A') // e.g. true <-- user is pushing the jump button
shoot = device.getCurrentState('B') // e.g. false <-- user is not pushing the shoot button
```

There are a few ways to configure logical button IDs to their hardware counterparts. One is to ask the user specifically using a configuration menu. 

There are several binding methods available on the GamepadDevice instance to do this. The following functions will check to see if any buttons or axes are currently being pushed that have no pre-existing configuration and will configure it to the given inputs. 
- bindDigitalButton(buttonId)
- bindDigitalAxis(buttonId, isPositive)
- bindDigitalAxis2dX(buttonId, isPositive)
- bindDigitalAxis2dY(buttonId, isPositive)
- bindAnalogButton(buttonId)
- bindAnalogAxis(buttonId, isPositive)
- bindAnalogAxis2dX(buttonId, isPositive)
- bindAnalogAxis2dY(buttonId, isPositive)

If configuration is successful, then true is returned, otherwise false. These are intended to be shown on a screen that says something like "push the button you want to use for firing your laser". The code would look something like this...

```
// somewhere in your game loop...
if (device.bindDigitalButton('laser')) {
  // move on to next button to configure
} else {
  // ensure 'laser' is still checked next frame
}
```
In the above example, the string 'laser' will be used as the button ID anytime the player pushes that button again. 

Note that these binding methods are not blocking. They immediately return true or false depending on if a key is being pushed. 

### Analog vs Digital bindings

Any analog binding indicates that that button/axis should report its value as a float. Even if the hardware is actually a digital button, the value returned will still be a float (either 0.0 or 1.0). Digital bindings will report their values as booleans (or integers in the case of 1D and 2D vectors).

### Button

A button is a thing on the device you can push. It does not have a direction. Generally buttons will be bound as digital buttons. When you query the value, it will be a boolean. Analog buttons are commonly used as triggers because they are used like buttons but can have an "inbetween" value where they are partially pressed. An analog button reports its value as a number between 0.0 and 1.0.

### Axis

An axis is a thing on the device that can be pushed in a single dimension in either direction. When bound as an analog axis, its value will be reported as a float between -1.0 and 1.0. When bound as a digital axis, its value will be reported as one of the integers: -1, 0, or 1. 

### 2D Axis

A 2-dimensional axis bound to a single button ID will report its value as a vector. This vector will return the X and Y components. 
An analog 2D axis will report its value as a list of two floats between -1.0 and 1.0. A digital 2D axis will report its value as a list of 2 integers (between -1 and 1).

### Gamepad Configuration Stacks

A gamepad has a configuration mapping of button ID's to its buttons and axes. However, it saves previous configurations as well in a stack where the topmost configuration in the stack is treated as the active configuration that is polled and configured. The purpose for this is make non-destructive changes with a way to return to a previous configuration. For example if you create a gamepad configuration menu where the user starts to configure a gamepad using the binding methods, a new empty configuration should be pushed to the device's stack. If the user finishes configuring the device, that configuration is now active. But if the user decides they don't want to re-configure the device and exits out, popping the stack will return the gamepad to its previous state.

### Auto Configuring

The Gamepad library comes with several enums of common gamepad layouts (XBox, PlayStation, SNES). All 3 of these enums have values that map to a common set of integers. You can push an "auto-configure" configuration to the stack using the device's `pushAutoConfiguration()` method. This will make the best guess possible as to the layout of the device. It might be wrong. But hey, we tried our best. You can then use any of the built in Button ID enums to reference buttons and axes. 
