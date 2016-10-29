# Storing Data Between Sessions

Files in this project:
* [UserDataTutorial.build](UserDataTutorial.build)
* [source/main.cry](source/main.cry)

The UserData library allows you to save files locally to the computer/device in a sandboxed application-scoped way. There is generally a standard way to do this on each platform:

* For Windows, this is a folder in the **%APPDATA%** with the name of the current project, which in this case is UserDataTutorial. **%APPDATA%** generally expands to something like **C:\Users\Blake\AppData\Roaming**.
* For Linux/Mac, this is the ~/.UserDataTutorial directory.
* For JavaScript, this is the localStorage object.
* For Android, it is an application scoped directory

Luckily the UserData library abstracts all of these methods for the developer and makes them all appear as though they are an isolated disk with a normal file system. Even on JavaScript, there is a virtualized file system that uses the localStorage as its permanent storage medium.

Most of the UserData library mirrors very closely to the FileIO library. To illustrate it in action, this tutorial is just a simple demo program with a blue dot that will appear the last place you click in the window. When you close the program and restart it, the dot will start in the last position it appeared previously.

