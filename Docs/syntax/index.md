# Syntax and Language Basics

This is very dry and is mostly intended as a source of reference. If you are interested in learning the language, it may be best to start with the tutorials.

## Crayon is a Curly Brace Language

Crayon is a curly brace language, much like C, C++, Java, C#, PHP, or JavaScript. It is designed to adhere to the intersection of as many commonly known concepts from each common language and is relatively free of expectation-violating quirks to keep the learning curve relatively low.

## Comments
Comments are arbitrary annotations you can put in the code that will be ignored by the compiiler. This is mentioned first since many code samples in this documentation will have comments in them.

A comment is denoted by two double-slashes. Anything after two consecutive slashes is ignored till the end of the line.
```
// This is a comment. So is this.
This is not a comment // but this is.
```

Comments that span multiple lines start with `/*` and end with `*/`.

```
/*
  This is a comment.
  So is this.
*/
This is not

/*/ <-- this is not valid.
/**/ <-- this is

```

Comments make your code easier to read.

## Functions

Most code is organized into functions. A function is defined by the word `function` followed by the name of the function, followed by a list of parameter names delimited by commas, followed by a block of code.
The code within the function is executed from top to bottom sequentially, barring any conditional statements or loops.
A function name can consist of any alphanumeric characters or underscores, but must not begin with a number.

```
function foo(a, b, c) {
  // Code goes here.
}
```

A block of code is indicated by a `{` and `}`.  Once the interpreter reaches the end of a block, the function ends.

## Variables and Assignment

Values can be stored into a variable and referenced later. This is generally done using an equals sign. The types of values that can be stored into a variable are covered in more depth later. For now, simple numbers will be used in the examples.

```
myValue = 42;
```

The name of this variable is `myValue`. Variable names can contain any combination of letters, numbers, and underscores. However it must not begin with a number (to avoid parser ambiguity).

```
myValue = 1; // this is okay
_someValue = 2; // so is this
_012345 = 3; // and even this
4value = 4; // however, this is invalid.
```

If you assign the value of one variable into another variable, you are copying the contents of that variable to the new variable. You are not aliasing one variable to the other. Consider the following code sample:

```
var1 = 42;
var2 = var1;
var1 = 3.14159;
```

After the above code runs, `var2` will maintain the value of 42. The assignment of 3.14159 to `var1` only affects `var1`.

If a variable is used before any value has been assigned to it, it will result in an error.

## Simple Types

There are a handful of value types in Crayon. These are pieces of data that can be stored in variables and manipulated by the language. You have already seen numbers in the previous example.

### Integers

Integers are whole numbers. These can be positive or negative. The range of the integer type is limited to the underlying implementation of the platform, but -2,147,483,648 through 2,147,483,647 (-231 through 231 - 1) is the universally accepted "safe" range as all platforms support 32-bit signed integers at a minimum. If the target platform is JavaScript then it depends on the browser, but generally this is a 64 bit signed range. If the target platform is Python, the range is unlimited (memory bound).

Integers are notated in base 10. However, if an integer is prefixed with `0x`, it uses hexadecimal notation. Hexadecimal digits can be upper or lowercase.

```
a = 42; // fourty-two
b = 0x2a; // also forty-two (hexadecimal)
c = 0x2A; // this is valid as well
d = -100; // negative numbers are supported
e = 0123; // 0 prefixes are also valid decimal notation. Note that octal is not supported
f = 3000000000; // valid, but not recommended as it may hit the platform's range restriction
```

### Floats

"Float" is short for "floating point decimal". Despite mostly using the underlying platform's implementation, there are a few restrictions to traditional float representation. Primarily the notion of `NaN` and `Infinity` are not supported. Division by 0 results in an error.

```
// examples of floats
a = 0.0;
b = 2.5;
c = 3.14159;
d = 9906753.0;
e = -48151623.42;
f = .125; // dropping the leading 0 is valid
```

Note that a float that ends with `.0` is still considered a float, not an integer, despite it mathematically being a whole number. This differs from JavaScript.

### Strings

Strings are pieces of text. ASCII is considered "safe" however unicode support depends on the underlying platform. Strings are notated in Crayon as text that is enclosed with either single quote or a double quote.

```
a = "The kittens were plotting to overthrow their opressors.";
b = 'The squid had a terrible secret.';
c = "canteloupe";
d = 'To stop a sneeze, push up on your septum with your thumb.';
e = ""; // This string is empty, which is valid
```

