# Installation

- Create a directory somewhere on your hard drive where you want to save Crayon. For Windows, you can use **C:\Crayon**. For OSX, you can use **/Users/yourusername/crayon**. If you prefer a different location, that's okay as well.
- Create an environment variable called **CRAYON_HOME** that points to this folder. Also add **CRAYON_HOME** to the end of your **PATH** environment variable. If you are unsure how to edit environment variables, scroll down to the walkthrough below.
- Download the Zip file for your operating system from the [Download Page](http://crayonlang.org/download).
- Extract it. 
- In the folder that was extracted, there are several files. Move them into the folder you created. 
- **OSX Only**: Add the following line to your .bash_profile file: **alias crayon='mono $CRAYON_HOME/crayon.exe'**
- Open a command line/terminal window. Type **crayon** and hit enter. 

If you see usage information, you're good to go! If you see an error message, such as "command not found" or "'crayon' is not recognized as an internal or external command" then you ran into a problem. For extra help, consult the [mailing list](https://groups.google.com/forum/#!forum/crayon-lang).

## Editing Environment Variables for Windows
- open control panel (Windows + R, type "control", press OK)
- Click **System and Security**
- Click **System**
- Click **Advanced SystemSettings** on the left side bar
- Click the **Environment Variables...** button towards the bottom
- Click the **New...** button for System variables. 
- Set the name to **CRAYON_HOME** and the value to **C:\Crayon** (or to something different if you chose a different location). Press **OK**.
- Now find **PATH** under System variables and click **Edit...**. 
- For Windows 10, click **New** and add **%CRAYON_HOME%**. For other Windows versions, scroll to the end of the value and add **;%CRAYON_HOME%** (the semicolon is not a typo). 
- Press **OK** a few times to clear all the menus. 

## Editing Environment Variables for OSX
- Open a terminal and type **pico ~/.bash_profile**. This will open a text editor for your Terminal settings file. 
- Add the following lines to the end (be sure to change **yourusername** to your actual username, or use whichever directory you created if it's elsewhere): 
  - `export CRAYON_HOME="/Users/yourusername/crayon`
  - `export PATH=$CRAYON_HOME:$PATH`
- While you have this file open, go ahead and add the following line as well, which you will need to do later in the setup anyway:
  - `alias crayon='mono $CRAYON_HOME/crayon.exe'`
- Press **Ctrl + X** (not Cmd + X) to close and press **Y** to save changes. 
- Close the terminal. 

