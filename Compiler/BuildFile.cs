using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Crayon
{
	public class BuildContext
	{
		public string ProjectID { get; set; }
		public string OutputFolder { get; set; }
		public string SourceFolder { get; set; }
		public Dictionary<string, string[]> SpriteSheetPrefixesById { get; set; }
		public Dictionary<string, BuildVarCanonicalized> BuildVariableLookup { get; private set; }
		public string[] SpriteSheetIds { get; set; }
		public string JsFilePrefix { get; set; }
		public string Platform { get; set; }
		public bool Minified { get; set; }
		public bool ReadableByteCode { get; set; }
        public string GuidSeed { get; set; }

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

		public abstract class BuildItem
		{
			[XmlElement("projectname")]
			public string ProjectName { get; set; }

			[XmlElement("source")]
			public string Source { get; set; }

			[XmlElement("output")]
			public string Output { get; set; }

			[XmlArray("spritesheets")]
			[XmlArrayItem("sheet")]
			public SpriteSheet[] SpriteSheets { get; set; }

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

		public class SpriteSheet
		{
			[XmlAttribute("id")]
			public string Id { get; set; }

			[XmlElement("prefix")]
			public string[] Prefixes { get; set; }
		}

		public static BuildContext Parse(string buildFile, string targetName)
		{
			System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(Build));
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

			flattened.Output = DoReplacement(targetName, desiredTarget.Output ?? flattened.Output);
			flattened.Source = DoReplacement(targetName, desiredTarget.Source ?? flattened.Source);
			flattened.ProjectName = DoReplacement(targetName, desiredTarget.ProjectName ?? flattened.ProjectName);
			flattened.JsFilePrefix = DoReplacement(targetName, desiredTarget.JsFilePrefix ?? flattened.JsFilePrefix);
			flattened.SpriteSheets = MergeSpriteSheets(desiredTarget.SpriteSheets, flattened.SpriteSheets);
			flattened.MinifiedRaw = desiredTarget.MinifiedRaw ?? flattened.MinifiedRaw;
			flattened.ExportDebugByteCodeRaw = desiredTarget.ExportDebugByteCodeRaw ?? flattened.ExportDebugByteCodeRaw;
            flattened.GuidSeed = DoReplacement(targetName, desiredTarget.GuidSeed ?? flattened.GuidSeed);

			return new BuildContext()
			{
				JsFilePrefix = flattened.JsFilePrefix,
				OutputFolder = flattened.Output,
				Platform = desiredTarget.Platform,
				ProjectID = flattened.ProjectName,
				SourceFolder = flattened.Source,
				SpriteSheetPrefixesById = flattened.SpriteSheets.ToDictionary<SpriteSheet, string, string[]>(s => s.Id, s => s.Prefixes),
				SpriteSheetIds = flattened.SpriteSheets.Select<SpriteSheet, string>(s => s.Id).ToArray(),
				Minified = flattened.Minified,
				ReadableByteCode = flattened.ExportDebugByteCode,
				BuildVariableLookup = varLookup,
                GuidSeed = flattened.GuidSeed,
			};
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

		private static SpriteSheet[] MergeSpriteSheets(SpriteSheet[] originalSheets, SpriteSheet[] newSheets)
		{
			Dictionary<string, List<string>> prefixDirectLookup = new Dictionary<string, List<string>>();
			List<string> order = new List<string>();

			originalSheets = originalSheets ?? new SpriteSheet[0];
			newSheets = newSheets ?? new SpriteSheet[0];

			foreach (SpriteSheet sheet in originalSheets.Concat<SpriteSheet>(newSheets))
			{
				if (sheet.Id == null)
				{
					throw new InvalidOperationException("Sprite sheet is missing an ID.");
				}

				if (!prefixDirectLookup.ContainsKey(sheet.Id))
				{
					prefixDirectLookup.Add(sheet.Id, new List<string>());
					order.Add(sheet.Id);
				}
				prefixDirectLookup[sheet.Id].AddRange(sheet.Prefixes);
			}

			return order
				.Select<string, SpriteSheet>(
					id => new SpriteSheet()
					{
						Id = id,
						Prefixes = prefixDirectLookup[id].ToArray()
					})
				.ToArray();
		}
	}
}
