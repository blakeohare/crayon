# Random Library

Functions for creating random values.

### randomBool

`Random.randomBool()`

Returns a random boolean.

**Return value**: boolean

---

### randomChance

`Random.randomChance(probability)` or `Random.randomChance(chances, total)`

Returns a boolean. The chance that `true` is returned is determined by the given odds.

| Argument | Type | Description |
| --- | --- | --- |
| **probability** | _float_ | A probability expressed as a ratio from 0.0 to 1.0 |
| **chances** | _number_ | The numerator **n** in "**n** chances out of **total**" |
| **total** | _number_ | The denominator **total** in "**n** chances out of **total**" |

**Return value**: boolean

```
// The following invocations have a "1 in a million chance" of returning true.
Random.randomChance(0.000001);
Random.randomChance(1, 1000000);
```

---

### randomFloat

`Random.randomFloat()`

Returns a random float value in the range 0.0 <= value < 1.0.

**Return value**: float

---

### randomInt

`Random.randomInt(max)` or `Random.randomInt(min, max)`

Returns a random integer in the given range.
If only a maximum upper bound is provided, 0 is automatically used as the lower bound.

| Argument | Type | Description |
| --- | --- | --- |
| **max** | _integer_ | The exclusive upper bound. |
| **min** | _integer_ | The inclusive lower bound. |

**Return value**: integer
