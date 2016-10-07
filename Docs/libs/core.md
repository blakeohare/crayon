# Core Library

Contains various common functions.

Unlike most libraries, Core is implicitly imported in all files.

[Functions](#functions)
- [assert](#assert)
- [chr](#chr)
- [currenttime](#currenttime)
- [fail](#fail)
- [isnumber](#isnumber)
- [isstring](#isstring)
- [ord](#ord)
- [parsefloat](#parsefloat)
- [parseint](#parseint)
- [print](#print)
- [sleep](#sleep)
- [typeof](#typeof)

[Classes](#classes)
- [Object](#object)

[Enums](#enums)
- [Type](#type)

## Functions

### assert

Checks a boolean condition and throws an error if it is not true.

`Core.assert(condition, message = null)`

| Argument | Type | Description |
| -------- | ---- | ----------- |
| **condition** | _boolean_ | A condition to check. If this is true, nothing will happen. If this is false, an error will be thrown. |
| **message** (optional) | _string_ | A mesage to display in the error output if the condition fails. |

See also: [Core.fail](#fail)

### chr

Converts an ASCII code value into a 1-character string.

`Core.chr(value)`

| Argument | Type | Description |
| --- | --- | --- |
| **value** | _integer_ | An integer between 0 and 255 that represents an ASCII value. |

See also: [Core.ord](#ord)

### currentTime

Returns the current time from the operating system as a float indicating number of seconds from the Unix epoch (Jan 1st, 1970).

`Core.currentTime()`

### fail

Generates an error.

`Core.fail(message)`

| Argument | Type | Description |
| --- | --- | --- |
| **message** | _string_ | Error message to display. |

See also: [Core.assert](#assert)

### isNumber

Returns true if the given value is a numeric type i.e. either an integer or float.

`Core.isNumber(value)`

Note that this is roughly equivalent to the following expression (although slightly faster):
`Core.typeof(value) == Core.Type.INTEGER || Core.typeof(value) == Core.Type.FLOAT`

| Argument | Type | Description |
| --- | --- | --- |
| **value** | _anything_ | Any value. True is returned if it is an integer or a float. |

See also: [Core.typeof](#typeof)

### isString

Returns true if the given value is a string.

`Core.isString(value)`

Note that this is equivalent to the following expression:
`Core.typeof(value) == Core.Type.STRING`

| Argument | Type | Description |
| --- | --- | --- |
| **value** | _anything_ | Any value. True is returned when this value is a string. `null` does not count as a string. |

See also: [Core.typeof](#typeof)

### ord

Returns the ASCII character code value for a given character as an integer.

`Core.ord(character)`

| Argument | Type | Description |
| --- | --- | --- |
| **character** | _string_ | A one-character string containing an ASCII character. |

See also: [Core.chr](#chr)

### parseFloat

Returns a float value represented by the given string. If the string does not represent a valid number, then `null` is returned.

`Core.parseFloat(stringValue)`

| Argument | Type | Description |
| --- | --- | --- |
| **stringValue** | _string_ | A string that represents a number. |

See also: [Core.parseInt](#parseint)

### parseInt

Returns an integer value represented by the given string. If the string does not represent a valid integer, then `null` is returned.

`Core.parseInt(stringValue)`

| Argument | Type | Description |
| --- | --- | --- |
| **stringValue** | _string_ | A string that represents an integer. |

See also: [Core.parseFloat](#parsefloat)

### print

Prints a value to STDOUT or its platform equivalent.

`Core.print(value)`

| Argument | Type | Description |
| --- | --- | --- |
| **value** | _anything_ | A value to print. |

Not all platforms support STDOUT.

| Platform | Print Target |
| --- | --- |
| C#/OpenTK | STDOUT |
| Java/AWT | STDOUT |
| Python/PyGame | STDOUT |
| JavaScript | console.log |
| C#/Android | Log Cat |

### sleep

Halts the VM for some number of seconds and then resumes.

`Core.sleep(seconds)`

| Argument | Type | Description |
| --- | --- | --- |
| **seconds** | _float_ | A float representing the number of seconds to pause the current execution. |

### typeof

Returns the primitive type of the given value. The return value is a value in the **[Type](#type)** enum.

`Core.typeof(value)`

| Argument | Type | Description |
| --- | --- | --- |
| **value** | _anything_ | A value to determine the type of. |

See also: [Core.Type](#type), [Core.isNumber](#isnumber), [Core.isString](#isstring)

## Classes

### Object

An empty class that has no methods or fields. Can be used for generating unique references.

## Enums

### Type

An enum of all the primitive types. 

| Value | Description |
| --- | --- |
| NULL | The type of the `null` constant. |
| BOOLEAN | The type of booleans (`true` or `false`). |
| INTEGER | The type of integers. |
| FLOAT | The type of floating point decimals. |
| STRING | The type of strings. |
| LIST | The type of lists. |
| DICTIONARY | The type of dictionaries. |
| OBJECT | The type of instances of classes. Note that specific class types are not distinguished in this enum. See the `is` operator. |
| FUNCTION | A function pointer. |
