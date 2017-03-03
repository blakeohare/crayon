using System;
using System.Collections.Generic;
using Common;
using Common.ImageSheets;
using Crayon.ParseTree;
using Crayon.Translator;

namespace Crayon
{
    internal abstract class AbstractPlatform
    {
        public AbstractTranslator Translator { get; private set; }
        public AbstractSystemFunctionTranslator SystemFunctionTranslator { get; private set; }
        public InterpreterCompiler InterpreterCompiler { get; private set; }
        public SystemLibraryManager LibraryManager { get; private set; }
        public virtual bool TrimBom { get { return false; } }

        public PlatformId PlatformId { get; private set; }
        public LanguageId LanguageId { get; private set; }

        public abstract bool IsAsync { get; }
        public abstract bool SupportsListClear { get; }
        public abstract bool IsStronglyTyped { get; }
        public abstract bool IsArraySameAsList { get; }
        public abstract bool IsCharANumber { get; }
        public abstract bool IntIsFloor { get; }
        public abstract bool IsThreadBlockingAllowed { get; }
        public virtual bool IsByteCodeLoadedDirectly { get { return false; } }
        public abstract string PlatformShortId { get; }
        public virtual bool SupportsIncrement { get { return true; } }

        public string LibraryBigSwitchStatement { get; set; }

        public virtual bool RemoveBreaksFromSwitch { get { return false; } }

        public AbstractPlatform(
            PlatformId platform,
            LanguageId language,
            AbstractTranslator translator,
            AbstractSystemFunctionTranslator systemFunctionTranslator)
        {
            this.PlatformId = platform;
            this.LanguageId = language;

            this.Translator = translator;
            this.SystemFunctionTranslator = systemFunctionTranslator;
            this.Translator.Platform = this;
            this.SystemFunctionTranslator.Platform = this;
            this.SystemFunctionTranslator.Translator = translator;
        }

        public string Translate(object expressionObj)
        {
            Expression expression = expressionObj as Expression;
            if (expression == null)
            {
                throw new InvalidOperationException("Only expression objects provided by the compiler can be used here.");
            }
            List<string> output = new List<string>();
            this.Translator.TranslateExpression(output, expression);
            return string.Join("", output);
        }

        public string DoReplacements(bool keepReplacements, string code, Dictionary<string, string> replacements)
        {
            return Constants.DoReplacements(keepReplacements, code, replacements);
        }

        private ByteBuffer GenerateByteCode(BuildContext buildContext)
        {
            Parser userCodeParser = new Parser(null, buildContext, null);
            Executable[] userCode = userCodeParser.ParseAllTheThings();

            ByteCodeCompiler bcc = new ByteCodeCompiler();
            ByteBuffer buffer = bcc.GenerateByteCode(userCodeParser, userCode);
            this.LibraryManager = userCodeParser.SystemLibraryManager;
            this.LibraryBigSwitchStatement = this.LibraryManager.GetLibrarySwitchStatement(this);
            this.InterpreterCompiler = new InterpreterCompiler(this, userCodeParser.SystemLibraryManager);
            return buffer;
        }

