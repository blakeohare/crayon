# ImageEncoder Library

Converts ImageResource objects into byte lists.
Byte lists can then be saved to files via the FileIO library or uploaded to the web via the Http library.

The ImageEncoder currently supports the following formats:
- PNG
- JPEG
- ICO (Favicon)

# Class: PngEncoder

Encodes an ImageResource into the PNG format.

### constructor

`new PngEncoder(imageResource)`

Creates a new PngEncoder instance.

| Argument | Type | Description |
| --- | --- | --- |
| **imageResource** | _ImageResource_ | An image resource to encode. |

---

## Methods

### encode

`pngEncoder.encode()`

Encodes the image into the PNG format and returns a list of byte values.

**Return type**: list of integers

---

# Class: JpegEncoder

Encodes an ImageResource into the JPEG format.

### constructor

`new JpegEncoder(imageResource)`

Creates a new PngEncoder instance.

| Argument | Type | Description |
| --- | --- | --- |
| **imageResource** | _ImageResource_ | An image resource to encode. |

---

## Methods

### encode

`jpegEncoder.encode()`

Encodes the image into the JPEG format and returns a list of byte values.

**Return type**: list of integers

---

# Class: IconEncoder

Encodes a series of ImageResources into a single Icon (.ico/favicon) file.

Icons, unlike other formats, are actually an aggregation of multiple image files for different resolutions.

### constructor

`new IconEncoder()`

Creates a new instance of an IconEncoder.
Unlike other formats, this consumes no ImageResource instance and instead relies on [addImage](#addimage) to be invoked
per file to be included in the icon.

---

## Methods

### addImage

`iconEncoder.addImage(imageResource)`

Adds an ImageResource to the icon file.
The ImageResource must be small enough to fit into a 256x256 space.

The resolutions supported are:
- 16 x 16
- 32 x 32
- 64 x 64
- 128 x 128
- 256 x 256

However, an image does not have to be one of the above sizes.
Images are automatically padded to fit evenly into the smallest size possible.
The padded image will have the original image centered in it and will not be stretched or distorted to fit.

This method returns the encoder instance such that builder chain syntax may be used.

| Argument | Type | Description |
| --- | --- | --- |
| **imageResource** | _ImageResource_ | Image resource to add to the icon bundle. |

**Return type**: IconEncoder

---

### encode

`iconEncoder.encode()`

Encodes the icon bundle into the ICO format and returns a list of byte values.

**Return type**: list of integers
