This isn't meant to be used right now.

The Game Library for java-app-android has a file called GameLibDualStackHelper which
has a boolean in it called IS_OPEN_GL. Setting this to true or false will toggle between
the OpenGL ES 2.0 renderer and the WebView renderer.

These are both Java-based VM's, however the WebView renderer is just a renderer and it
receives render events from a JavaScript bridge. It's not very fast, and was mostly an
experiment out of curiosity and worth holding on to for reference.

The OpenGL implementation will likely not move forward for now as there are severe
performance issues with Java in general on Android in the way that the VM is currently
structured. Until these are resolved, the Android strategy for Crayon will likely move
towards a web view approach like the one used on iOS, although this has its own
performance problems as well, just not as severe.
