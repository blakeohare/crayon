using Common;
using CommonUtil.Collections;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exporter
{
    internal class LibraryResourceDatabase
    {
        // This needs to be more extensible
        private abstract class ResourceCopyInstruction
        {
            public string Type { get; set; }
            public Dictionary<string, string> Values { get; set; }
            public virtual string Value { get; set; }
        }

        private LibraryExporter library;
        private Multimap<string, ExportEntity> exportEntities;

        public LibraryResourceDatabase(LibraryExporter library, AbstractPlatform platform)
        {
            this.library = library;
            this.exportEntities = null;
            this.ApplicablePlatformNamesMostGeneralFirst = new List<string>();
            while (platform != null)
            {
                this.ApplicablePlatformNamesMostGeneralFirst.Add(platform.Name);
                platform = platform.ParentPlatform;
            }
            this.ApplicablePlatformNamesMostGeneralFirst.Reverse();
        }

        public List<string> ApplicablePlatformNamesMostGeneralFirst { get; private set; }

        public Multimap<string, ExportEntity> ExportEntities
        {
            get
            {
                if (this.exportEntities == null)
                {
                    this.Init();
                }
                return this.exportEntities;
            }
        }

        private static readonly char[] COLON_CHAR = new char[] { ':' };

        public bool HasNativeCode
        {
            get { return this.library.Metadata.Manifest.Get("cni") != null; }
        }

        private List<Dictionary<string, string>> ParseApplicableInstructions()
        {
            List<Dictionary<string, string>> instructions = new List<Dictionary<string, string>>();
            foreach (string platformName in this.ApplicablePlatformNamesMostGeneralFirst)
            {
                string resourceManifest = this.library.Metadata.ReadFile(false, "native/" + platformName + "/manifest.txt", true).Trim();
                if (resourceManifest.Length == 0) continue;

                foreach (string lineRaw in resourceManifest.Split('\n'))
                {
                    string line = lineRaw.Trim();
                    if (line.Length == 0 || line[0] == '#') continue; // blank or comment
                    string[] parts = line.Split(COLON_CHAR, 2);

                    string command = parts[0].ToUpper().Trim();

                    Dictionary<string, string> values = KeyValuePairParser.Parse(parts[1]);
                    if (values == null)
                    {
                        throw new InvalidOperationException("The library '" + this.library.Metadata.ID + "' has a malformed copy instruction on the following line: " + lineRaw);
                    }
                    values["TYPE"] = command;

                    // pre-pend the current directory to any attributes that have a path. Currently this is just "from".
                    switch (command)
                    {
                        case "COPY_FILE":
                        case "COPY_FILES":
                        case "EMBED_FILE":
                        case "EMBED_FILES":
                        case "DOTNET_DLL":
                            if (values.ContainsKey("from"))
                            {
                                values["from"] = platformName + "/" + values["from"];
                            }
                            break;
                    }
                    instructions.Add(values);
                }
            }
            return instructions;
        }

        /*
         * Does a first pass on instructions and simplifies them to a smaller set of instructions.
         * In particular it expands out any multi-file operation into a series of single-file operations.
         * COPY_FILES and COPY_FILE become 1 or more COPY_CODE instructions.
         * EMBED_FILES and EMBED_FILE becomes 1 or more EMBED_CODE instructions.
         */
        private List<Dictionary<string, string>> GetResourceCopyInstructions()
        {
            List<Dictionary<string, string>> output = new List<Dictionary<string, string>>();
            StringBuilder totalEmbed = new StringBuilder();
            string from, to, content, typeFilter;
            foreach (Dictionary<string, string> instruction in this.ParseApplicableInstructions())
            {
                switch (instruction["TYPE"])
                {
                    case "COPY_FILES":
                        this.EnsureInstructionContainsAttribute("COPY_FILES", instruction, "from");
                        this.EnsureInstructionContainsAttribute("COPY_FILES", instruction, "to");
                        from = instruction["from"];
                        to = instruction["to"];
                        typeFilter = null;
                        if (instruction.ContainsKey("type"))
                        {
                            typeFilter = instruction["type"];
                            if (!typeFilter.StartsWith("."))
                            {
                                typeFilter = "." + typeFilter.ToLower();
                            }
                        }
                        foreach (string file in this.library.ListDirectory("native/" + from)
                            .Where(path => typeFilter == null || path.ToLower().EndsWith(typeFilter)))
                        {
                            content = this.library.Metadata.ReadFile(false, "native/" + from + "/" + file, false);
                            output.Add(new Dictionary<string, string>() {
                                { "TYPE", "COPY_CODE" },
                                { "target", to.Replace("%FILE%", file) },
                                { "value", content },
                            });
                        }
                        break;

                    case "COPY_FILE":
                        this.EnsureInstructionContainsAttribute("COPY_FILE", instruction, "from");
                        this.EnsureInstructionContainsAttribute("COPY_FILE", instruction, "to");
                        from = instruction["from"];
                        to = instruction["to"];
                        content = this.library.Metadata.ReadFile(false, "native/" + from, false);
                        output.Add(new Dictionary<string, string>()
                        {
                            { "TYPE", "COPY_CODE" },
                            { "target", to },
                            { "value", content },
                        });
                        break;

                    case "EMBED_FILE":
                        this.EnsureInstructionContainsAttribute("EMBED_FILES", instruction, "from");
                        from = instruction["from"];
                        totalEmbed.Append(this.library.Metadata.ReadFile(false, "native/" + from, false));
                        totalEmbed.Append("\n\n");
                        break;

                    case "EMBED_FILES":
                        this.EnsureInstructionContainsAttribute("EMBED_FILES", instruction, "from");
                        from = instruction["from"];
                        foreach (string file in this.library.ListDirectory("native/" + from))
                        {
                            totalEmbed.Append(this.library.Metadata.ReadFile(false, "native/" + from + "/" + file, false));
                            totalEmbed.Append("\n\n");
                        }
                        break;

                    default:
                        output.Add(instruction);
                        break;
                }
            }

            string totalEmbedFinal = totalEmbed.ToString();

            if (totalEmbedFinal.Length > 0)
            {
                output.Add(new Dictionary<string, string>()
                {
                    { "TYPE", "EMBED_CODE" },
                    { "value", totalEmbedFinal },
                });
            }

            return output;
        }

        private void EnsureInstructionContainsAttribute(string command, Dictionary<string, string> instruction, string attribute)
        {
            if (!instruction.ContainsKey(attribute))
            {
                throw new InvalidOperationException(command + " command in '" + this.library.Metadata.ID + "' is missing a '" + attribute + "' attribute.");
            }
        }

        private void Init()
        {
            this.exportEntities = new Multimap<string, ExportEntity>();

            ExportEntity entity;
            foreach (Dictionary<string, string> instruction in this.GetResourceCopyInstructions())
            {
                string command = instruction["TYPE"];
                switch (command)
                {
                    case "COPY_CODE":
                        this.EnsureInstructionContainsAttribute(command, instruction, "target");
                        this.EnsureInstructionContainsAttribute(command, instruction, "value");

                        entity = new ExportEntity()
                        {
                            FileOutput = new FileOutput()
                            {
                                Type = FileOutputType.Text,
                                TextContent = instruction["value"],
                            },
                        };
                        entity.Values["target"] = instruction["target"];
                        this.exportEntities.Add("COPY_CODE", entity);
                        break;

                    case "EMBED_CODE":
                        this.EnsureInstructionContainsAttribute(command, instruction, "value");
                        entity = new ExportEntity()
                        {
                            Value = instruction["value"],
                        };
                        this.exportEntities.Add("EMBED_CODE", entity);
                        break;

                    case "DOTNET_REF":
                        this.EnsureInstructionContainsAttribute(command, instruction, "name");
                        entity = new ExportEntity()
                        {
                            Type = "DOTNET_REF",
                            Value = instruction["name"],
                        };
                        this.exportEntities.Add("DOTNET_REF", entity);
                        break;

                    case "DOTNET_DLL":
                        this.EnsureInstructionContainsAttribute(command, instruction, "from");
                        this.EnsureInstructionContainsAttribute(command, instruction, "hintpath");

                        if (instruction.ContainsKey("to"))
                        {
                            throw new InvalidOperationException(
                                "DOTNET_DLL resource types should not use the 'to' field. " +
                                "The destination is automatically determined by the hintpath.");
                        }

                        string from = instruction["from"];

                        entity = new ExportEntity();

                        entity.FileOutput = new FileOutput()
                        {
                            Type = FileOutputType.Binary,
                            BinaryContent = this.library.Metadata.ReadFileBytes("native/" + from)
                        };

                        entity.Values["hintpath"] = instruction["hintpath"];
                        foreach (string dllAttr in new string[] {
                            "name", "version", "culture", "token", "architecture", "specificversion" })
                        {
                            if (instruction.ContainsKey(dllAttr))
                            {
                                entity.Values[dllAttr] = instruction[dllAttr];
                            }
                        }
                        this.exportEntities.Add("DOTNET_DLL", entity);
                        break;

                    default:
                        throw new InvalidOperationException("The command '" + command + "' is not recongized in the resource manifest of library: '" + this.library.Metadata.ID + "'");
                }
            }
        }
    }
}
