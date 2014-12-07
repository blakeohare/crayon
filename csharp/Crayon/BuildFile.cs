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
		public string[] SpriteSheetIds { get; set; }
		public string JsFilePrefix { get; set; }
		public string Platform { get; set; }
		public bool Minified { get; set; }
		public bool ReadableByteCode { get; set; }

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

			private bool TranslateStringToBoolean(string value) {
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

			flattened.Output = DoReplacement(targetName, desiredTarget.Output ?? flattened.Output);
			flattened.Source = DoReplacement(targetName, desiredTarget.Source ?? flattened.Source);
			flattened.ProjectName = DoReplacement(targetName, desiredTarget.ProjectName ?? flattened.ProjectName);
			flattened.JsFilePrefix = DoReplacement(targetName, desiredTarget.JsFilePrefix ?? flattened.JsFilePrefix);
			flattened.SpriteSheets = MergeSpriteSheets(desiredTarget.SpriteSheets, flattened.SpriteSheets);
			flattened.MinifiedRaw = desiredTarget.MinifiedRaw ?? flattened.MinifiedRaw;
			flattened.ExportDebugByteCodeRaw = desiredTarget.ExportDebugByteCodeRaw ?? flattened.ExportDebugByteCodeRaw;

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
				ReadableByteCode = flattened.ExportDebugByteCode
			};
		}

		private static string DoReplacement(string target, string value)
		{
			return value != null && value.Contains("%TARGET_NAME%")
				? value.Replace("%TARGET_NAME%", target)
				: value;
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
					id => new SpriteSheet() { 
						Id = id, 
						Prefixes = prefixDirectLookup[id].ToArray()
					})
				.ToArray();
		}
	}
}
