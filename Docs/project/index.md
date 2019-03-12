# Project Structure and Build Files

A Crayon project consists primarily of a build file, a source directory, and an output directory.

Note that a simple default build file can be generated from the command line by running `crayon -genDefaultProj ProjectName` 
which will create a directory called ProjectName with a build file in it with reasonable starting values and the source code
for a simple HelloWorld app (where *ProjectName* can be changed to some other name).

## Build File

A build file is a JSON file (with a **.build** file extension) that defines the various metadata properties of the project.

The root JSON object has a key called "id" which is the project ID of your app.
Additionally, there is a key called "targets" which contains a list of build targets that you can build from the command line
using `crayon YourBuildFile.build -target targetName`. Building a build target will export the project to the platform defined
in that target. A build target has a field called "platform" that denotes which platform the target exports to and a field called
"name" that designates how to invoke the target from the command line. See the [full list of platform targets](#platform-targets).

Other fields can generally exist in either the root object or within a specific target. Fields within a target object override
the value from the root object.

Consider the following sample build file for an Asteroids game. This build file has two targets for a beta build and a release build.
The beta build overrides the title, and enables some debug features through the use of build variables. 

```json
{
  "id": "Asteroids",
  "source": "source/",
  "output": "output/%TARGET_NAME%/",
  "icon": "assets/icon.png",
  "default-title": "Asteroids",

  "vars": [
    { "name": "enable_debug_features", "value": false }
  ],
  
  "targets": [
	{
	  "name": "javascript_release",
	  "platform": "javascript-app"
	},
    {
	  "name": "javascript_beta",
	  "platform": "javascript-app",
	  "default-title": "Asteroids (Beta)",
	  
      "vars": [
        { "name": "enable_debug_features", "value": true }
      ]
	}
  ]
}
```


These are the various fields that can be included in either the root object or a target object

| Name | Description |
| --- | --- |
| default-title | The default title that could be used by the platform-specific UI library. This is generally settable from code, however, certain platforms need to know the default title at compile time. Mobile apps, for example, need to know this for the app name. |
| id | An alphanumerics-only name for your project. Unlike the title, this is required and used by things like executable name, file names, metadata, user data directory, etc. |
| source | The relative path to the source directory. |
| output | The relative path to the output directory. This **MUST** be unique for each target. However, you can use **%TARGET_NAME%** in this value to insert the name of the target name so that you can define this once in the root object rather than individual per target. |
| var | Define a compile-time variable. A compile time variable can be used as a constant in code and defined on a target-by-target basis. The `<var>` element has a type attribute and 2 sub-elements: `<id>` and `<value>`. **id** is the name of the variable and **value** is the value it will have. <br> `<var type="boolean"><id>enable-audio</id><value>true</value></var>` |
| jsfileprefix | A path to pre-pend to all file resource paths in JavaScript projects. Since knowledge of the URL pattern of where the project will be uploaded to and whether it has a trailing slash should not be encoded into the actual source code itself, you can define a root-absolute path in the build file. <br> e.g. `<jsfileprefix>/uploads/mygame</jsfileprefix> |
| icon | A path relative to the build file of an image to use as a project icon. Specifically how this icon is used depends on the target platform. This value can be a comma-separated list of file names containing icons of different sizes. Many target platforms (such as Android icons and JavaScript favicons) support having different images for different sizes. |
| guidseed | An arbitrary string. This is used as the seed for the randomizer for generating a unique project GUID for C# projects. A recommended value is a comma-delimited list of: a reverse domain name (Java package style), a project version number, and **%TARGET%**. This helps prevent project GUID collisions between projects and versions.<br> `<guidseed>org.crayonlang.demos.asteroids,v0.2.0,%TARGET%</guidseed>` |
| imagesheets | This is used by the ImageResources library for consolidating image resources embedded in the project into larger image sheets for improving performance or optimizing disk/network reads. TODO: add separate page for Image Sheet documentation. |
| orientation | Used by mobile game platforms. This will fix the orientation of the screen into landscape or portrait mode. Possible values: **portrait**, **landsacpe**, **landscapeleft**, **landscaperight**, **upsidedown**, **all**. The default behavior is **portrait** when this field is not specified. |
| launch-screen | A path to an image that will appear while the app loads on platforms that require such a concept. Currently only applies to iOS. |
| vars | A list of compile-time constants. See the full documentation of [compile time variables](#compile-time-variables). |

### Compile Time Variables

Compile time variables are defined in the build file and are converted into constants in the source code at compile time.



```json
{
  "id": "MyProject",
  
  ...
  
  "vars": [
    { "name": "version", "value": 1.4 },
	{ "name": "enable-debug-output", "value": false }
  ],
  
  ...
  
  "targets": [
    {
	  "name": "release",
	  ...
	},
	{
	  "name": "test_version",
	  ...
	  "vars": [
	    { "name": "enable-debug-output", "value": true }
	  ]
	},
	...
  ]
}
```

In code, this would be used with the `$var` dictionary...
```csharp
  ...
  
  if ($var['show-debug-output']) {
    print showDebugOutput();
  }

  ...
```

### Platform Targets

This is the list of available targets that you can export your project to.

| Name | Description |
| --- | --- |
| csharp-app | A .NET project appropriate for consumption on a Desktop/Laptop on either Windows or Mac. |
| javascript-app-android | A JavaScript-based web view Android app + a small Java wrapper. |
| java-app | A Java program appropriate for consumption on a Desktop/Laptop (not Android). Uses Java 1.8 and ant builds. |
| python-app | A python program. |
| javascript-app | A JavaScript program appropriate for the general web consumption. |
| javascript-app-ios | An iOS app that's based on WebView + a small Swift wrapper |

## Source Directory

The source directory contains code files (with the file extension of **.cry**) and other asset files (such as sounds, images, text files, etc). 
The source code files are all compiled together. 
The arrangement or names of source code files do not matter. All non-code files are compiled into the final output and are 
available as resources (although there are some limitations to this that will be outlined later).

Code itself may only contain namespace, class, function, and enum definitions. 
"Unwrapped" code is not valid.

```
// This is a valid file...
namespace MyNamespace {
  function foo() {
    print("Hello, World!");
  }
}
```

```
// This is not valid...
namespace MyNamspace {
  function foo() {
    print("Uh oh.");
  }
}

print("This shouldn't be here.");
```

Class and Namespace names do not have to correlate with directory/file names. 
Everything is compiled and resolved iteratively without regard to where the code came from.

Execution of a program always begins with a function called `main` which is required in every project. 
A simple HelloWorld app would look like this...

```
function main() {
  print("Hello, World!");
}
```

The `main` function can also be nested in a namespace and still be the starting point of execution:
```
namespace MyHelloApp {
  function main() {
    print("Hello again, World!");
  }
}
```

## Output Directory

The output directory is populated with the source code of the final project after it has been exported. The output directory must not be nested under the source directory, but does not necessarily need to be nested in the directory where the build file is. 
