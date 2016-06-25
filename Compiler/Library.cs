using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon
{
    internal class Library
    {
        private string platformName;
        public string Name { get; set; }
        public string RootDirectory { get; set; }

        private Dictionary<string, string> replacements = new Dictionary<string, string>();
        
        public Library(string name, string libraryManifestPath, string platformName)
        {
            this.Name = name;
            this.RootDirectory = System.IO.Path.GetDirectoryName(libraryManifestPath);
            string[] manifest = System.IO.File.ReadAllText(libraryManifestPath).Split('\n');
            Dictionary<string, string> values = new Dictionary<string, string>();
            Dictionary<string, bool> flagValues = new Dictionary<string, bool>();

            this.platformName = platformName;
            string platformPrefix = "[" + platformName + "]";

            foreach (string line in manifest)
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.Length > 0 && line[0] != '#')
                {
                    string[] parts = trimmedLine.Split(':');
                    if (parts.Length >= 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1];
                        for (int i = 2; i < parts.Length; ++i)
                        {
                            value += ":" + parts[i];
                        }

                        if (key.StartsWith("["))
                        {
                            if (key.StartsWith(platformPrefix))
                            {
                                key = key.Substring(platformPrefix.Length).Trim();
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (key == "BOOL_FLAG")
                        {
                            // TODO: parse bool flag value
                            parts = value.Split(':');
                            if (parts.Length == 2)
                            {
                                key = parts[0].Trim();
                                bool boolValue = parts[1].Trim().ToLowerInvariant() == "true";
                                flagValues[key] = boolValue;
                            }
                            else
                            {
                                throw new ParserException(null, "Library '" + name + "' has a syntax error in a boolean flag.");
                            }
                        }
                        else
                        {
                            values[key] = value;
                        }
                    }
                    else if (parts.Length == 1 && parts[0].Length != 0)
                    {
                        throw new ParserException(null, "Library '" + name + "' has a syntax error in its manifest.");
                    }
                }
            }

            foreach (string key in flagValues.Keys)
            {
                this.replacements[key] = flagValues[key] ? "true" : "false";
            }

            this.filepathsByFunctionName = new Dictionary<string, string>();
            // Build a lookup dictionary of all file names that are simple function names e.g. "foo.cry"
            // Then go through and look up all the file names that contain . prefixes with the platform name and
            // overwrite the lookup value for that entry with the more specific path.
            // myFunction.cry
            // android.myFunction.cry
            // on Python, myFunction will be included for lib_foo_myFunction(), but on Android, android.myFunction.cry will be included instead.

            string[] files = System.IO.Directory.GetFiles(System.IO.Path.Combine(this.RootDirectory, "native"));
            Dictionary<string, string> moreSpecificFiles = new Dictionary<string, string>();
            foreach (string fileWithDirectory in files)
            {
                string file = System.IO.Path.GetFileName(fileWithDirectory);
                if (file.EndsWith(".cry"))
                {
                    string functionName = file.Substring(0, file.Length - ".cry".Length);
                    if (functionName.Contains('.'))
                    {
                        // Add this file to the more specific lookup, but only if it contains the current platform.
                        if (functionName.StartsWith(platformName + ".") ||
                            functionName.Contains("." + platformName + "."))
                        {
                            string[] parts = functionName.Split('.');
                            moreSpecificFiles[parts[parts.Length - 1]] = file;
                        }
                        else
                        {
                            // just let it get filtered away.
                        }
                    }
                    else
                    {
                        this.filepathsByFunctionName[functionName] = file;
                    }
                }
            }

            foreach (string functionName in moreSpecificFiles.Keys)
            {
                this.filepathsByFunctionName[functionName] = moreSpecificFiles[functionName];
            }
        }

        private Dictionary<string, string> filepathsByFunctionName;

        public string GetEmbeddedCode()
        {
            return this.ReadFile("embed.cry", true);
        }

        private Dictionary<string, string> supplementalFiles = null;

        public Dictionary<string, string> GetSupplementalTranslatedCode()
        {
            if (this.supplementalFiles == null)
            {
                this.supplementalFiles = new Dictionary<string, string>();
                string supplementalFilesDir = System.IO.Path.Combine(this.RootDirectory, "supplemental");
                if (System.IO.Directory.Exists(supplementalFilesDir))
                {
                    foreach (string filepath in System.IO.Directory.GetFiles(supplementalFilesDir))
                    {
                        string name = System.IO.Path.GetFileName(filepath);
                        if (name.EndsWith(".cry"))
                        {
                            string key = name.Substring(0, name.Length - ".cry".Length);
                            this.supplementalFiles[key] = this.ReadFile(System.IO.Path.Combine("supplemental", name), false);
                        }
                    }
                }
            }
            return this.supplementalFiles;
        }

        // TODO: once the DLL's are factored out
        public string GetTranslationCode(string functionName)
        {
            string prefix = "lib_" + this.Name.ToLower() + "_";
            if (!functionName.StartsWith(prefix))
            {
                throw new Exception();
            }
            string shortName = functionName.Substring(prefix.Length);
            if (!this.filepathsByFunctionName.ContainsKey(shortName))
            {
                throw new Exception();
            }
            
            return this.ReadFile(System.IO.Path.Combine("native", this.filepathsByFunctionName[shortName]), false);
        }

        Dictionary<string, string> translations = null;

        public string TranslateNativeInvocation(AbstractPlatform translator, string functionName, object[] args)
        {
            if (this.translations == null)
            {
                string methodTranslations = this.ReadFile(System.IO.Path.Combine("methods", this.platformName + ".txt"), false);
                this.translations = new Dictionary<string, string>();
                foreach (string line in methodTranslations.Split('\n'))
                {
                    string[] parts = line.Trim().Split(':');
                    if (parts.Length > 1)
                    {
                        string key = parts[0];
                        string value = parts[1];
                        for (int i = 2; i < parts.Length; ++i)
                        {
                            value += ":" + parts[i];
                        }
                        this.translations[key.Trim()] = value.Trim();
                    }
                }
            }

            if (this.translations.ContainsKey(functionName))
            {
                string output = this.translations[functionName];
                for (int i = 0; i < args.Length; ++i)
                {
                    string argAsString = translator.Translate(args[i]);
                    output = output.Replace("[ARG:" + (i + 1) + "]", argAsString);
                }
                return output;
            }

            throw new InvalidOperationException("No native translation provided for " + functionName);
        }

        private string ReadFile(string pathRelativeToLibraryRoot, bool failSilently)
        {
            string fullPath = System.IO.Path.Combine(this.RootDirectory, pathRelativeToLibraryRoot);
            if (System.IO.File.Exists(fullPath))
            {
                string text = System.IO.File.ReadAllText(fullPath);
                
                return Constants.DoReplacements(text, this.replacements);
            }

            if (failSilently)
            {
                return "";
            }

            throw new Exception(); // TODO: add token data here and throw a parser exception.
        }
    }
}
