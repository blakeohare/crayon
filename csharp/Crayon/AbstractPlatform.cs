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

		private string GenerateByteCode(string inputFolder)
		{
			Parser userCodeParser = new Parser(null);
			ParseTree.Executable[] userCode = userCodeParser.ParseRoot(inputFolder);
			ByteCodeCompiler bcc = new ByteCodeCompiler();
			ByteBuffer byteBuffer = bcc.GenerateByteCode(userCodeParser, userCode);
			return ByteCodeEncoder.Encode(byteBuffer);
		}

		public void Compile(string inputFolder, string baseOutputFolder)
		{
			string byteCode = GenerateByteCode(inputFolder);

			this.Context.ByteCodeString = byteCode;

			InterpreterCompiler interpreterCompiler = new InterpreterCompiler(this);
			Dictionary<string, Executable[]> executablesByFile = interpreterCompiler.Compile();

			StructDefinition[] structs = interpreterCompiler.GetStructDefinitions();

			List<string> filesToCopyOver = new List<string>();
			this.GetRelativePaths(inputFolder, null, filesToCopyOver);
			Dictionary<string, FileOutput> files = this.Package(
				"CrayonProject",
				executablesByFile,
				filesToCopyOver,
				structs);

			string outputFolder = System.IO.Path.Combine(baseOutputFolder, this.OutputFolderName);

			if (!System.IO.Directory.Exists(baseOutputFolder))
			{
				throw new Exception("Target directory does not exist.");
			}

			if (!System.IO.Directory.Exists(outputFolder))
			{
				System.IO.Directory.CreateDirectory(outputFolder);
			}

			this.DeleteExistingContents(outputFolder);

			this.GenerateFiles(files, outputFolder, inputFolder);
		}

		private void DeleteExistingContents(string rootDirectory)
		{
			// TODO: this
		}

		private void GenerateFiles(Dictionary<string, FileOutput> files, string rootDirectory, string inputDirectory)
		{
			foreach (string path in files.Keys)
			{
				FileOutput file = files[path];
				string fullOutputPath = System.IO.Path.Combine(rootDirectory, path).Replace('/', '\\');
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

					default:
						// TODO: Sprite Sheeting file copy type
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
			string projectId, 
			Dictionary<string, Executable[]> finalCode, 
			List<string> filesToCopyOver, 
			ICollection<StructDefinition> structDefinitions);
	}
}
