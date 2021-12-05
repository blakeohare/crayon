using System;
using System.Collections.Generic;
using System.Linq;

namespace Router
{
    internal class FlagParser
    {
        public ParsedFlags Parse(string[] args)
        {
            if (args.Length == 0) return new ParsedFlags(ArgsIntent.SHOW_USAGE);
            if (args.Length == 1 && args[0] == "-version") return new ParsedFlags(ArgsIntent.SHOW_VERSION);

            Dictionary<string, Dictionary<string, string>> extensionArgsByExtensionName = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> crayonArgsByName = new Dictionary<string, string>();
            List<string> runtimeArgs = new List<string>();
            string targetFile = null;

            string[] parts;
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg[0] == '-')
                {
                    if (arg.StartsWith("-crayon:"))
                    {
                        parts = arg.Substring("-crayon:".Length).Split('=', 2, StringSplitOptions.None);
                        string argName = parts[0];
                        string argValue = parts.Length == 1 ? "" : parts[1];
                        crayonArgsByName[argName] = argValue;
                    }
                    else if (arg.StartsWith("-ext:"))
                    {
                        string extName;
                        string extArgName = null;
                        string extArgValue = null;
                        parts = arg.Substring("-ext:".Length).Split('.', 2, StringSplitOptions.None);
                        extName = parts[0];
                        if (parts.Length > 1)
                        {
                            parts = parts[1].Split('=', 2, StringSplitOptions.None);
                            extArgName = parts[0];
                            extArgValue = parts.Length == 1 ? "" : parts[1];
                        }

                        if (!extensionArgsByExtensionName.ContainsKey(extName))
                        {
                            extensionArgsByExtensionName.Add(extName, new Dictionary<string, string>());
                        }
                        extensionArgsByExtensionName[extName][extArgName] = extArgValue;
                    }
                    else
                    {
                        runtimeArgs.Add(arg);
                    }
                }
                else if (targetFile == null &&
                  (arg.ToLowerInvariant().EndsWith(".build") || arg.ToLowerInvariant().EndsWith(".cbx")))
                {
                    targetFile = arg;
                }
                else
                {
                    runtimeArgs.Add(arg);
                }
            }

            if (targetFile != null)
            {
                return new ParsedFlags(targetFile, crayonArgsByName, extensionArgsByExtensionName, runtimeArgs);
            }
            else if (extensionArgsByExtensionName.Count > 0)
            {
                return new ParsedFlags(extensionArgsByExtensionName, crayonArgsByName);
            }
            else
            {
                return new ParsedFlags(ArgsIntent.SHOW_USAGE);
            }
        }
    }

    internal enum ArgsIntent
    {
        SHOW_USAGE,
        SHOW_VERSION,
        CBX_WITH_ARGS,
        BUILD_WITH_ARGS,
        EXTENSIONS,
    }

    internal class ParsedFlags
    {
        public ArgsIntent Intent { get; private set; }
        private Dictionary<string, Dictionary<string, string>> extensionInfo = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, string> toolchainArgs = new Dictionary<string, string>();
        public string[] RuntimeArgs { get; set; }
        public string TargetFile { get; private set; }

        public ParsedFlags(
            string targetFile,
            Dictionary<string, string> crayonArgs,
            Dictionary<string, Dictionary<string, string>> extensionsAndArgs,
            IList<string> runtimeArgs)
        {
            this.TargetFile = targetFile;
            this.toolchainArgs = crayonArgs;
            this.extensionInfo = extensionsAndArgs;
            this.RuntimeArgs = runtimeArgs.ToArray();
            this.Intent = this.TargetFileIsCbx ? ArgsIntent.CBX_WITH_ARGS : ArgsIntent.BUILD_WITH_ARGS;
        }

        public ParsedFlags(Dictionary<string, Dictionary<string, string>> extensionsAndArgs, Dictionary<string, string> toolchainArgs)
        {
            this.Intent = ArgsIntent.EXTENSIONS;
            this.extensionInfo = extensionsAndArgs;
            this.RuntimeArgs = new string[0];
            this.toolchainArgs = toolchainArgs;
        }

        public ParsedFlags(ArgsIntent intent)
        {
            this.Intent = intent;
        }

        public bool TargetFileIsCbx { get { return this.TargetFile.ToLowerInvariant().EndsWith(".cbx"); } }

        public bool IncludeRun
        {
            get
            {
                return (this.Intent == ArgsIntent.CBX_WITH_ARGS || this.Intent == ArgsIntent.BUILD_WITH_ARGS) && !this.toolchainArgs.ContainsKey("skipRun");
            }
        }

        public string[] ToolchainArgs { get { return this.toolchainArgs.Keys.OrderBy(k => k).ToArray(); } }

        public bool HasToolchainArg(string name)
        {
            return this.toolchainArgs.ContainsKey(name);
        }

        public string GetToolchainArg(string name)
        {
            if (this.toolchainArgs.ContainsKey(name)) return this.toolchainArgs[name];
            return null;
        }

        public string[] ExtensionNames { get { return this.extensionInfo.Keys.OrderBy(k => k).ToArray(); } }

        public Dictionary<string, string> GetExtensionArgs(string extensionName)
        {
            // Let it throw a key error if not found.
            return new Dictionary<string, string>(this.extensionInfo[extensionName]);
        }
    }
}