        private string GenerateReadableByteCode(ByteBuffer byteCode)
        {
            List<string> output = new List<string>();
            List<int[]> rows = byteCode.ToIntList();
            List<string> stringArgs = byteCode.ToStringList();
            int length = rows.Count;
            int rowLength;
            int[] row;
            Dictionary<int, string> opCodes = new Dictionary<int, string>();
            foreach (OpCode opCode in Enum.GetValues(typeof(OpCode)))
            {
                opCodes[(int)opCode] = opCode.ToString();
            }
            List<string> rowOutput = new List<string>();
            for (int i = 0; i < length; ++i)
            {
                row = rows[i];
                rowLength = row.Length;
                rowOutput.Add(i + "\t");
                rowOutput.Add(opCodes[row[0]] + ":");
                for (int j = 1; j < row.Length; ++j)
                {
                    rowOutput.Add(" " + row[j]);
                }

                string sArg = stringArgs[i];
                if (sArg != null)
                {
                    rowOutput.Add(" " + sArg.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t"));
                }

                output.Add(string.Join("", rowOutput));
                rowOutput.Clear();
            }

            return string.Join("\r\n", output);
        }

        private static readonly string PROJECT_ID_VERIFICATION_ERROR_EMPTY = "Project ID is blank or missing.";
        private static readonly string PROJECT_ID_VERIFICATION_ERROR = "Project ID can only contain alphanumerics and cannot start with a number.";
        private void VerifyProjectId(string projectId)
        {
            if (projectId.Length == 0)
            {
                throw new InvalidOperationException(PROJECT_ID_VERIFICATION_ERROR_EMPTY);
            }
            foreach (char c in projectId)
            {
                if (!(c >= 'a' && c <= 'z') &&
                    !(c >= 'A' && c <= 'Z') &&
                    !(c >= '0' && c <= '9'))
                {
                    throw new InvalidOperationException(PROJECT_ID_VERIFICATION_ERROR);
                }
            }

            if (projectId[0] >= '0' && projectId[0] <= '9')
            {
                throw new InvalidOperationException(PROJECT_ID_VERIFICATION_ERROR);
            }
        }

        public void Compile(
            BuildContext buildContext,
            string baseOutputFolder)
        {
            Parser.IsTranslateMode_STATIC_HACK = false;

            this.VerifyProjectId(buildContext.ProjectID);

            ResourceDatabase resourceDatabase = ResourceDatabaseBuilder.CreateResourceDatabase(buildContext);

            ImageSheetBuilder imageSheetBuilder = new ImageSheetBuilder();
            if (buildContext.ImageSheetIds != null)
            {
                foreach (string imageSheetId in buildContext.ImageSheetIds)
                {
                    imageSheetBuilder.PrefixMatcher.RegisterId(imageSheetId);

                    foreach (string fileMatcher in buildContext.ImageSheetPrefixesById[imageSheetId])
                    {
                        imageSheetBuilder.PrefixMatcher.RegisterPrefix(imageSheetId, fileMatcher);
                    }
                }
            }
            Sheet[] imageSheets = imageSheetBuilder.Generate(resourceDatabase);

            resourceDatabase.AddImageSheets(imageSheets);

            resourceDatabase.GenerateResourceMapping();

            ByteBuffer byteCodeBuffer = this.GenerateByteCode(buildContext);
            
            resourceDatabase.ByteCodeFile = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = ByteCodeEncoder.Encode(byteCodeBuffer),
            };

            Parser.IsTranslateMode_STATIC_HACK = true;
            Dictionary<string, Executable[]> executablesByFile = this.InterpreterCompiler.Compile();
            Parser.IsTranslateMode_STATIC_HACK = false;

            StructDefinition[] structs = this.InterpreterCompiler.GetStructDefinitions();

            Dictionary<string, FileOutput> files = this.Package(
                buildContext,
                buildContext.ProjectID,
                executablesByFile,
                structs,
                resourceDatabase,
                this.LibraryManager);

            if (buildContext.ReadableByteCode)
            {
                files["readable_byte_code.txt"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = this.GenerateReadableByteCode(byteCodeBuffer),
                };
            }

            string outputFolder = baseOutputFolder;

            FileUtil.EnsureParentFolderExists(outputFolder);

            // TODO: delete all files and directories in the output folder that are not in the new output
            // which is better than deleting everything and re-exporting because the user may have command
            // lines and windows open viewing the previous content, which will prevent a full delete from
            // working, but won't stop a simple overwrite of old content.

            this.GenerateFiles(buildContext, files, outputFolder);
        }

        private void GenerateFiles(BuildContext buildContext, Dictionary<string, FileOutput> files, string rootDirectory)
        {
            rootDirectory = rootDirectory.Replace("%PROJECT_ID%", buildContext.ProjectID);
            foreach (string path in files.Keys)
            {
                FileOutput file = files[path];
                string fullOutputPath = FileUtil.JoinPath(rootDirectory, path);
                FileUtil.EnsureParentFolderExists(fullOutputPath);
                switch (file.Type)
                {
                    case FileOutputType.Text:
                        if (this.TrimBom)
                        {
                            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(file.TextContent);
                            FileUtil.WriteFileBytes(fullOutputPath, bytes);
                        }
                        else
                        {
                            FileUtil.WriteFileText(fullOutputPath, file.TextContent);
                        }
                        break;

                    case FileOutputType.Binary:
                        FileUtil.WriteFileBytes(fullOutputPath, file.BinaryContent);
                        break;

                    case FileOutputType.Copy:
                        string absolutePath = file.AbsoluteInputPath
                            ?? FileUtil.JoinPath(buildContext.ProjectDirectory, file.RelativeInputPath);
                        FileUtil.CopyFile(absolutePath, fullOutputPath);
                        break;

                    case FileOutputType.Image:
                        if (file.IsLossy)
                        {
                            string jpegPath = file.AbsoluteInputPath
                                ?? FileUtil.JoinPath(buildContext.ProjectDirectory, file.RelativeInputPath);
                            FileUtil.CopyFile(jpegPath, fullOutputPath);
                        }
                        else
                        {
                            FileUtil.WriteFileImage(fullOutputPath, file.Bitmap);
                        }
                        break;

                    case FileOutputType.Ghost:
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public abstract Dictionary<string, FileOutput> Package(
            BuildContext buildContext,
            string projectId,
            Dictionary<string, Executable[]> finalCode,
            ICollection<StructDefinition> structDefinitions,
            ResourceDatabase resourceDatabase,
            SystemLibraryManager libraryManager);

        public virtual string TranslateType(string original)
        {
            return original;
        }
    }
}
