# Project Structure and Build Files

A Crayon project consists of a build file, a source directory, and an output directory.

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

## Build File

A build file is an XML file (with a **.build** file extension) that defines the various metadata properties of the project.

The root element of a build file is **<build>** which contains various **<target>** elements nested in it. 
Each target has a **name** attribute which represents the command line value to invoke building that target.

Both **<build>** and **<target>** elements may contain any build property. 
The final build instructions are determined by flattening the properties of the target on top of the properties defined directly
in the build element.

One of these properties are **<var>** elements can be included

```xml
<build>
  <title>Asteroids</title>
  
  ...
  
  <target name="javascript_beta">
    <title>Asteroids (Beta)</title>
    ...
  </target>
  
  <target name="javascript_release">
    ...
  </target>
  
</build>
```

These are the various elements that can be included as a sub property of a **<build>** or **<target>**:

| Name | Description |
| --- | --- |
| title | The default title that could be used by the platform-specific UI library. This is generally settable from code, however, certain platforms need to know the default title at compile time. Mobile apps, for example. <br> `<title>My Neat-o Game</title>` |
| projectname | An alphanumerics-only name for your project. Unlike the title, this is required and used by things like executable name, file names, metadata, user data directory, etc. <br> `<projectname>myneatogame</projectname>` |
| source | The relative path to the source directory. <br> `<source>source</source>` |
| output | The relative path to the output directory. This **MUST** be unique for each target. However, you can use **%TARGET_NAME%** in this value to insert the name of the target so that you can define this once on the build element. <br> `<output>bin/%TARGET_NAME%</output>` |
| platform | The platform this target is exporting to. See the [full list of platform targets](#platform-targets). <br> `<platform>game-csharp-opentk</platform>` |
| var | Define a compile-time variable. A compile time variable can be used as a constant in code and defined on a target-by-target basis. The `<var>` element has a type attribute and 2 sub-elements: `<id>` and `<value>`. **id** is the name of the variable and **value** is the value it will have. <br> `<var type="boolean"><id>enable-audio</id><value>true</value></var>` |
| jsfileprefix | A path to pre-pend to all file references. Since knowledge of the URL pattern of where the project will be uploaded to and whether it has a trailing slash should not be encoded into the actual source code itself, you can define a root-absolute path in the build file. <br> `<jsfileprefix>/uploads/mygame</jsfileprefix> |
| icon | A path relative to the build file of an image to use as a project icon. Specifically how this is used depends on the target platform. |
| guidseed | An arbitrary string. This is used as the seed for the randomizer for generating a unique project GUID for C# projects. A recommended value is a comma-delimited list of: a reverse domain name (Java package style), a project version number, and **%TARGET%**. This helps prevent project GUID collisions between projects and versions.<br> `<guidseed>org.crayonlang.demos.asteroids,v0.2.0,%TARGET%</guidseed>` |
| imagesheets | This is used by the ImageResources library for consolidating image resources embedded in the project into larger image sheets for improving performance or optimizing disk/network reads. TODO: add separate page for Image Sheet documentation. |
| orientation | Used by mobile game platforms. This will fix the orientation of the screen into landscape or portrait mode. Possible values: **landsacpe**, **portrait**, **auto**. The default value is **auto**. |

### Compile Time Variables

Compile time variables are defined in the build file and are converted into constants in code.

```xml
<build>
  <var type="boolean">
    <id>show-debug-output</id>
    <value>true</value>
  </var>
  
  ...
  
  <target name="debug">
    ...
  </target>
  
  <target name="release">
    <var type="boolean">
      <id>show-debug-output</id>
      <value>false</value>
    </var>
    ...
  </target>
</build>
```

In code, this would be used with the `$var` dictionary...
```
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
| game-csharp-opentk | A game project using C# and OpenTK designed for using on a Desktop/Laptop on Windows. |
| game-csharp-android | An Android game project using C# and Xamarin. |
| game-java-awt | A game project using basic Java for using on a Desktop/Laptop. |
| game-python-pygame | A game project using Python and PyGame. |
| game-javascript | A game project using JavaScript and the HTML5 Canvas. |

## Output Directory

The output directory is populated with the source code of the final project after it has been exported. The output directory must not be nested under the source directory, but does not necessarily need to be nested in the directory where the build file is. 
