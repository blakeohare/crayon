# Types and Methods

In Crayon, there are base-level types. There are exactly 10 different base level types. They are:

* Null
* Boolean
* Integer
* Float
* String
* List
* Dictionary
* Object Instances
* Functions
* Class Definitions

Some of the base level types have sub-types. For example, an object instance has its own type system. However that type system is contained within object instances itself. You cannot, for example, create a class that extends from List or Function. The object instance type system is the only type system that can have uesr-defined types.

Enums are notably missing from this list. An enum is a compile-time concept and are converted into integers during compilation.

Functions also have sub-types but these are fixed at only 5 subtypes:
* Standalone functions
* Lambdas
* Instance methods
* Static methods
* Primitive type methods

Given a value, you can check what top-level type something is by calling the `typeof(value)` function. This function will return an enum value from `Core.Type`. For example:

```
if (typeof(unknownValue) == Type.LIST) {
    // loop through the list if we know it's a list...
    for (item : unknownValue) {
        print(item);
    }
}
```

This document is the documentation of the behavior and features of these types including the built-in methods that can be used with them.

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

An optional comma after the final element in the list is allowed.

`items = [1, 2, 3,];`

This is to allow for streamlined code, particularly when listing items on individual lines:

```
items = [
    "fork",
    "spoon",
    "knife",
];
```

### Length and Indexing

Lists are similar to strings in many ways. For example, if you want to know the length of the list, you can use the `.length` property to get an integer. For example, `fruits.length` will return `4`.

Indexing is also similar to strings. To access an item in a string, you pass the item's index as an integer into square brackets after the list value.

`fruits[2]` would return `"canteloupe"`. Like strings, indexes are 0-based.

> Negative indexes are also supported. See the section on indexes for strings for more information.

### Slicing

Like strings, lists also support slicing to create new lists that are sub arrays of the original list.

When you use slicing, you are creating a new list with references the original values in the list, but the original list is unaffected.

Creating a slice of the first 5 elements of a list is essentially the same as doing this:

```
let newList = [];
for (i = 0; i < 5; i++) {
    newList.add(oldList[i]);
}
```

In addition to the start and end index, you can also pass a third integer into the slice arguments that represents the step amount.

For example, `myList[0:10:2]` will generate a new list that contains *every other* item from the original list up until the 10th index of the original list.

```
letters = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'];
someLetters = letters[0:10:2]; // a, c, e, g, i
```

If the step amount is negative, the slice will be backwards. In this case, the starting index must be higher than the end index.

```
nums = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
countdown = [9:-1:-3]; // 10, 7, 4, 1
```
Remember that the last index is not included, and so to have a backwards slice that goes all the way to the beginning, the end index must be -1. An easy way to get around this problem is to just omit parameters.

One common pattern to get a backwards list isi to use the slice `[::-1]` to create a copy of a list that starts from the end and goes to the beginning.

```
alphabetBackwards = letters[::-1];
```

### List Methods

