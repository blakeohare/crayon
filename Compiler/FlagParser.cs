using System;
using System.Collections.Generic;

namespace Crayon
{
    public class FlagParser
    {
        public static readonly string MIN_ARG = "min";
        public static readonly string READABLE_BYTE_CODE = "readablebytecode";
        public static readonly string LIBRARY_DEP_TREE = "librarydeptree";
        public static readonly string SHOW_PERFORMANCE_MARKERS = "showperf";
        public static readonly string BUILD_TARGET = "target";
        public static readonly string BUILD_FILE = "buildfile";
        public static readonly string VM = "vm";
        public static readonly string VM_DIR = "vmdir";
        public static readonly string CBX = "cbx";
        public static readonly string GEN_DEFAULT_PROJ = "genDefaultProj";

        private static readonly HashSet<string> ATOMIC_FLAGS = new HashSet<string>() {
            MIN_ARG,
            READABLE_BYTE_CODE,
            LIBRARY_DEP_TREE,
            SHOW_PERFORMANCE_MARKERS,
            CBX,
        };

        private static readonly HashSet<string> ONE_ARG_FLAGS = new HashSet<string>()
        {
            BUILD_TARGET,
            BUILD_FILE, // this will be implicitly applied to the first argument that ends in .build and has no flag associated with it.
            VM,
            VM_DIR,
            GEN_DEFAULT_PROJ,
        };

        public static Dictionary<string, string> Parse(string[] args)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();

            int i = 0;
            while (i < args.Length)
            {
                string arg = args[i];
                if (arg.StartsWith("-"))
                {
                    string noHyphen = arg.Substring(1);
                    if (ATOMIC_FLAGS.Contains(noHyphen))
                    {
                        if (output.ContainsKey(noHyphen))
                        {
                            throw new InvalidOperationException("Found an extraneous " + arg + " flag");
                        }
                        else
                        {
                            output[noHyphen] = "";
                        }
                        ++i;
                    }
                    else if (ONE_ARG_FLAGS.Contains(noHyphen))
                    {
                        if (output.ContainsKey(noHyphen))
                        {
                            throw new InvalidOperationException("Found an extraneous " + arg + " flag");
                        }
                        else if (i + 1 >= args.Length || args[i + 1].StartsWith("-"))
                        {
                            throw new InvalidOperationException("The " + arg + " argument requires a parameter following it.");
                        }
                        else
                        {
                            output[noHyphen] = args[i + 1];
                            i += 2;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown command line argument: " + arg);
                    }
                }
                else if (!output.ContainsKey("buildfile") && arg.ToLower().EndsWith(".build"))
                {
                    output["buildfile"] = arg;
                    ++i;
                }
                else
                {
                    // TODO: specific bad-argument exception that will make it show the usage notes.
                    throw new InvalidOperationException("Unexpected argument: '" + arg + "'");
                }
            }

            return output;
        }
    }
}
