using System;
using System.Collections.Generic;

namespace Crayon
{
    public class FlagParser
    {
        private static readonly string READABLE_BYTE_CODE = "readablebytecode";
        private static readonly string LIBRARY_DEP_TREE = "librarydeptree";
        private static readonly string SHOW_PERFORMANCE_MARKERS = "showperf";
        private static readonly string BUILD_TARGET = "target";
        private static readonly string BUILD_FILE = "buildfile";
        private static readonly string VM = "vm";
        private static readonly string VM_DIR = "vmdir";
        private static readonly string CBX = "cbx";
        private static readonly string GEN_DEFAULT_PROJ = "genDefaultProj";

        private static readonly HashSet<string> ATOMIC_FLAGS = new HashSet<string>() {
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

        public static ExportCommand Parse(string[] args)
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

            return GenerateExportCommand(output);
        }

        private static ExportCommand GenerateExportCommand(Dictionary<string, string> args)
        {
            ExportCommand command = new ExportCommand();

            if (args.Count == 0) command.IsEmpty = true;

            if (args.ContainsKey(GEN_DEFAULT_PROJ)) command.DefaultProjectId = args[GEN_DEFAULT_PROJ].Trim();
            if (args.ContainsKey(BUILD_FILE)) command.BuildFilePath = args[BUILD_FILE].Trim();
            if (args.ContainsKey(BUILD_TARGET)) command.BuildTarget = args[BUILD_TARGET].Trim();
            if (args.ContainsKey(VM_DIR)) command.VmExportDirectory = args[VM_DIR].Trim();
            if (args.ContainsKey(VM)) command.VmPlatform = args[VM].Trim();
            if (args.ContainsKey(CBX)) command.CbxExportPath = args[CBX].Trim();
            command.ShowPerformanceMarkers = args.ContainsKey(SHOW_PERFORMANCE_MARKERS);
            command.ShowLibraryDepTree = args.ContainsKey(LIBRARY_DEP_TREE);

            return command;
        }
    }
}
