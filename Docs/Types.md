# Types and Methods

## Integers

Integers are number types that are whole numbers. 

### Integer min/max range
The minimum and maximum value is dependent on the platform the VM is running on. 

* When running in mobile/web or other JavaScript-based platforms, the range of integers is +/- 2<sup>53</sup>.
* When running on native platforms directly, the range is +/- 2<sup>31</sup>.

### Behavior

When performing mathematical operations on integers, the result will always be an integer (with the exception of exponents `**` which produce a float). This holds true even if the result is mathematically incorrect. For example, the expression `7 / 2` will produce `3` instead of `3.5`

### Methods

Integers do not have any built-in methods.

## Floats

The word "float" is short for "floating point numbers". These represent decimal based numbers. It is important to note that floats are their own type and can represent arbitrary numbers, even if the number happens to be equal to a whole number. For example, the number `2.0` is a float, even though it is equal to the integer `2`. 

### Behavior

When performing mathematical operations on floats, the result will always be a float, even if the result is a whole number. For example, `2.5 + 8.5` will produce the float-typed value `11.0` instead of the integer-typed value `11`. 

When integers and floats are mixed in a mathematical operation, the result will always be a float. For example, `4 * 2.5` will result in `10.0`. 

### Methods

Floats do not have any built-in methods.

## Booleans

A boolean is the constant `true` or `false`. Booleans can be generated using the comparison operators `==`, `!=`, `>`, `>=`, `<`, and `<=` and used in `if` and `while` statements. They can also be combined using the `&&` (and) and `||` (or) operators.  

### Methods

Booleans do not have any built-in methods.

## Strings

Strings are a type that represent text. A string can be denoted by surrounding text with either a double quote `"` or a single `'`. There is no difference between these two conventions. 

> While it doesn't make a technical difference, it is recommended that double quotes are used for user-facing text data and single quotes are used for internal data such as dictionary keys, flags, etc. 

### Behavior

Strings are an immutable reference type in Crayon. This means that strings cannot be modified and each string operation will generate a new string. However, because they are still reference types, passing long strings around in code is still a O(1) operation.

### String Length

You can find the length of the string by calling the property `.length` on any string value. For example, if you have the string `"Hello, world!"` stored in the variable `message`, the expression `message.length` would return the integer value `13`. Length is internally stored directly on string values and so accessing the length of a string is a O(1) operation (as opposed to null terminator internal formats). 

### String Indexing

