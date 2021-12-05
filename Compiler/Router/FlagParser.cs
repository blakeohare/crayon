using System;
using System.Collections.Generic;
using System.Linq;
using Wax;

namespace Router
{
    internal class FlagParser
    {
        public ToolchainCommand Parse(string[] args)
        {
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

            ToolchainCommand output = new ToolchainCommand();
            if (targetFile != null)
            {
                if (targetFile.EndsWith(".cbx")) output.CbxFile = targetFile;
                else output.BuildFile = targetFile;
            }

            output.RuntimeArgs = runtimeArgs.ToArray();

            List<ExtensionArg> extensionArgList = new List<ExtensionArg>();
            foreach (string extension in extensionArgsByExtensionName.Keys.OrderBy(k => k))
            {
                Dictionary<string, string> extensionArgs = extensionArgsByExtensionName[extension];
                if (extensionArgs.Count == 0)
                {
                    extensionArgList.Add(new ExtensionArg() { Extension = extension });
                }
                foreach (string argName in extensionArgs.Keys.OrderBy(k => k))
                {
                    extensionArgList.Add(new ExtensionArg() { Extension = extension, Name = argName, Value = extensionArgs[argName] });
                }
            }
            output.ExtensionArgs = extensionArgList.ToArray();

            List<ToolchainArg> toolchainArgList = new List<ToolchainArg>();
            foreach (string toolchainArg in crayonArgsByName.Keys.OrderBy(k => k))
            {
                toolchainArgList.Add(new ToolchainArg() { Name = toolchainArg, Value = crayonArgsByName[toolchainArg] });
            }
            output.ToolchainArgs = toolchainArgList.ToArray();

            return output;
        }
    }
}
