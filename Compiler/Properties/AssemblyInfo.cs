using System.Reflection;
using System.Runtime.CompilerServices;
#if WINDOWS
using System.Runtime.InteropServices;
#endif

[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Crayon")]
[assembly: AssemblyCopyright("Copyright © 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if WINDOWS

[assembly: AssemblyTitle("Crayon")]
[assembly: ComVisible(false)]
[assembly: Guid("6c822f9b-cce6-49aa-acc5-a1a03788c983")]
[assembly: AssemblyFileVersion("0.2.1")]

#endif

#if OSX
[assembly: AssemblyTitle("CrayonMac")]

// The following attributes are used to specify the signing key for the assembly,
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]
#endif

[assembly: AssemblyVersion("0.2.1")]
