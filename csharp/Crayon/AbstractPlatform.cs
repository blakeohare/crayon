using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;
using Crayon.Translator;

namespace Crayon
{
	internal abstract class AbstractPlatform
	{
		public bool IsMin { get; private set; }
		public AbstractTranslator Translator { get; private set; }
		public AbstractSystemFunctionTranslator SystemFunctionTranslator { get; private set; }

		public abstract bool IsAsync { get; }
		public abstract string OutputFolderName { get; }
		public abstract bool SupportsListClear { get; }
		public abstract bool IsStronglyTyped { get; }
		public abstract bool IntIsFloor { get; }
		public abstract bool ImagesLoadInstantly { get; }
		public abstract bool ScreenBlocksExecution { get; }
		protected abstract string GeneratedFilesFolder { get; }
		public abstract bool IsOpenGlBased { get; }

		// When passing args to a new stack frame, build these by placing args into a fixed length list
		// Otherwise append items from the stack to the variable length list.
		public abstract bool UseFixedListArgConstruction { get; }

		public virtual bool RemoveBreaksFromSwitch { get { return false; } }

		internal CompileContext Context { get; private set; }

		public AbstractPlatform(
			bool isMin,
			AbstractTranslator translator,
			AbstractSystemFunctionTranslator systemFunctionTranslator)
		{
			this.Context = new CompileContext();
			this.IsMin = isMin;
			this.Translator = translator;
			this.SystemFunctionTranslator = systemFunctionTranslator;
			this.Translator.Platform = this;
			this.SystemFunctionTranslator.Platform = this;
			this.SystemFunctionTranslator.Translator = translator;
		}

		private ByteBuffer GenerateByteCode(string inputFolder, List<string> spriteSheetOpsStringArgs, List<int[]> spriteSheetOpsIntArgs)
		{
			Parser userCodeParser = new Parser(null);
			ParseTree.Executable[] userCode = userCodeParser.ParseRoot(inputFolder);
			ByteCodeCompiler bcc = new ByteCodeCompiler();
			return bcc.GenerateByteCode(userCodeParser, userCode, spriteSheetOpsStringArgs, spriteSheetOpsIntArgs);
		}

		private void GenerateReadableByteCode(string path, ByteBuffer byteCode)
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

			System.IO.File.WriteAllText(path, string.Join("\r\n", output));
		}

		public void Compile(BuildContext buildContext, string inputFolder, string baseOutputFolder, string nullableReadableByteCodeOutputPath)
		{
			inputFolder = inputFolder.Replace('/', '\\');
			if (inputFolder.EndsWith("\\")) inputFolder = inputFolder.Substring(0, inputFolder.Length - 1);

			List<string> filesToCopyOver = new List<string>();
			this.GetRelativePaths(inputFolder, null, filesToCopyOver);

			SpriteSheetBuilder spriteSheetBuilder = new SpriteSheetBuilder(buildContext);
			foreach (string spriteSheetId in buildContext.SpriteSheetIds)
			{
				foreach (string fileMatcher in buildContext.SpriteSheetPrefixesById[spriteSheetId])
				{
					spriteSheetBuilder.AddPrefix(spriteSheetId, fileMatcher);
				}
			}
			List<string> spriteSheetOpsStringArgs = new List<string>();
			List<int[]> spriteSheetOpsIntArgs = new List<int[]>();
			Dictionary<string, FileOutput> spriteSheetFiles = new Dictionary<string,FileOutput>();
			HashSet<string> filesAccountedForInSpriteSheet = new HashSet<string>();
			spriteSheetBuilder.Generate(this.GeneratedFilesFolder, filesToCopyOver, spriteSheetOpsStringArgs, spriteSheetOpsIntArgs, spriteSheetFiles, filesAccountedForInSpriteSheet);

			ByteBuffer byteCodeBuffer = GenerateByteCode(inputFolder, spriteSheetOpsStringArgs, spriteSheetOpsIntArgs);

			if (nullableReadableByteCodeOutputPath != null)
			{
				this.GenerateReadableByteCode(nullableReadableByteCodeOutputPath, byteCodeBuffer);
			}

			string byteCode = ByteCodeEncoder.Encode(byteCodeBuffer);

			this.Context.ByteCodeString = byteCode;

			InterpreterCompiler interpreterCompiler = new InterpreterCompiler(this);
			Dictionary<string, Executable[]> executablesByFile = interpreterCompiler.Compile();

			StructDefinition[] structs = interpreterCompiler.GetStructDefinitions();

			HashSet<string> filesToCopyOverTemporary = new HashSet<string>(filesToCopyOver);
			foreach (string fileInSpriteSheet in filesAccountedForInSpriteSheet)
			{
				filesToCopyOverTemporary.Remove(fileInSpriteSheet.Replace('/', '\\'));
			}
			filesToCopyOver.Clear();
			filesToCopyOver.AddRange(filesToCopyOverTemporary);

			Dictionary<string, FileOutput> files = this.Package(
				buildContext,
				buildContext.ProjectID,
				executablesByFile,
				filesToCopyOver,
				structs,
				inputFolder,
				spriteSheetBuilder);

			foreach (string file in spriteSheetFiles.Keys)
			{
				if (files.ContainsKey(file))
				{
					throw new InvalidOperationException("Autogenerated sprite sheet files are overwriting existing files.");
				}

				files[file] = spriteSheetFiles[file];
			}

			string outputFolder = baseOutputFolder;

			Util.EnsureFolderExists(outputFolder);
			
			this.DeleteExistingContents(outputFolder);

			this.GenerateFiles(buildContext, files, outputFolder, inputFolder);
		}