You can access individual characters in a string by using square brackets `[` and `]` and passing in an integer. The integer is the offset from the beginning of the string (0-indexed). For example, if you you have a string in the variable `foo` and you wanted to get the first character of the string, you would use `foo[0]`. If you wanted to see the 5th character of `foo`, you would use `foo[4]` (because these are 0-indexed offsets from the beginning, 4 is the 5th character since 0 is the 1st character`). 

The character value itself is also a string. The length of the string will be 1. There is no special character type. 

In addition to positive integers, you can also use negative integers as string indexes. A negative string index counts backwards from the end of the list (in a 1-indexed fashion). For example, `foo[-1]` is the last character in the string. Basically if you add the length of the string to the negative number, you will get its equivalent positive index.

If you access a number greater tha or equal to the length or less than the negative length, an `IndexOutOfRangeException` will be thrown.

### String Slicing

You can access a section of a string to create a new smaller string by using string slicing. This is similar to substring in most other languages. 

String slicing uses square brackets `[` and `]` just like indexing, but uses two integers separated by a colon `:` instead of one integer. These indicate the start and end points of the string that you want to get a slice of.

For example, suppose you have the string `alphabet = "abcdefghijklmnopqrstuvwxyz"` which has a length of 26. You can get the first half of the string by using `firstHalf = alphabet[0:13]`. The first number is the starting inclusive index and the second number is the ending exclusive index. Since we want all the characters up to the 13th index (but not including the 13th index), we use 13. This will return a new string `"abcdefghijklm"`. 

All list slicing operations create new strings and do not affect the original string. Remember, strings are immutable types. 

The above operation can be shortened to `firstHalf = alphabet[:13]`. When the slice starts at the beginning of the string, the first number can be omitted and `0` will be assumed. Likewise, if the slice goes through to the end of the string, the 2nd number can be ommitted and the string length will be assummed. For example, the second half of the alphabet can be created with the following expression: `lastHalf = alphabet[13:]`

In addition to positive numbers, negative numbers are supported as well. Negative numbers work the same way they when they're used as indexes. For example, if you wanted to get the last 4 characters of a string, you could do so by `myString[-4:]`. In this code, the starting index is -4 which is 4 characters from the end.

It is okay to go out of bounds when using slicing. If your indexes go out of bounds, the slice value will just go as far as it possibly can before it runs off the end. If the start and endpoint are out of bounds in the same direction, it will generate an empty string.

String slices also support a 3rd argument for the step value, but these are not common for strings. For more information about how this behaves, read the equivalent documentation on list slicing, below.

### String Methods

String values have many built-in methods that can be used. to accomplish common tasks

| **Method** | **Explanation** |
| --- | --- |
| `value.contains(searchString)` | Returns a boolean for whether `value` contains the given `searchString`. This method is case sensitive. |
| `value.endsWith(searchString)` | Returns a boolean for whether `value` ends with the given `searchString`. This method is case sensitive. |
| `value.indexOf(searchString)` | Returns an integer, indicating the index of the location of the first character of `searchString`, if it appears in `value`. If `searchString` does NOT appear in `value`, then `-1` is returned. |
| `value.lower()` | Returns an all-lowercase version of `value`. |
| `value.ltrim()` | Returns a new string that has all the whitespace characters removed from the beginning of the original value. |
| `value.replace(searchString, newValue)` | Replaces all instances of `searchString` in `value` with `newValue` and returns a new string. |
| `value.reverse()` | Generates and returns a new string that is the old string, but backwards. |
| `value.rtrim()` | Returns a new string that has all the whitespace characters removed from the end of the original value. |
| `value.split(separator)` | Splits the string into a list of strings, making cuts anywhere the `separator` value is encountered. The `separator` value is not included in the list of string values. For example `words.split(" ")` will create a list of word strings by splitting on each space character. |
| `value.trim()` | Returns a new string that has all the whitespace characters removed from the beginning and end of the original value. |
| `value.upper()` | Returns an all-uppercase version of `value`. |


### String Internal Encoding

Strings are currently stored in UTF-16, however, **this is planned to change to unicode-codepoint-list format sometime around Crayon 3**. 

This generally should not affect most use cases, however this subtly can be observed in certain situations using surrogate pair characters, such as when emojis are assumed to be a single character:

```
message = "Greetings 🐱 kitty!";
catIndex = msg.indexOf("🐱");
removedEmojiMessage = message[:catIndex] + message[catIndex + 1:];
print(removedEmojiMessage); // "Greetings ? kitty!"
```

## Lists

Lists are linear collections of values and are created by using square brackets `[` and `]`.

### Creating Lists

To create a list, surround the values you want store in the list with square brackets and separate each item with a comma:

`fruits = ["apple", "banana", "canteloupe", "durian"];`

### Length and Indexing

Lists are similar to strings in many ways. For example, if you want to know the length of the list, you can use the `.length` property to get an integer. For example, `fruits.length` will return `4`.

Indexing is also similar to strings. To access an item in a string, you pass the item's index as an integer into square brackets after the list value.

`fruits[2]` would return `"canteloupe"`. Like strings, indexes are 0-based. 

> Negative indexes are also supported. See the section on indexes for strings for more information.

### Slicing

Like strings, lists also support slicing to create new lists that are sub arrays of the original list. 

When you use slicing, you are creating a new list with references the original values in the list, but the original list is unaffected. 


## Dictionaries

## Null

## Classes

## Functions

## Object Instances


