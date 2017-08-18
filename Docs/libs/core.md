# Core Library

Contains various common functions.

Unlike most libraries, Core is implicitly imported in all files.

[Functions](#functions)
- [assert](#assert)
- [chr](#chr)
- [currentTime](#currenttime)
- [fail](#fail)
- [isNumber](#isnumber)
- [isString](#isstring)
- [ord](#ord)
- [parseFloat](#parsefloat)
- [parseInt](#parseint)
- [print](#print)
- [sleep](#sleep)
- [typeof](#typeof)

[Enums](#enums)
- [Type](#type)

## Functions

### assert

Checks a boolean condition and throws an error if it is not true, ending program execution.

`Core.assert(condition, message = null)`

| Argument | Type | Description |
| -------- | ---- | ----------- |
| **condition** | _boolean_ | A condition to check. If this is true, nothing will happen. If this is false, an error will be thrown. |
| **message** (optional) | _string_ | A message to display in the error output if the condition fails. |

**Return value**: nothing

```
assert(value > 0, "Only positive numbers are allowed.");
```

See also: [Core.fail](#fail)

---

### chr

Converts an ASCII code value into a 1-character string.

`Core.chr(value)`

| Argument | Type | Description |
| --- | --- | --- |
| **value** | _integer_ | An integer between 0 and 255 that represents an ASCII value. |

**Return value**: string (1 character)

```
value = chr(97); // value is 'a'
value = chr(64); // value is '@'
```

See also: [Core.ord](#ord)

---

### currentTime

Returns the current time from the operating system as a float indicating number of seconds from the Unix epoch (Jan 1st, 1970).

`Core.currentTime()`

**Return value**: float

```
now = currentTime(); // value is 1475899688.35710588
```

---

### fail

Generates an error with the given message, ending program execution.

`Core.fail(message)`

| Argument | Type | Description |
| --- | --- | --- |
| **message** | _string_ | Error message to display. |

**Return value**: nothing

```
if (value % 2 == 0) {
  fail("value should have been odd, not event.");
}
```

See also: [Core.assert](#assert)

---

### isNumber

Returns true if the given value is a numeric type i.e. either an integer or float.

`Core.isNumber(value)`

Note that this is roughly equivalent to the following expression (although slightly faster):
`Core.typeof(value) == Core.Type.INTEGER || Core.typeof(value) == Core.Type.FLOAT`

| Argument | Type | Description |
| --- | --- | --- |
| **value** | _anything_ | Any value. True is returned if it is an integer or a float. |

**Return value**: boolean

```
isNumber(42); // true
isNumber(3.14159); // true
isNumber(null); // false
isNumber("151"); // false, this is a string
```

See also: [Core.typeof](#typeof)

---

### isString

Returns true if the given value is a string.

`Core.isString(value)`

Note that this is equivalent to the following expression:
`Core.typeof(value) == Core.Type.STRING`

| Argument | Type | Description |
| --- | --- | --- |
| **value** | _anything_ | Any value. True is returned when this value is a string. `null` does not count as a string. |

**Return value**: boolean

```
isString("abc"); // true
isString('abc'); // true
isString(''); // true
isString(null); // false
isString(123); // false
isString(['a', 'b', 'c']); // false
```

See also: [Core.typeof](#typeof)

---

### ord

Returns the ASCII character code value for a given character as an integer.

`Core.ord(character)`

| Argument | Type | Description |
| --- | --- | --- |
| **character** | _string_ | A one-character string containing an ASCII character. |

**Return value**: integer

```
ord('a'); // 97
ord('A'); // 65
ord(" "); // 32
```

See also: [Core.chr](#chr)

---

### parseFloat

Returns a float value represented by the given string. If the string does not represent a valid number, then `null` is returned.

`Core.parseFloat(stringValue)`

| Argument | Type | Description |
| --- | --- | --- |
| **stringValue** | _string_ | A string that represents a number. |

**Return value**: float or `null`

```
parseFloat("3.14159"); // 3.14159
parseFloat("pi"); // null
```

See also: [Core.parseInt](#parseint)

---

### parseInt

Returns an integer value represented by the given string. If the string does not represent a valid integer, then `null` is returned.

`Core.parseInt(stringValue)`

| Argument | Type | Description |
| --- | --- | --- |
| **stringValue** | _string_ | A string that represents an integer. |

**Return value**: integer or `null`

```
parseInt("12345"); // 12345
parseInt("a baker's dozen"); // null
```

See also: [Core.parseFloat](#parsefloat)

---

### print

Prints a value to STDOUT or its platform equivalent.

`Core.print(value)`

| Argument | Type | Description |
| --- | --- | --- |
| **value** | _anything_ | A value to print. |

**Return value**: nothing

```
print("Hello, World!");
print(x); // prints the value of x
```

Not all platforms support STDOUT. The following table shows the form of the output by platform type.

| Platform | Print Target |
| --- | --- |
| C#/OpenTK | STDOUT |
| Java/AWT | STDOUT |
| Python/PyGame | STDOUT |
| JavaScript | console.log |
| C#/Android | Log Cat |

---

### sleep

Halts the VM for some number of seconds and then resumes.

`Core.sleep(seconds)`

| Argument | Type | Description |
| --- | --- | --- |
| **seconds** | _float_ | A float representing the number of seconds to pause the current execution. |

**Return value**: nothing

```
for (i = 10; i >= 1; i--) {
  sleep(1);
  print(i + " Mississippi");
}
```

---

### typeof

Returns the primitive type of the given value. The return value is a value in the **[Type](#type)** enum.

`Core.typeof(value)`

| Argument | Type | Description |
| --- | --- | --- |
| **value** | _anything_ | A value to determine the type of. |

**Return value**: Type enum value (integer)

```
function isEven(number) {
  assert(typeof(number) == Type.INTEGER, "isEven must be given an integer.");
  return number % 2 == 0;
}
```

See also: [Core.Type](#type), [Core.isNumber](#isnumber), [Core.isString](#isstring)

---

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