Special characters can be notated in a string by using escape sequences. Escape sequences adhere to the universal/C standard). An escape sequences is denoted as a backslash `\` followed by a special character. This pair of characters is swapped out at compile-time with a different character that cannot be notated inline in the string.

```
a = "Quoth the raven, \"Nevermore\"."; // double quotes appearing in a double-quoted string
tab_character = '\t';
two_lines = "line 1\nline2";
```

Here is a list of all the available escape sequence codes:

| Escape Sequence | Compiled Value |
| --- | --- |
| \' | Single Quote |
| \" | Double Quote |
| \n | New Line |
| \r | Carriage Return |
| \t | Tab |
| \\ | Backslash |
| \0 | Null terminator |

If you want to check the length of a string, you can append `.length` to the end of a string or an expression containing a string.

```
weee_length = "Weeeeeeeeeeeeee".length;
meow_string = "Meow meow meow meow";
meow_length = meow_string.length;
```

### Booleans

A boolean value is either true or false. These are notated with the keywords `true` or `false`. These are used in conditions where something must be in one of two states.

```
bool1 = true;
bool2 = false;
// I'd give more examples, but that's the extent of it.
```

### Null

Null is used as a representation of the lack of a value. It is notated with the keyword `null`.

```
nothing_to_see_here = null;
```

## Complex Types

### Lists

### Dictionaries

### Objects

## Operators

Operators are things that take one or two expressions and combines them into a new value. For example, addition (+) is an operator which can take two numbers and combines them to their sum. Binary operators combine two values. Unary operators perform some sort of transformation on one value.

Operators are evaluated in a specific order (see Order of Operations below). If you want to override the order of operationrs, group items with parenthesis.

### Simple Arithmetic

There are a handful of simple arithmetic operators.

| Symbol and usage | Name |   |
| --- | --- | --- |
| x + y | Addition / Concatenation | If both values are numbers, they will be added together. If both values are lists, they will concatenated together and form a new list. If one of the values is a string, it will be concatenated to a string representation of the other value and form a new string. |
| x - y | Subtraction | Subtracts y from x |
| x * y | Multiplication | Multiplies x and y if they are both numbers. If one of the values is a string and the other is an integer, it will duplicate the string that many times and return a concatenated version of the string. |
| x / y | Division | Divides one number by another. If the denominator is 0, an error will result. |
| x % y | Modulo | Mods x by base y. If 0 is used as the base, an error will result. |
| x ** y | Exponent | Raises x to the power of y. 0 ** 0 results in 1. |
| -x | Negative sign | Flips the sign of the value. |

For most of these operators, if the values are integers, the result will also be an integer, otherwise it will result in a float. The exception is if you raise an integer to a negative exponent value, which will result in a float.

```
a = 1 + 3; // 4
b = (a + 1) * 2; // 10
c = a + 1 * 2; // 6
d = 102 % 10; // 2
e = 3 ** 5; // 243
f = -e; // -243
g = 3 ** -2; // 0.111111...
h = "meow" * 4; // "meowmeowmeowmeow"
```

### Value Comparisons

Values can be compared with various operators to return a boolean. These are frequently used for if statements.

| Symbol and usage | Name |    |
| --- | --- | --- |
| x == y | Equals | Returns true if both values are the same. For lists, dictionaries, and objects, it will only return true if the instance is the same. Will not recurisvely check component values. |
| x != y | Not Equals | Returns the opposite value as `==` |
| x < y | Less Than | Numbers only. Will return true if the left side is less than the right side. |
| x > y | Greater Than | Numbers only. Will return true if the left side is greater than the right side. |
| x <= y | Less Than Or Equal | Numbers only. Will return true if the left side is less than or equal to the right side. |
| x >= y | Greater Than Or Equal | Numbers only. Will return true if the left side is greater than or equal to the right side. |

```
a = 3 == 3; // true
b = 3 != 3.0; // false
c = "string" == 'string'; // true
d = 1 < 2; // true
e = 1 >= 2; // false
f = 10 < 10; // false
g = 4 >= 4; // true
h = [1, 2, 3] == [1, 2, 3]; // false. Separate list instances.
i = 1 < 3 < 5; // Invalid
```

### Boolean Logic

There are 3 boolean operators.

| Symbol and usage | Name |    |
| --- | --- | --- |
| x && y | And | Returns true if the boolean on the left and right are BOTH true. If the boolean on the left is false, then the expression on the right is not evaluated. This is commonly referred to as "short circuiting". |
| x \|\| y | Or | Returns true if either the boolean on the left or right are true. If the boolean on the left is true, then the expression on the right short circuits, similar to And. |
| !x | Not | This is unary operator. It will change true values to false and false values to true. |

All boolean operators only operate on boolean inputs. If another type is used, then that will result in an error.

```
a = 1 == 1 && 2 == 2; // true
b = false && something(); // false. something() is not evaluated.
c = true || something(); // true. something() is not evaluated.
d = something() || true; // true. something() IS evaluated.
f = !(1 == 2); // true. ! is a higher priority in order of operationrs than ==, so parenthesis are used.
```

### Bitwise Operators

Bitwise operators operate integers, specifically they perform operations on the individual binary bits an integer.

| Symbol and usage | Name |     |
| --- | --- | --- |
| x & y | Bitwise And | Returns a number where the bits are 1 if the corresponding bit in the input numbers are both 1. Otherwise the bit is 0. |
| x \| y | Bitwise Or | Returns a number where the bits are 1 if the corresponding bit in either of the input numbers is a 1. Otherwise the bit is 0. |
| x ^ y | Bitwise Xor | Xor stands for "eXclusive Or". Returns a number where the bits are 1 if one of the corresponding bits from one (but not both) of the input numbers is a 1. The bit is 0 if the input bits match. |
| x << y | Bit Shift Left | Moves all the bits in x to the left by y digits. The rightmost bit is set to 0. |
| x >> y | Bit Shift Right | Moves all the bits in x to the right by y digits. Supports sign extending (the bit on the left will match the bit that was previously on the left). |

Examples...

```
a = 7 & 20; // 4 (00111 & 10100 -> 00100)
b = 65 | 17; // 81 (1000001 | 0010001 -> 1010001);
c = 8 << 2; // 32 (00100 << 2 -> 10000);
d = 101 >> 1; // 50 (1101001 >> 1 -> 110100);
```

## Order of operations

There are several tiers of operations that have different priorities. The parser sorts expressions into these tiers and performs the operations within each tier group from left to right. This is known as Order of Operations.

```
a = 4 * 3 * 2 + 1 * 2; // 26, not 50. 4 * 3 * 2 runs, then 1 * 2, and then they are added together.
```

If you want to override the default order of operations, place parenthesis around pieces you want to evaluate first. For example, if you wanted the expression above to evaluate from left to right...

```
a = (4 * 3 * 2 + 1) * 2; // 50
```
Parenthesis are not required around `4 * 3 * 2` because it already has higher operation priority than `+`.

This chart illustrates each tier. Entries towards the top run first. Operators in the same tier run from left to right.

| Operators | Name | Priority |
| --- | --- | --- |
| () | Parenthesis | Highest |
| -, !, --, ++, [], (), . | Unary operators, including brackets, function invocations, and dot dereferencing (covered later). |  |
| ** | Exponents |   |
| *, /, % | Multiplication, Division, Modulo |   |
| +, - | Addition, Subtraction |   |
| <<, >> | Bitshifting |   |
| <, >, <=, >= | Inequality comparisons |   |
| ==, != | Equality comparisons |   |
| &, \|, ^ | Bitwise operators |   |
| &&, \|\| | Boolean operators |   |
| ?? | Null coalescing operator |   |
| ? : | Ternary expression (covered later) | Lowest |

## Control Flow

Control flow refers to changing the flow of a program from sequentially executing line-by-line. For example, if statements only execute lines conditionally. While loops execute lines multiple times. Crayon supports most control flow structures that appear in almost all curly-brace syntax programming languages.

### If Statements

An if statement checks an expression and executes some code if that expression resolves to true.

```
a = 4;
if (a == 4) {
  print("a is four.");
}
```

If statement expressions must always be booleans. Because `==` is an operator that resolves into a boolean, this is valid. Expressions that are not booleans DO NOT have a canonical boolean value. This is different from many languages.

```
a = 1;
if (a) { // This will crash at runtime. 1 is not a "true" boolean (pardon the pun).
  print("a is true.");
}
```

If an if statement only has 1 executable entity in it, then the curly braces are optional. The following code is completely equivalent to the code above:

```
a = 4;
if (a == 4)
  print("a is four.");
