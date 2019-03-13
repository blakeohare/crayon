# ImageResources Library

The ImageResources library contains functionality to load images from the program's resources both synchronously
and asynchronously. It also allows interprets ImageSheet information as defined in the build file.

Ultimately the ImageResources library's various loaders will generate ImageResource instances, which do not do anything directly
and only represent an image resource. These resources are generally used by other libraries such as Graphics or ImageEncoder.

[ImageResources](#class-imageresource)
- [getWidth](#getwidth)
- [getHeight](#getheight)

[ImageLoader](#class-imageloader)
- [loadFromResourcesAsync](#loadfromresourcesasync)
- [loadFromResources](#loadfromresources)
- [isDone](#isdone)
- [getImage](#getimage)

[ImageSheet](#class-imagesheet)
- [loadFromResourcesAsync](#loadfromresourcesasync-1)
- [loadFromResources](#loadfromresources-1)
- [isDone](#isdone-1)
- [getProgress](#getprogress)
- [getImage](#getimage-1)
- [getFiles](#getfiles)

# Class: ImageResource

Represents an image resource in memory. An ImageResource is loaded and ready to use and is generally implemented as an in-memory
32-bit bitmap. There is very little that can be done with an image directly and is generally passed as input to functionality in
other libraries.

## Methods

### getWidth

`imageResource.getWidth()`

Returns the image's width.

**Return type**: integer.

---

### getHeight

`imageResource.getHeight()`

Returns the image's height.

**Return type**: integer.

---

# Class: ImageLoader

Loads an image from the program's resources.

## Methods

### loadFromResourcesAsync

`ImageLoader.loadFromResourcesAsync(path)`

Static method that will initiate loading an image from the embedded resources asynchronously.
The return value is an ImageLoader instance, from which loading status can be queried and ultimately fetch the image resource
after it has finished loading.

| Argument | Type | Description |
| --- | --- | --- |
| **path** | _string_ | A path to the image resource relative to the source root. The directory delimiter can either be `/` or `\` |

**Return type**: ImageLoader instance

See also: [ImageLoader.isDone](#isdone), [ImageLoader.getImage](#getimage)

---

### loadFromResources

`ImageLoader.loadFromResources(path)`

Static method that will load an image from the embedded resources synchronously.
The return value is an ImageResource instance.

| Argument | Type | Description |
| --- | --- | --- |
| **path** | _string_ | A path to the image resource relative to the source root. The directory delimiter can either be `/` or `\` |

**Return type**: ImageResource instance

---

### isDone

`loader.isDone()`

Checks to see if the asynchronous ImageLoader is done loading.

**Return type**: boolean.

---

### getImage

`loader.getImage()`

Gets an ImageResource from an asynchronous ImageLoader if it is finished.
`getImage()` may only be called once.
Will throw an error if called before finished. Be sure to check the status of [.isDone()](#isdone)

---

# Class: ImageSheet

Represents a collection of image resources that can all be loaded simultaneously.

## Methods

### loadFromResourcesAsync

`ImageSheet.loadFromResourcesAsync(sheetIdOrIdList)`

Static method to initiate loading of an ImageSheet or a collection of ImageSheets.
Returns an ImageSheet instance. However the returned instance is not ready to be used and must be checked for loading progress.

| Argument | Type | Description |
| --- | --- | --- |
| sheetIdOrIdList | _string or list of strings_ | An ID of an image sheet as defined by the build file, or a collection of such IDs. |

**Return Type**: ImageSheet instance.

See also: [imageSheet.isDone()](#isdone-1), [imageSheet.getProgress()](#getprogress), [imageSheet.getImage(path)](#getimage-1)

---

### loadFromResources

`ImageSheet.loadFromResources(sheetIdOrIdList)`

Static method to sychronously load an ImageSheet. Returns an ImageSheet instance which is ready to have `.getImage(path)` called on it.

| Argument | Type | Description |
| --- | --- | --- |
| sheetIdOrIdList | _string or list of strings_ | An ID of an image sheet as defined by the build file, or a collection of such IDs. |

**Return Type**: ImageSheet instance.

See also: [imageSheet.getImage(path)](#getimage-1)

---

### isDone

`imageSheet.isDone()`

Method to check if an ImageSheet instance is done loading.

**Return Type**: boolean

See also: [imageSheet.getProgress()](#getprogress)

---

### getProgress

`imageSheet.getProgress()`

Method to check what ratio of the ImageSheet has been loaded.
Ideal for creating progress bars on loading screens.

The return value is a float from 0.0 to 1.0.
Despite completion coinciding with a return value of 1.0, it is generally better style to check [.isDone()](#isdone-1) directly instead.

**Return Type**: float

See also: [imageSheet.getProgress()](#isdone-1)

---

### getImage

`imageSheet.getImage(path)`

Method to get an ImageResource instance from the image sheet.
This method can only be called if the ImageSheet is completely finished loading and will throw an error if called before.

| Argument | Type | Description |
| --- | --- | --- |
| **path** | _string_ | A path to the image resource relative to the source root. The directory delimiter can either be `/` or `\` |

**Return Type**: ImageResource instance

See also: [imageSheet.getProgress()](#isdone-1)

---

### getFiles

`imageSheet.getFiles()`

Returns a list of all files included in this ImageSheet.

**Return Type**: list of strings
