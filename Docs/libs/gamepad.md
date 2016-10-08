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

A static method that returns true if Gamepads are supported.

`GamepadManager.isGamepadSupported()`

**Return value**: boolean

```
if (GamepadManager.isGamepadSupported()) {
  ...
}
```

---

### platformRequiresRefresh

A static method that returns true if the current platform does not detect gamepads on startup and must be periodically checked.

Notably this returns true in JavaScript projects, because the browser does not report the existence of a gamepad unless you first push a button after the page has loaded.

Technically this method is not funcetionally necessary as it is safe to call refreshDevices multiple times on any platform. But it does allow for better UX i.e. displaying a message in your gamepad config UI that explicitly tells the user "Please push a button for the gamepad to show up".

`GamepadManager.platformRequiresRefresh()`

**Return value**: boolean

---

### refreshDevices

A static method that checks to see if any new devices are available. This must be called before accessing any Gamepad methods that interact with devices.

`GamepadManager.refreshDevices()`

**Return value**: nothing

See also: [platformRequiresRefresh](#platformrequiresrefresh)

---

### getDeviceCount

A static method that returns the number of devices that are currently available.

`GamepadManager.getDeviceCount()`

**Return value**: integer

---

### getDeviceByIndex

A static method that returns a GamepadDevice instance by providing its index value.

`GamepadManager.getDeviceByIndex(index)`

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

A static method that returns a GamepadDevice instance by providing the ID value previously assigned to it.

`GamepadManager.getDeviceByIndex(id)`

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

Checks to see if there is a previously saved gamepad configuration in UserData. If so, configure as many gamepad devices as possible and assign ID's accordingly. Return the number of gamepads successfully configured.

`GamepadManager.restoreSettingsFromUserData(deviceIdOrIdList)`

| Argument | Type | Description |
| --- | --- | --- |
| **deviceIdOrIdList** | _integer, string, or list of integers/strings_ | ID's to assign to devices. |

**Return value**: integer

See also: [GamepadManager.saveSettingsToUserData](#savesettingstouserdata)

---

### saveSettingsToUserData

Saves the current gamepad configuration to UserData so that it can be re-used the next time this program runs.

`GamepadManager.saveSettingsToUserData()`

**Return value**: nothing

This will overwrite configurations for previous gamepads that have the same hardware fingerprint as those currently configured. But if no gamepads are currently configured, this function will not delete previous configurations.

See also: [GamepadManager.restoreSettingsFromUserData](#restoresettingsfromuserdata)

---

### clearAllIds

Clears all the ID's that are currently configured to GamepadDevices.

`GamepadManager.clearAllIds()`

**Return value**: nothing

---

# Class: GamepadDevice

Represents a physical gamepad device.

## Methods

### getId

Returns the user-assigned ID to this device.

`device.getId()`

**Return value**: integer, string, or `null` if not set

---

### setId

Sets an ID for this device.

`device.setId(id)`

| Argument | Type | Description |
| --- | --- | --- |
| **id** | _integer or string_ | A value that can be used to identify this device. More consistently reliable than simply its device index. |

**Return value**: nothing

---

### clearId

Clears the ID for this device.

`device.clearId()`

**Return value**: nothing

---

### pushAutoConfigure

Pushes a configuration to the GamepadDevice's configuration stack that makes its best guess as to how the buttons ought to be configured.

`device.pushAutoConfigure()`

**Return value**: nothing

Read more about [Configuring Gamepads](#configuring-gamepads)

---

### pushEmptyConfig

Pushes an empty configuration to the GamepadDevice's configuration stack.

`device.pushEmptyConfig()`

**Return value**: nothing

Read more about [Configuring Gamepads](#configuring-gamepads)

---

### popConfig

Pops the active configuration off of the GamepadDevice's configuration stack.

`device.popConfig()`

**Return value**: nothing

Read more about [Configuring Gamepads](#configuring-gamepads)

---

### flattenConfigs

Removes all items in the GamepadDevice's configuration stack other than the active one.

`device.flattenConfigs()`

**Return value**: nothing

Read more about [Configuring Gamepads](#configuring-gamepads)

---

### clearBinding

Removes the binding for the given button ID in the active configuration.

`device.clearBinding(buttonId)`

| Argument | Type | Description |
| --- | --- | --- |
| **buttonId** | _string or integer_ | A button ID previously assigned. |

If the button ID does not exist in the current configuration, nothing will happen and no error is generated.

Read more about [Configuring Gamepads](#configuring-gamepads)

**Return value**: nothing

---

### clearBindings

Removes all bindings in the active configuration.

`device.clearBindings()`

**Return value**: nothing

Read more about [Configuring Gamepads](#configuring-gamepads)

---

### getName

Returns the name of this device as reported by the hardware. This is a product string such as "Lodgy-Tek Stick-o-Joy 5000". 

This value should be shown to the user in any configuration UI to help identify specific devices.

`device.getName()`

**Return value**: string

Note: some platforms are unable to identify the product name and will instead return a description of the device including the number of buttons and axes instead.

---

### getButtonCount

Returns the number of buttons on this device.

`device.getButtonCount()`

**Return value**: integer

This is the number of push-buttons on this device. Axes and directional pads do not count towards this value.

---

### getAxisCount

Returns the number of axes on this device.

`device.getAxisCount()`

**Return value**: integer

This is the number of hardware axes on this device. Note that a single "joystick" axis on a device is generally counted as 2 separate axes. This is because an axis is single-directional that has a positive and negative direction, whereas a joystick axis is 2-dimensional and is treated as two separate axes by the hardware.

---

### getButtonState

Returns the state of a button.

`device.getButtonState(index)`

| Argument | Type | Description |
| --- | --- | --- |
| **index** | _integer_ | The index of the button. This must be between `0` and `n - 1` where `n` is the value returned by `getButtonCount()` |

**Return value**: boolean

...


# Configuring Gamepads

TODO: explain this part