| **Method** | **Explanation** | **Time Complexity** |
| --- | --- | -- |
| `list.add(item, ...)` | Adds 1 or more items to `list`. Return value is `null`. | `O(1)` amortized, Worst case is `O(n)` when new capacity is added to the list. |
| `list.choice()` | Returns a random item from the list. If the length of the list is `0`, throws an `UnsupportedOperationException`. | `O(1)` |
| `list.clear()` | Removes all items from the list. | `O(1)`, but `O(n)` deferred to GC |
| `list.clone()` | Creates a shallow copy of the list. | `O(n)` |
| `list.concat(otherList)` | Adds all items from `otherList` to the end of `list`. This mutates the original list. For a new list that is the concatenation of two lists, you can use the `+` operator between two lists to create a new combined list. | `O(otherList.length)`  amortized, Worst case is `O(n) + O(otherList.length)` when new capacity is added to the list.|
| `list.contains(item)` | Returns a boolean indicating whether the given item is present in the list. The condition of presence uses the same rules as `==`. | `O(n)` worst case. `O(1)` when the items are guaranteed to be at or near the beginning of the list. |
| `list.filter(func)` | Takes in a function `func` as an argument. This function must take in exactly 1 argument and return a boolean. Each item in the list will be passed in to `func` and the items that cause it to return `true` will be preserved in the new list. Items that return `false` will be discarded. | `O(n) * O(func)` |
| `list.insert(index, value)` | Inserts an item into the list such that it will have the given index. This will push any other items that come after it back by 1 index. If the index is out of range, an `IndexOutOfRangeException` will be thrown. Negative indexes are allowed as per the wrap-around rules. | `O(n)` worst/average case. `O(1)` when inertions occur at or near the end of the list. |
| `list.join(seperator)` | Creates a new string that concatenates all the items in the list as strings, inserting the `separator` string between each element. To join a list together as a string with no sepatator, use the empty string `""` as the sepator. | `O(finalStringLength)` |
| `list.map(func)` | Takes in a function `func` that takes in exactly one argument. `func` will be called on each item of `list` and a new list will be generated consisting of the return values from `func`. This new list is returned by `map`. | `O(n) * O(func)` |
| `list.pop()` | Removes the last item from the list and returns it. | `O(1)` |
| `list.reduce(func, accumulator[optional])` | Takes in a function `func` that takes in two arguments. Each item in the list will be passed into this function in pairs starting with the first two items. The result of the function will be passed into the next invocation of `func` as the first argument (this is called the accumulator) and the next item in the list will be passed in as the 2nd argument. This will continue until all items in the list have been passed into `func` in this fashion. Optionally, you can provide a starting accumulator value that will be used instead of the first item of the list. If no starting `accumulator` is provided, there must be at least one item in the list. This item will be returned as the final value. Otherwise, an `InvalidArgumentException` will be thrown for empty lists. | `O(n) * O(func)` |
| `list.remove(index)` | Removes the item from the list at the given `index`. This modifies the original list and returns the item that was removed. | `O(n)` generally, `O(1)` if performed at or near end of list |
| `list.reverse()` | Reverses the list. This modifies the original list and returns `null`. If you want to create a reversed copy, use list slicing instead e.g. `list[::-1]`. | `O(n)` |
| `list.shuffle()` | Randomly shuffles the list. This modifies the original list and returns `null`. | `O(n)` |
| `list.sort(func[optional])` | If `sort` is called without any arguments, all numbers or strings in the list will be sorted. Numbers will be sorted in increasing order and strings will be sorted in character code order. The list MUST only contain homogenous types i.e. the list can ONLY contain strings or the list can ONLY contain numbers. When a `func` value is provided, this is a key-based sort. The function must take exactly one argument. Each item in the list will be passed to the function and the list will be sorted by the return value of this function, which must be either a string or number (but not mixed). | `O(n log n)` or `O(n) * O(func) + O(n log n)` (key is generated once per item) |

## Dictionaries

A dictionary is a collection that stores key-value-pairs in an unordered fashion. A dictionary can hold any value. However the keys of a dictionary must always be a string, integer, or object instance. Dictionary keys cannot be mixed in the same dictionary. For example, you can't add a value to a dictionary with a key of `42` and then add another value to a dictionary with a key of `"x"`.

To create a dictionary, use curly braces.

`emptyDictionary = {};`

To create initial values in the dictionary, list them inside the curly braces, separated by commas. Each key and value should be separated by a colon `:`.

```
dictionaryWithValues = {
  'a': 1,
  'b': 2,
  'c': "three", // values can be mixed type but keys cannot.
};
```

To access a value in a dictionary, follow the dictionary with square brackets with the key's value inside e.g. `dictionaryWithValues['b']`. If the key does not exist, a `KeyNotFoundException` is thrown. To safely get a value at a key that you don't know for sure exists, use the `dictionary.get(key, fallback)` method instead or use the `dictionary.contains(key)` method.

Setting a value in a dictionary is performed in a similar way. Use the assignment operator `=` with the target dictionary key on the left.

`dictionaryWithValues['d'] = "four";`

Setting a value at a key that already exists is also okay. The old value will be overwritten with the new value.

Setting/adding and getting by a key in a dictionary are both `O(1)` operations. However, the dictionary must internally allocate space occasionally if it grows too much and so adding a value is amortized `O(1)`, but incurs `O(n)` complexity in the worst case of an individual addition.

### Getting the size of a dictionary

Like strings and lists, you can use the `.length` property on dictionaries to get the total number of items in the dictionary. This property is a `O(1)` operation.

### Iterating Through Dictionaries

A dictionary cannot be used with a `for` loop directly. However, you can get a list of keys or a list of values by using the `dictionary.keys()` and `dictionary.values()` methods. These lists can be used in `for` loops:

```
for (key : myDictionary.keys()) {
    print(key + " -> " + myDictionary[key]);
}
```

The iteration order is consistent between platforms and will generally prefer the order that the keys were added to the dictionary. However, as with all unordered collections in other programming languages, the developer is encouraged to not depend on the iteration order since this is more of a behavioral quirk than a feature.

