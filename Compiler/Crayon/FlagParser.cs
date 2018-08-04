using Exporter;
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
        private static readonly string GEN_DEFAULT_PROJ_ES = "genDefaultProjES";
        private static readonly string GEN_DEFAULT_PROJ_JP = "genDefaultProjJP";
        private static readonly string SHOW_LIB_STACK = "showLibStack";

        private static readonly HashSet<string> ATOMIC_FLAGS = new HashSet<string>() {
            READABLE_BYTE_CODE,
            LIBRARY_DEP_TREE,
            SHOW_PERFORMANCE_MARKERS,
            CBX,
            SHOW_LIB_STACK,
        };

        private static readonly HashSet<string> ONE_ARG_FLAGS = new HashSet<string>()
        {
            BUILD_TARGET,
            BUILD_FILE, // this will be implicitly applied to the first argument that ends in .build and has no flag associated with it.
            VM,
            VM_DIR,

            GEN_DEFAULT_PROJ,
            GEN_DEFAULT_PROJ_ES,
            GEN_DEFAULT_PROJ_JP,
        };

        private static readonly Dictionary<string, string> ALIASES = new Dictionary<string, string>()
        {
            { "genDefaultProject", GEN_DEFAULT_PROJ },
            { "genDefaultProjectES", GEN_DEFAULT_PROJ_ES },
            { "genDefaultProjectJP", GEN_DEFAULT_PROJ_JP },
        };

        public static ExportCommand Parse(string[] args)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            int i;

            // TODO: I need to fix the TODO below regarding having an unambiguous format.
            // For now, do a first-pass scrape of showLibStack.
            List<string> argsMutable = new List<string>(args);
            for (i = 0; i < argsMutable.Count; ++i)
            {
                if (argsMutable[i] == "-" + SHOW_LIB_STACK)
                {
                    argsMutable.RemoveAt(i);
                    output[SHOW_LIB_STACK] = "";
                }
            }
            args = argsMutable.ToArray();

            // TODO: change this. This is a hack.
            // Ideally, the format will change to something that is not ambiguous.
            // For now, just check to see if the first argument ends with ".build" and
            // that the second argument is not "-target".
            // Current idea is to have -export Foo.build instead of just Foo.build
            // However, this will make all currently written documentation and tutorials
            // correct, at least for 0.2.1.
            if (args.Length > 0 && args[0].ToLower().EndsWith(".build"))
            {
                if (args.Length == 1 || args[1].ToLower() != "-target")
                {
                    string[] directRunArgs = new string[args.Length - 1];
                    Array.Copy(args, 1, directRunArgs, 0, directRunArgs.Length);
                    return new ExportCommand()
                    {
                        IsDirectCbxRun = true,
                        DirectRunArgs = directRunArgs,
                        BuildFilePath = args[0],
                        DirectRunShowLibStack = output.ContainsKey(SHOW_LIB_STACK)
                    };
                }
            }

            i = 0;
            while (i < args.Length)
            {
                string arg = args[i];
                if (arg.StartsWith("-"))
                {
                    string noHyphen = arg.Substring(1);
                    if (ALIASES.ContainsKey(noHyphen))
                    {
                        noHyphen = ALIASES[noHyphen];
                    }

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
                else if (!output.ContainsKey(BUILD_FILE) && arg.ToLower().EndsWith(".build"))
                {
                    output[BUILD_FILE] = arg;
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

            if (args.ContainsKey(GEN_DEFAULT_PROJ))
            {
                command.DefaultProjectId = args[GEN_DEFAULT_PROJ].Trim();
                command.DefaultProjectLocale = "EN";
            }
            if (args.ContainsKey(GEN_DEFAULT_PROJ_ES))
            {
                command.DefaultProjectId = args[GEN_DEFAULT_PROJ_ES].Trim();
                command.DefaultProjectLocale = "ES";
            }
            if (args.ContainsKey(GEN_DEFAULT_PROJ_JP))
            {
                command.DefaultProjectId = args[GEN_DEFAULT_PROJ_JP].Trim();
                command.DefaultProjectLocale = "JP";
            }

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
