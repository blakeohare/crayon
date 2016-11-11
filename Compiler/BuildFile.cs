using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Crayon
{
	// TODO: split these classes out into their own namespace. It is time.
	public class BuildContext
	{
		public string ProjectID { get; set; }
		public string ProjectDirectory { get; set; }
		public string OutputFolder { get; set; }
		public FilePath[] SourceFolders { get; set; }
		public Dictionary<string, string[]> ImageSheetPrefixesById { get; set; }
		public Dictionary<string, BuildVarCanonicalized> BuildVariableLookup { get; private set; }
		public string[] ImageSheetIds { get; set; }
		public string JsFilePrefix { get; set; }
		public string Platform { get; set; }
		public bool Minified { get; set; }
		public bool ReadableByteCode { get; set; }
		public string GuidSeed { get; set; }
		public string IconFilePath { get; set; }
		public string DefaultTitle { get; set; }
		public string Orientation { get; set; }

		public enum VarType
		{
			BOOLEAN,
			INT,
			FLOAT,
			STRING
		}

		public class BuildVarCanonicalized
		{
			public string ID { get; set; }
			public VarType Type { get; set; }
			public string StringValue { get; set; }
			public int IntValue { get; set; }
			public bool BoolValue { get; set; }
			public double FloatValue { get; set; }
		}

		public class SourceItem
		{
			[XmlAttribute("alias")]
			public string Alias { get; set; }

			[XmlText]
			public string Value { get; set; }
		}

		public abstract class BuildItem
		{
			[XmlElement("projectname")]
			public string ProjectName { get; set; }

			[XmlElement("source")]
			public SourceItem[] Sources { get; set; }

			public SourceItem[] SourcesNonNull { get { return this.Sources ?? new SourceItem[0]; } }

			[XmlElement("output")]
			public string Output { get; set; }

			[XmlArray("imagesheets")]
			[XmlArrayItem("sheet")]
			public ImageSheet[] ImageSheets { get; set; }

			[XmlElement("jsfileprefix")]
			public string JsFilePrefix { get; set; }

			[XmlElement("minified")]
			public string MinifiedRaw { get; set; }

			[XmlElement("readablebytecode")]
			public string ExportDebugByteCodeRaw { get; set; }

			[XmlElement("var")]
			public BuildVar[] Var { get; set; }

			[XmlElement("guidseed")]
			public string GuidSeed { get; set; }

			[XmlElement("icon")]
			public string IconFilePath { get; set; }

			[XmlElement("title")]
			public string DefaultTitle { get; set; }

			[XmlElement("orientation")]
			public string Orientation { get; set; } // values = { portrait | landscape | auto }

			private bool TranslateStringToBoolean(string value)
			{
				if (value == null) return false;
				value = value.ToLowerInvariant();
				return value == "1" || value == "t" || value == "y" || value == "true" || value == "yes";
			}

			public bool Minified
			{
				get { return this.TranslateStringToBoolean(this.MinifiedRaw); }
			}

			public bool ExportDebugByteCode
			{
				get { return this.TranslateStringToBoolean(this.ExportDebugByteCodeRaw); }
			}
		}

		[XmlRoot("build")]
		public class Build : BuildItem
		{
			[XmlElement("target")]
			public Target[] Targets { get; set; }
		}

		public class Target : BuildItem
		{
			[XmlAttribute("name")]
			public string Name { get; set; }

			[XmlElement("platform")]
			public string Platform { get; set; }
		}

		public class BuildVar
		{
			[XmlElement("id")]
			public string Id { get; set; }

			[XmlAttribute("type")]
			public string Type { get; set; }

			[XmlElement("value")]
			public string Value { get; set; }

			[XmlElement("env")]
			public string EnvironmentVarValue { get; set; }
		}

		public class ImageSheet
		{
			[XmlAttribute("id")]
			public string Id { get; set; }

			[XmlElement("prefix")]
			public string[] Prefixes { get; set; }
		}

		public static BuildContext Parse(string projectDir, string buildFile, string targetName)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(Build));
			Build buildInput;
			try
			{
				buildInput = (Build)xmlSerializer.Deserialize(new StringReader(buildFile));
			}
			catch (InvalidOperationException e)
			{
				// Yeah, yeah, I know...
				string[] parts = e.Message.Split('(');
				if (parts.Length == 2)
				{
					parts = parts[1].Split(')');
					if (parts.Length == 2)
					{
						parts = parts[0].Split(',');
						if (parts.Length == 2)
						{
							int line, col;

							if (int.TryParse(parts[0], out line) && int.TryParse(parts[1], out col))
							{
								throw new InvalidOperationException("There is an XML syntax error in the build file on line " + line + ", column " + col);
							}
						}
					}
				}
				throw new InvalidOperationException("An error occurred while parsing the build file.");
			}
			Build flattened = buildInput;
			Target desiredTarget = null;
			foreach (Target target in buildInput.Targets)
			{
				if (target.Name == null) throw new InvalidOperationException("A target in the build file is missing a name.");
				if (target.Platform == null) throw new InvalidOperationException("A target in the build file is missing a platform.");
				if (target.Name == targetName)
				{
					desiredTarget = target;
				}
			}

			if (desiredTarget == null)
			{
				throw new InvalidOperationException("Build target does not exist in build file: '" + targetName + "'.");
			}

			Dictionary<string, BuildVarCanonicalized> varLookup = GenerateBuildVars(buildInput, desiredTarget, targetName);
			Dictionary<string, string> aliases = new Dictionary<string, string>();

			flattened.Sources = desiredTarget.SourcesNonNull.Union<SourceItem>(flattened.SourcesNonNull).ToArray();
			flattened.Output = FileUtil.GetCanonicalizeUniversalPath(DoReplacement(targetName, desiredTarget.Output ?? flattened.Output));
			flattened.ProjectName = DoReplacement(targetName, desiredTarget.ProjectName ?? flattened.ProjectName);
			flattened.JsFilePrefix = DoReplacement(targetName, desiredTarget.JsFilePrefix ?? flattened.JsFilePrefix);
			flattened.ImageSheets = MergeImageSheets(desiredTarget.ImageSheets, flattened.ImageSheets);
			flattened.MinifiedRaw = desiredTarget.MinifiedRaw ?? flattened.MinifiedRaw;
			flattened.ExportDebugByteCodeRaw = desiredTarget.ExportDebugByteCodeRaw ?? flattened.ExportDebugByteCodeRaw;
			flattened.GuidSeed = DoReplacement(targetName, desiredTarget.GuidSeed ?? flattened.GuidSeed);
			flattened.IconFilePath = DoReplacement(targetName, desiredTarget.IconFilePath ?? flattened.IconFilePath);
			flattened.DefaultTitle = DoReplacement(targetName, desiredTarget.DefaultTitle ?? flattened.DefaultTitle);
			flattened.Orientation = DoReplacement(targetName, desiredTarget.Orientation ?? flattened.Orientation);

			return new BuildContext()
			{
				ProjectDirectory = projectDir,
				JsFilePrefix = flattened.JsFilePrefix,
				OutputFolder = flattened.Output,
				Platform = desiredTarget.Platform,
				ProjectID = flattened.ProjectName,
				SourceFolders = ToFilePaths(projectDir, flattened.Sources),
				ImageSheetPrefixesById = flattened.ImageSheets.ToDictionary<ImageSheet, string, string[]>(s => s.Id, s => s.Prefixes),
				ImageSheetIds = flattened.ImageSheets.Select<ImageSheet, string>(s => s.Id).ToArray(),
				Minified = flattened.Minified,
				ReadableByteCode = flattened.ExportDebugByteCode,
				BuildVariableLookup = varLookup,
				GuidSeed = flattened.GuidSeed,
				IconFilePath = flattened.IconFilePath,
				DefaultTitle = flattened.DefaultTitle,
				Orientation = flattened.Orientation,
			};
		}

		private static FilePath[] ToFilePaths(string projectDir, SourceItem[] sourceDirs)
		{
			Dictionary<string, FilePath> paths = new Dictionary<string, FilePath>();

			foreach (SourceItem sourceDir in sourceDirs)
			{
				string relative = FileUtil.GetCanonicalizeUniversalPath(sourceDir.Value);
				FilePath filePath = new FilePath(relative, projectDir, sourceDir.Alias);
				paths[filePath.AbsolutePath] = filePath;
			}

			List<FilePath> output = new List<FilePath>();
			foreach (string key in paths.Keys.OrderBy<string, string>(k => k))
			{
				output.Add(paths[key]);
			}
			return output.ToArray();
		}

		private static string DoReplacement(string target, string value)
		{
			return value != null && value.Contains("%TARGET_NAME%")
				? value.Replace("%TARGET_NAME%", target)
				: value;
		}

		private static Dictionary<string, BuildVarCanonicalized> GenerateBuildVars(BuildItem root, BuildItem target, string targetName)
		{

			Dictionary<string, BuildVar> firstPass = new Dictionary<string, BuildVar>();

			if (root.Var != null)
			{
				foreach (BuildVar rootVar in root.Var)
				{
					if (rootVar.Id == null)
					{
						throw new InvalidOperationException("Build file contains a <var> without an id attribute.");
					}
					firstPass.Add(rootVar.Id, rootVar);
				}
			}

			if (target.Var != null)
			{
				foreach (BuildVar targetVar in target.Var)
				{
					if (targetVar.Id == null)
					{
						throw new InvalidOperationException("Build file target contains a <var> without an id attribute.");
					}
					firstPass[targetVar.Id] = targetVar;
				}
			}

			Dictionary<string, BuildVarCanonicalized> output = new Dictionary<string, BuildVarCanonicalized>();

			foreach (BuildVar rawElement in firstPass.Values)
			{
				string id = rawElement.Id;
				string value = rawElement.Value;
				int intValue = 0;
				double floatValue = 0;
				bool boolValue = false;
				VarType type = VarType.BOOLEAN;
				switch ((rawElement.Type ?? "string").ToLowerInvariant())
				{
					case "int":
					case "integer":
						type = VarType.INT;
						break;
					case "float":
					case "double":
						type = VarType.FLOAT;
						break;
					case "bool":
					case "boolean":
						type = VarType.BOOLEAN;
						break;
					case "string":
						type = VarType.STRING;
						break;
					default:
						throw new InvalidOperationException("Build file variable '" + id + "' contains an unrecognized type: '" + rawElement.Type + "'. Types must be 'string', 'integer', 'boolean', or 'float'.");
				}

				int score = (rawElement.EnvironmentVarValue != null ? 1 : 0)
					+ (rawElement.Value != null ? 1 : 0);

				if (score != 1)
				{
					throw new InvalidOperationException("Build file variable '" + id + "' must contain either a <value> or a <env> content element but not both.");
				}

				if (value == null)
				{
					value = System.Environment.GetEnvironmentVariable(rawElement.EnvironmentVarValue);
					if (value == null)
					{
						throw new InvalidOperationException("Build file varaible '" + id + "' references an environment variable that is not set: '" + rawElement.EnvironmentVarValue + "'");
					}
					value = DoReplacement(targetName, value.Trim());
				}

				switch (type)
				{
					case VarType.INT:
						if (!int.TryParse(value, out intValue))
						{
							throw new InvalidOperationException("Build file variable: '" + id + "' contains an invalid integer value.");
						}
						break;
					case VarType.FLOAT:
						if (!double.TryParse(value, out floatValue))
						{
							throw new InvalidOperationException("Build file variable: '" + id + "' contains an invalid float value.");
						}
						break;
					case VarType.BOOLEAN:
						switch (value.ToLowerInvariant())
						{
							case "0":
							case "no":
							case "false":
							case "f":
							case "n":
								boolValue = false;
								break;
							case "1":
							case "true":
							case "t":
							case "yes":
							case "y":
								boolValue = true;
								break;
							default:
								throw new InvalidOperationException("Build file variable: '" + id + "' contains an invalid boolean valud.");
						}
						break;
					case VarType.STRING:
						break;

					default:
						break;
				}
				output[id] = new BuildVarCanonicalized()
				{
					ID = id,
					Type = type,
					StringValue = value,
					IntValue = intValue,
					FloatValue = floatValue,
					BoolValue = boolValue
				};
			}

			return output;
		}

		private static ImageSheet[] MergeImageSheets(ImageSheet[] originalSheets, ImageSheet[] newSheets)
		{
			Dictionary<string, List<string>> prefixDirectLookup = new Dictionary<string, List<string>>();
			List<string> order = new List<string>();

			originalSheets = originalSheets ?? new ImageSheet[0];
			newSheets = newSheets ?? new ImageSheet[0];

			foreach (ImageSheet sheet in originalSheets.Concat<ImageSheet>(newSheets))
			{
				if (sheet.Id == null)
				{
					throw new InvalidOperationException("Image sheet is missing an ID.");
				}

				if (!prefixDirectLookup.ContainsKey(sheet.Id))
				{
					prefixDirectLookup.Add(sheet.Id, new List<string>());
					order.Add(sheet.Id);
				}
				prefixDirectLookup[sheet.Id].AddRange(sheet.Prefixes);
			}

			return order
				.Select<string, ImageSheet>(
					id => new ImageSheet()
					{
						Id = id,
						Prefixes = prefixDirectLookup[id].ToArray()
					})
				.ToArray();
		}
	}
}
