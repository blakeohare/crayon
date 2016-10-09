FOR OSX:

In the following steps `$CPD` will represent the directory that the Crayon product has been cloned into.

1. Install mono: http://www.mono-project.com/docs/about-mono/supported-platforms/osx/

2. Change the build target to `Release`: `$CPD> sed -i -e 's/\(.*Configuration Condition.*\)Debug\(.*\)/\1Release\2/' Compiler/CrayonOSX.csproj`

3. Compile the Crayon compiler: `$CPD> xbuild Compiler/CrayonOSX.csproj`

4. Run the Release script: `$CPD/Release> python release.py`

5. Copy the resulting directory (`crayon-VERSION-mono`) to the desired install location and update `$CRAYON_HOME` to point to this directory.

6. Optional: add an alias that will run the compiler, I use: `alias cryc='mono $CRAYON_HOME/crayon.exe'`


FOR LINUX:
- (TODO)