		private void DeleteExistingContents(string rootDirectory)
		{
			// TODO: this
		}

		private void GenerateFiles(BuildContext buildContext, Dictionary<string, FileOutput> files, string rootDirectory, string inputDirectory)
		{
			foreach (string path in files.Keys)
			{
				FileOutput file = files[path];
				string fullOutputPath = System.IO.Path.Combine(rootDirectory, path).Replace('/', '\\').Replace("%PROJECT_ID%", buildContext.ProjectID);
				Util.EnsureFolderExists(fullOutputPath);
				switch (file.Type)
				{
					case FileOutputType.Text:
						System.IO.File.WriteAllText(fullOutputPath, file.TextContent);
						break;

					case FileOutputType.Binary:
						System.IO.File.WriteAllBytes(fullOutputPath, file.BinaryContent);
						break;

					case FileOutputType.Copy:
						string absolutePath = System.IO.Path.Combine(inputDirectory, file.RelativeInputPath);
						System.IO.File.Copy(absolutePath, fullOutputPath, true);
						break;

					case FileOutputType.Image:
						file.Bitmap.Save(fullOutputPath);
						break;

					default:
						throw new NotImplementedException();
				}
			}
		}

		private void GetRelativePaths(string root, string folder, List<string> output)
		{
			if (folder == null) folder = root;
			foreach (string subfolder in System.IO.Directory.GetDirectories(folder))
			{
				string lowername = subfolder.ToLowerInvariant();
				bool ignore = lowername == ".svn";
				if (!ignore)
				{
					GetRelativePaths(root, subfolder, output);
				}
			}

			foreach (string file in System.IO.Directory.GetFiles(folder))
			{
				string extension = System.IO.Path.GetExtension(file).ToLowerInvariant();
				bool ignore = (extension == ".cry") ||
					file == "thumbs.db" ||
					file == ".ds_store";

				if (!ignore)
				{
					string relativePath = file.Substring(root.Length + 1);
					output.Add(relativePath);
				}
			}
		}

		public abstract Dictionary<string, FileOutput> Package(
			BuildContext buildContext,
			string projectId, 
			Dictionary<string, Executable[]> finalCode, 
			List<string> filesToCopyOver, 
			ICollection<StructDefinition> structDefinitions,
			string fileCopySourceRoot,
			SpriteSheetBuilder spriteSheet);

		public virtual string TranslateType(string original)
		{
			return original;
		}
	}
}
