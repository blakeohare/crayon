# Math Library

## Constants

### PI

`Math.PI`

Value equal to `3.141592653589793238462`.

---

### E

`Math.E`

Value equal to `2.718281828459045235360`

---

## Functions

### abs

`Math.abs(number)`

Returns the absolute value of a number.

| Argument | Type | Description |
| --- | --- | --- |
| **number** | _number_ | A number. |

**Return type**: float

---

### arccos

`Math.arccos(ratio)`

The Arc Cosine (or inverse cosine) of a ratio. Returns an angle in radians.

| Argument | Type | Description |
| --- | --- | --- |
| **ratio** | _number_ | Ratio value. |

**Return type**: float (radians)

---

### arcsin

`Math.arccos(ratio)`

The Arc Sine (or inverse sine) of a ratio. Returns an angle in radians.

| Argument | Type | Description |
| --- | --- | --- |
| **ratio** | _number_ | Ratio value. |

**Return type**: float (radians)

---

### arctan

`Math.arctan(ratio)` OR `Math.arctan(yComponent, xComponent)`

The Arc Tangent (or inverse tangent) of a ratio. Returns an angle in radians. 
The ratio can be provided as a single scalar value and a value will be returned in the range of `-pi / 2` to `pi / 2`.
Alternatively, the ratio can be provided as a vector of two values using 2 arguments (y and x components respectively).
This will return a value in the range of `0` to `2 * pi`.

| Argument | Type | Description |
| --- | --- | --- |
| **ratio** | _number_ | Ratio value. |

**Return type**: float (radians)

---

### cos

`Math.cos(theta)`

Calculates the cosine of an angle.

| Argument | Type | Description |
| --- | --- | --- |
| **theta** | _number_ | An angle in radians. |

**Return type**: float

---

### ensureRange

`Math.ensureRange(number, bound1, bound2)`

Ensures that the number is within the range of the given bounds.

`bound1` and `bound2` do not need to be in any sort of order.

The value returned will be one of the original inputs, and will preserve its type.

| Argument | Type | Description |
| --- | --- | --- |
| **number** | _number_ | A number. |
| **bound1** | _number_ | A boundary value. |
| **bound2** | _number_ | Another boundary value. |

**Return type**: integer or float

```
ensureRange(3, 1, 5); // 3
ensureRange(3.0, 1, 5); // 3.0
ensureRange(3, 1.0, 5.0); // 3
ensureRange(-10, 0, 20); // 0
ensureRange(-10, 0.0, 20); // 0.0
```

---

### floor

`Math.floor(number)`

Returns the floor value of a number i.e. the next closest integer less than or equal to the input.

Note that this will increase the magnitude of a number for negative non-whole numbers.

In addition to floats, integers are also valid input, but of course do nothing.

| Argument | Type | Description |
| --- | --- | --- |
| **number** | _number_ | A number. |

**Return type**: integer

---

### log10

`Math.log10(number)`

Calculates the log base 10 of a number.

This will return precise values when the input is precisely a power of 10.

| Argument | Type | Description |
| --- | --- | --- |
| **number** | _number_ | A number. |

**Return type**: float

---

### log2

`Math.log2(number)`

Calculates the log base 2 of a number.

This will return precise values when the input is precisely a power of 2.

| Argument | Type | Description |
| --- | --- | --- |
| **number** | _number_ | A number. |

**Return type**: float

---

### ln

`Math.ln(number)`

Calculates the natural log (base e) of a number.

| Argument | Type | Description |
| --- | --- | --- |
| **number** | _number_ | A number. |

**Return type**: float

---

### max

`Math.max(a, b)`

Returns the maximum value between two numbers.

The type is preserved. For example, if you pass in an integer and a float, the value that is returned will have its original type.

| Argument | Type | Description |
| --- | --- | --- |
| **a** | _number_ | A number. |
| **b** | _number_ | Another number. |

**Return type**: integer or float

---

### min

`Math.min(a, b)`

Returns the minimum value between two numbers.

The type is preserved. For example, if you pass in an integer and a float, the value that is returned will have its original type.

| Argument | Type | Description |
| --- | --- | --- |
| **a** | _number_ | A number. |
| **b** | _number_ | Another number. |

**Return type**: integer or float

---

### sign

`Math.sign(number)`

Returns `1` if the input is positive.
Returns `0` if the input is 0.
Returns `-1` if the input is negative.

| Argument | Type | Description |
| --- | --- | --- |
| **number** | _number_ | A number. |

**Return type**: integer

---

### sin

`Math.sin(theta)`

Calculates the sine of an angle.

| Argument | Type | Description |
| --- | --- | --- |
| **theta** | _number_ | An angle in radians. |

**Return type**: float

---

### tan

`Math.tan(theta)`

Calculates the tangent of an angle.

| Argument | Type | Description |
| --- | --- | --- |
| **theta** | _number_ | An angle in radians. |

**Return type**: float