### Dictionary Methods

Dictionaries have many methods, including some already covered. This is the list of all dictionary methods.

| **Method** | **Explanation** | **Time Complexity** |
| --- | --- | -- |
| `dictionary.clear()` | Removes all items from the dictionary so that it is empty. | `O(1)`, `O(n)` deferred to GC |
| `dictionary.clone()` | Creates a shallow copy of the dictionary. | `O(n)` |
| `dictionary.contains(key)` | Checks to see if the dictionary contains the given key. | `O(1)` |
| `dictionary.get(key, fallbackValue[optional])` | Attempts to get the value at the given key. If `fallbackValue` is provided, the fallback value will be returned in the event that the key does not exist. If no fallback value is provided and the key does not exist, then `null` is returned. | `O(1)` |
| `dictionary.keys()` | Returns a list of keys in the dictionary. The order of this list is not guaranteed to correspond to the order the keys were added. The order IS guaranteed to be in the same order as the list returned by `dictionary.values()` | `O(n)` |
| `dictionary.merge(otherDictionary)` | Copies all the keys and values from `otherDictionary` into `dictionary`. If there are collisions, the original keys from `dictionary` are overwritten. | `O(otherDictionary)`, incurs an additional `O(dictionary.length)` if more capacity is needed to be allocated. |
| `dictionary.remove(key)` | Removes the given key and its value from the dictionary. If the key is not present, a `KeyNotFoundException` is thrown. | `O(1)` |
| `dictionary.values()` | Returns a list of values in the dictionary. The order of this list is not guaranteed to correspond to the order the values were added. The order IS guaranteed to be in the same order as the list returned by `dictionary.keys()` | `O(n)` |

## Null

The `null` contant is its own type. It has no methods or other notable behaviors. It represents the lack of a value.

When a `null` is used as though it were a non-null value, a `NullReferenceException` is thrown.

## Classes (Class Definition References)

> **NOTE:** This type is different than object instances, which is documented in a section below. A class type is a reference to the actual class definition itself.

To get a class definition reference, follow the class name with `.class`. Because `class` is a language keyword, this is guaranteed to not collide with a static field or method.

```
class Person {
    field name;
    field age;
}

...

personClass = Person.class;
print("I have defined a class called " + personClass.getName());
```

> Usage of the class definition type is not considered part of normal-use programming and is oriented around Reflection. For more reflection-oriented tasks, see the documentation of `Core.Reflection` in the `Core` library.

### Class Methods

| **Method** | **Explanation** | **Time Complexity** |
| --- | --- | --- |
| `classDef.getName()` | Returns the name of the class as a string. | `O(1)` |
| `classDef.createInstance(args...)` | Creates an instance of the class definition, as if you were calling the constructor. | `O(constructor)` |
| `classDef.isA(otherClassDef)` | Returns true if the class definition is the same as or a subclass of `otherClassDef`. This is different from the `is` operator which requires a hard-coded class definition name on the right side of `is` whereas `.isA()` can take a class definition value that can have a dynamic value. | `O(class hierarchy depth)` |

## Functions

This is the documentation of the function type, not the creation and use of functions.

Functions can be passed around as pointers, just like any other value.

### Function methods

There are a few built-in methods to the function value.

| **Method** | **Explanation** | **Time Complexity** |
| --- | --- | --- |
| `func.argCountMax()` | Returns an integer indicating the maximum possible number of arguments that can be passed into the function i.e. when you provide a value for all arguments including the optional ones. | `O(1)` |
| `func.argCountMin()` | Returns an integer indicating the minimum possible number of arguments that can be passed into the function i.e. when you don't provide a value for any optional arguments.. | `O(1)` |
| `func.getName()` | Returns the name of the function as a string. | `O(n)` |
| `func.invoke(argumentList)` | Invokes the function using the given arguments. This allows you to use a list as arguments instead of unwrapping the arguments and passing them individually, which is useful if the `func` value is dynamic. |

## Object Instances

Object instances are generally user-defined values and so they follow the features that you yourself have included.

### Reflection

Note that if you use the `typeof` function on an object instance, you will only get the enum value `Type.INSTANCE` in return (which resolves to an integer). If you want to get a reference to a specific class definition, there's a function called `Core.Reflection.getClassFromInstance(obj)`

You can also check if an object is an instance of a specific class or parent class by using the `is` operator.

```
if (obj is Person) {
    print(obj + " is a Person instance.");
}
```
