# Graphics2D Library

Library for drawing shapes and images to a GameWindow screen. Works in conjunction with the [Game Library](game.md)

[Draw](#class-draw)
- [ellipse](#ellipse)
- [fill](#fill)
- [line](#line)
- [quad](#quad)
- [rectangle](#rectangle)
- [triangle](#triangle)

[GraphicsTexture](#class-graphicstexture)
- [load](#load)
- [draw](#draw)
- [drawWithAlpha](#drawwithalpha)
- [drawRegion](#drawregion)
- [drawStretched](#drawstretched)
- [drawRegionStretched](#drawregionstretched)
- [drawRegionStretchedWithAlpha](#drawregionstretchedwithalpha)
- [drawRotated](#drawrotated)
- [drawRotatedWithAlpha](#drawrotatedwithalpha)
- [drawWithOptions](#drawwithoptions)
- [flip](#flip)
- [scale](#scale)

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

# Class: GraphicsTexture

A GraphicsTexture object is an image that has been loaded into the graphics engine and is ready to be displayed on the screen.
The images are loaded from ImageResource objects.

## Methods

### load

`GraphicsTexture.load(imageResource)`

Static method to load an ImageResource into the graphics engine and returns a GraphicsTexture instance.

| Argument | Type | Description |
| --- | --- | --- |
| **imageResource** | ImageResource | An image resource instance. |

**Return value**: GraphicsTexture

---

### draw

`texture.draw(x, y)`

Draws the texture onto the screen at full size at the given location.

| Argument | Type | Description |
| --- | --- | --- |
| **x** | _integer_ | X coordinate on the GameWindow of the left side of the image. |
| **y** | _integer_ | Y coordinate on the GameWindow of the top side of the image. |

**Return value**: nothing

---

### drawWithAlpha

`texture.drawWithAlpha(x, y, alpha)`

Draws the texture onto the screen at full size with the given transparency.

| Argument | Type | Description |
| --- | --- | --- |
| **x** | _integer_ | X coordinate on the GameWindow of the left side of the image. |
| **y** | _integer_ | Y coordinate on the GameWindow of the top side of the image. |
| **alpha** | _integer_ | Alpha value (0-255) |

**Return value**: nothing

---

### drawRegion

`texture.drawRegion(x, y, sourceX, sourceY, width, height)`

Draws a portion of the texture onto the screen at the given coordinate.
`sourceX`, `sourceY`, `width`, and `height` represent a rectangle within the image to draw, where `sourceX` and `sourceY` is the top left corner of that rectangle.
The image is drawn at the same scale.

| Argument | Type | Description |
| --- | --- | --- |
| **x** | _integer_ | X coordinate on the GameWindow of the left side of the final drawn image. |
| **y** | _integer_ | Y coordinate on the GameWindow of the top side of the final drawn image. |
| **sourceX** | _integer_ | Coordinate within the texture of the left side of the region to draw. |
| **sourceY** | _integer_ | Coordinate within the texture of the top side of the region to draw. |
| **width** | _integer_ | Width of the region within the image to draw. |
| **height** | _integer_ | Height of the region within the image to draw. |

**Return value**: nothing

---

### drawStretched

`texture.drawStretched(x, y, width, height)`

Draws the entire image onto the screen but stretches it to the given `width` and `height` values.

| Argument | Type | Description |
| --- | --- | --- |
| **x** | _integer_ | X coordinate on the GameWindow of the left side of the image. |
| **y** | _integer_ | Y coordinate on the GameWindow of the top side of the image. |
| **width** | _integer_ | The width to stretch the drawing to. |
| **height** | _integer_ | The height to stretch the drawing to. |

**Return value**: nothing

---

### drawRegionStretched

`texture.drawRegionStretched(targetX, targetY, targetWidth, targetHeight, sourceX, sourceY, sourceWidth, sourceHeight)`

Crops and stretches a specific region of the image while drawing it onto a region on the screen.

The `target`-prefixed arguments represent the region on the screen.

The `source`-prefixed arguments represent the region within the image.

| Argument | Type | Description |
| --- | --- | --- |
| **targetX** | _integer_ | X coordinate on the GameWindow of the left side of the region where to draw the image. |
| **targetY** | _integer_ | Y coordinate on the GameWindow of the top side of the region where to draw the image. |
| **targetWidth** | _integer_ | Width of the region on the GameWindow to draw the image. |
| **targetHeight** | _integer_ | Height of the region on the GameWindow to draw the image. |
| **sourceX** | _integer_ | Coordinate within the texture of the left side of the region to draw. |
| **sourceY** | _integer_ | Coordinate within the texture of the top side of the region to draw. |
| **sourceWidth** | _integer_ | Width of the region within the image to draw. |
| **sourceHeight** | _integer_ | Height of the region within the image to draw. |

**Return value**: nothing

---

### drawRegionStretchedWithAlpha

```
texture.drawRegionStretched(
    targetX,
    targetY,
    targetWidth,
    targetHeight,
    sourceX,
    sourceY,
    sourceWidth,
    sourceHeight,
    alpha)
```

Identical to drawRegionStretched, but with an alpha value.

| Argument | Type | Description |
| --- | --- | --- |
| **targetX** | _integer_ | X coordinate on the GameWindow of the left side of the region where to draw the image. |
| **targetY** | _integer_ | Y coordinate on the GameWindow of the top side of the region where to draw the image. |
| **targetWidth** | _integer_ | Width of the region on the GameWindow to draw the image. |
| **targetHeight** | _integer_ | Height of the region on the GameWindow to draw the image. |
| **sourceX** | _integer_ | Coordinate within the texture of the left side of the region to draw. |
| **sourceY** | _integer_ | Coordinate within the texture of the top side of the region to draw. |
| **sourceWidth** | _integer_ | Width of the region within the image to draw. |
| **sourceHeight** | _integer_ | Height of the region within the image to draw. |
| **alpha** | _integer_ | Alpha value (0-255) |

**Return value**: nothing

---

### drawRotated

`texture.drawRotated(centerX, centerY, theta)`

Draws the texture to the GameWindow screen centered at the given coordinates and rotated counterclockwise by the given angle.

| Argument | Type | Description |
| --- | --- | --- |
| **centerX** | _integer_ | X coordinate on the GameWindow of where the center of the image will be drawn. |
| **centerY** | _integer_ | Y coordinate on the GameWindow of where the center of the image will be drawn. |
| **theta**| _float_ | Angle (in radians) of how much to rotate the image. |

**Return value**: nothing

---

### drawRotatedWithAlpha

`texture.drawRotatedWithAlpha(centerX, centerY, theta, alpha)`

Identical to drawRotated except with alpha.

| Argument | Type | Description |
| --- | --- | --- |
| **centerX** | _integer_ | X coordinate on the GameWindow of where the center of the image will be drawn. |
| **centerY** | _integer_ | Y coordinate on the GameWindow of where the center of the image will be drawn. |
| **theta**| _float_ | Angle (in radians) of how much to rotate the image. |
| **alpha** | _integer_ | Alpha value (0-255) |

**Return value**: nothing

---

### drawWithOptions

```
texture.drawWithOptions(
    targetCenterX,
    targetCenterY,
    targetWidth,
    targetHeight,
    sourceX,
    sourceY,
    sourceWidth,
    sourceHeight,
    theta,
    alpha)
```

Draws the image onto the screen with all available options: stretch/shrink, crop, rotate, alpha.

| Argument | Type | Description |
| --- | --- | --- |
| **targetCenterX** | _integer_ | X coordinate on the GameWindow of where the center of the image will be drawn. |
| **targetCenterY** | _integer_ | Y coordinate on the GameWindow of where the center of the image will be drawn. |
| **targetWidth** | _integer_ | Final width of the image to draw on the screen. This width is the length across the top of the image before it is rotated. |
| **targetHeight** | _integer_ | Final height of the image to draw on the screen. This height is the length across the left/right side of the image before it is rotated. |
| **sourceX** | _integer_ | Coordinate within the texture of the left side of the region to draw. |
| **sourceY** | _integer_ | Coordinate within the texture of the top side of the region to draw. |
| **sourceWidth** | _integer_ | Width of the region within the image to draw. |
| **sourceHeight** | _integer_ | Height of the region within the image to draw. |
| **theta**| _float_ | Angle (in radians) of how much to rotate the image. |
| **alpha** | _integer_ | Alpha value (0-255) |

**Return value**: nothing

---

### flip

`texture.flip(flipHorizontal, flipVertical)`

Creates a new instance of the texture that is flipped horizontally, vertically, or both.

| Argument | Type | Description |
| --- | --- | --- |
| **flipHorizontal** | _boolean_ | Set to true to create a horizontally flipped image. |
| **flipVertical** | _boolean_ | Set to true to create a vertically flipped image. |

**Return value**: GraphicsTexture

Passing in false for both will return the reference to the original texture.

---

### scale

`texture.scale(width, height)`

Creates a new instance of the texture that is scaled to a new size.

| Argument | Type | Description |
| --- | --- | --- |
| **width** | _integer_ | New width to scale the texture to. |
| **height** | _integer_ | New height to scale the texture to. |

**Return value**: GraphicsTexture

While technically redundant with the various draw methods that include scaling the output, scaling the image itself may simplify code or have performance benefits.
