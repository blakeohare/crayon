# Graphics2D Library

Library for drawing shapes and images to a GameWindow screen. Works in conjunction with the [Game Library](game.md)

# Class: Draw

A class containing static methods for drawing various geometric primitives.

## Methods

### ellipse

`Draw.ellipse(left, top, width, height, red, green, blue, alpha = 255)`

Draws an ellipse on the screen at the given location with the given size and color.

Only supports drawing ellipses whose major and minor axes are aligned with the cartesian grid.

| Argument | Type | Description | Default |
| --- | --- | --- | --- |
| **left** | _integer_ | X coordinate of the left side of the ellipse. | |
| **top** | _integer_ | Y coordinate of the top side of the ellipse. | |
| **width** | _integer_ | Width of the ellipse. | |
| **height** | _integer_ | Height of the ellipse. | |
| **red** | _integer_ | Red color component. (0-255) | |
| **green** | _integer_ | Green color component. (0-255) | |
| **blue** | _integer_ | Blue color component. (0-255) | |
| **alpha** | _integer_ | Alpha value. (0-255) | 255 |

**Return type**: nothing

---

### fill

`Draw.fill(red, green, blue)`

Fills the screen with a solid color. Alpha is not supported as this is intended as the background color.
If your intention is to overlay a translucent layer on the entire screen, use [Draw.rectangle](#rectangle) with an alpha value.

| Argument | Type | Description | Default |
| --- | --- | --- | --- |
| **red** | _integer_ | Red color component. (0-255) | |
| **green** | _integer_ | Green color component. (0-255) | |
| **blue** | _integer_ | Blue color component. (0-255) | |

**Return type**: nothing

---

### line

`Draw.line(startX, startY, endX, endY, lineWidth, red, green, blue, alpha = 255)`

Draws a straight line on the screen. This line has flat caps that terminate exactly at the coordinates provided.

The start and end coordinates do not have to be provided in a specific order.

| Argument | Type | Description | Default |
| --- | --- | --- | --- |
| **startX** | _integer_ | X coordinate of the starting point of the line. | |
| **startY** | _integer_ | Y coordinate of the starting point of the line. | |
| **endX** | _integer_ | X coordinate of the ending point of the line. | |
| **endY** | _integer_ | Y coordinate of the ending point of the line. | |
| **lineWidth** | _integer_ | Thickness of the line. | |
| **red** | _integer_ | Red color component. (0-255) | |
| **green** | _integer_ | Green color component. (0-255) | |
| **blue** | _integer_ | Blue color component. (0-255) | |
| **alpha** | _integer_ | Alpha value. (0-255) | 255 |

---

### quad

`Draw.quad(ax, ay, bx, by, cx, cy, dx, dy, red, green, blue, alpha = 255)`

Draws an arbitrarily shaped quadrilateral on the screen with the given position and color.

As of version 0.2.0, only convex quadrilaterals are officially supported. 
The end result of concave quadrilaterals or quads with criss-crossing lines depends on the platform.
Unintended behavior can be avoided by keeping in mind that some platforms will render this as 2 triangles: `ABC` and `BCD`. 

| Argument | Type | Description | Default |
| --- | --- | --- | --- |
| **ax** | _integer_ | X coordinate of point A. | |
| **ay** | _integer_ | Y coordinate of point A. | |
| **bx** | _integer_ | X coordinate of point B. | |
| **by** | _integer_ | Y coordinate of point B. | |
| **cx** | _integer_ | X coordinate of point C. | |
| **cy** | _integer_ | Y coordinate of point C. | |
| **dx** | _integer_ | X coordinate of point D. | |
| **dy** | _integer_ | Y coordinate of point D. | |
| **red** | _integer_ | Red color component. (0-255) | |
| **green** | _integer_ | Green color component. (0-255) | |
| **blue** | _integer_ | Blue color component. (0-255) | |
| **alpha** | _integer_ | Alpha value. (0-255) | 255 |

**Return type**: nothing

---

### rectangle

`Draw.rectangle(left, top, width, height, red, green, blue, alpha = 255)`

Draws a rectangle on the screen at the given location with the given size and color.

Only supports drawing rectangles aligned with the cartesian grid. For drwaing arbitrary quadrilaterals, see [Draw.quad](#quad).

| Argument | Type | Description | Default |
| --- | --- | --- | --- |
| **left** | _integer_ | X coordinate of the left side of the rectangle. | |
| **top** | _integer_ | Y coordinate of the top side of the rectangle. | |
| **width** | _integer_ | Width of the rectangle. | |
| **height** | _integer_ | Height of the rectangle. | |
| **red** | _integer_ | Red color component. (0-255) | |
| **green** | _integer_ | Green color component. (0-255) | |
| **blue** | _integer_ | Blue color component. (0-255) | |
| **alpha** | _integer_ | Alpha value. (0-255) | 255 |

**Return type**: nothing

---

### triangle

`Draw.triangle(ax, ay, bx, by, cx, cy, red, green, blue, alpha = 255)`

Draws an arbitrarily shaped triangle on the screen with the given position and color.

| Argument | Type | Description | Default |
| --- | --- | --- | --- |
| **ax** | _integer_ | X coordinate of point A. | |
| **ay** | _integer_ | Y coordinate of point A. | |
| **bx** | _integer_ | X coordinate of point B. | |
| **by** | _integer_ | Y coordinate of point B. | |
| **cx** | _integer_ | X coordinate of point C. | |
| **cy** | _integer_ | Y coordinate of point C. | |
| **red** | _integer_ | Red color component. (0-255) | |
| **green** | _integer_ | Green color component. (0-255) | |
| **blue** | _integer_ | Blue color component. (0-255) | |
| **alpha** | _integer_ | Alpha value. (0-255) | 255 |

**Return type**: nothing