```

Or, because whitespace doesn't matter, you can also do this for brevity...

```
a = 4;
if (a == 4) print("a is four.");
```

### Else Statements

If the boolean expression in an if statement is false, the code will not run. However, if you want some code to run when the expression is false, then you can add an else statement...

```
a = 4;
if (a == 4) {
  print("a is four.");
} else {
  print("a is not four.");
}
```

Else statements can also omit curly braces if there is only one executable entity in it.

### If/Else Chains

Because an if statement itself counts as 1 executable entity, if the `else` statement contains just an `if` statement, you can abbreviate it like this into an if/else chain (also known as `elif`s in Python).

```
if (a == 4) {
  print("a is four.");
} else if (a % 2 == 0) {
  print("a is even.");
} else if (a % 2 == 1) {
  print("a is odd.");
} else {
  print("I don't know what a is.");
}
```

This is a list of mutually exclusive conditions that are evaluated sequentially. See also: switch statement.

### While Loops

While loops syntactically are similar to if statements, except keep repeating until the boolean condition expression becomes false.

```
a = 1;
while (a <= 10) {
  print("Counting: " + a);
  a++;
}
```

The above code displays the following:

```
1
2
3
4
5
6
7
8
9
10
```

Another example...

```
msg = "Greetings";
while (msg.length > 0) {
  print(msg);
  msg = msg[:-1]; // remove one character from the end.
}
```

The above code displays the following:

```
Greetings
Greeting
Greetin
Greeti
Greet
Gree
Gre
Gr
G
```

See slicing for more information about the `[:-1]` syntax.

### For Loops

There are two common types of loops. For loops offer a compact syntax for implementing both of them.

Looping through a series of numbers can be handled by while loops, but using for loop syntax is generally more compact, and more conventional anyway.

Consider the following example from the while loop example:

```
a = 1;
while (a <= 10) {
  print("Counting: " + a);
  a++;
}
```

This can be written more compactly using a for loop:

```
for (a = 1; a <= 10; a++) {
  print("Count: " + a);
}
```

A for loop contains components between the parenthesis:

**Initialization**: code that runs before the loop starts.
**Condition**: a boolean expression that is evaluated at the end of each loop.
**Step**: code that runs at the end of each loop BEFORE the condition is evaluated.
Initialization and Step can contain multiple statements, delimited by commas...

```
for (a = 1, b = 10; a <= 10; a++, b--) {
  print("Count up: " + a);
  print("Count down: " + b);
}
```

Additionally, any of these components can be eliminated altogether. If the condition is omitted, it is assumed to be an always-true expression.

For example, the following for loop...

```
for (;;) {
  print("Hello.");
}
```

...is functionally equivalent to the following while loop...

```
while (true) {
  print("Hello.");
}
```

As with while loops, be careful of not accidentally creating infinite loops. One common mistake of creating multiple nested loops is accidentally creating loops that modify the same variable names.

```
for (i = 0; i < width; ++i) {
  ...
  for (i = 0; i < height; ++i) {
    ...
  }
}
```

The above code runs forever because the inner loop is always resetting the looping variable in the outer loop, but may not be obvious, particularly if the code between the loop declarations is long and convoluted.

The other kind of common looping scenario is iterating through items in a list. This is commonly referred to as a for-each loop. However it also uses the for keyword.

```
grocery_items = ["apples", "milk", "eggs", "off brand Fruit Loops(TM)"];
for (item : grocery_items) {
  print("Remember to buy " + item);
}
```

This is roughly equivalent to dereferencing the individual items with an index:

```
grocery_items = ["apples", "milk", "eggs", "off brand Fruit Loops(TM)"];
for (i = 0; i < grocery_items.length; ++i) {
  item = grocery_items[i];
  print("Remember to buy " + item);
}
```

Because the index is tracked natively as opposed to interpreted code, it is slightly faster to use a foreach loop instead of keeping track of the index yourself. Also less error prone.

Additionally, it is actually okay to modify a collection while it is being used by a foreach loop. But be warned that it is only tracking the active index. If you remove an item from a list while you're looping through it, you will iterate through that item multiple times.

### Do While Loops

### Switch Statements

### Continue

### Break

### Ternary Expressions

### Null Coalescer

## More on functions

### Return

### Optional Parameters

## Enums and Constants

### Enums

### Constants

## Namespaces

## Imports

## Collections Revisited

### List Slicing

### List methods

### Dictionary Methods

## Object Oriented Programming

### Declaring a class

### Constructor

### Methods

### Fields

### Extended a class

### Static Methods

### Static Fields

### Static Constructors

### Private Constructors

### is Operator
