using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Crayon
{
	/*
	 * Sprite sheets are an internal optimization. It is strongly recommended for JavaScript and OpenGL projects.
	 * For JavaScript, it makes downloads go much faster if there are a bunch of small images and for OpenGL it
	 * makes rendering much faster since the sprite sheet is used as a texture and less texture swaps are needed.
	 * Even for other platform types that have lots of small images that would have been read from the hard drive
	 * individual can create a noticeable difference in load times.
	 *
	 * Sprite sheets only work for PNG images.
	 *
	 * Each sprite sheet is assigned a unique ID.
	 * You can have multiple sprite sheets.
	 * The files in each sprite sheet are defined by file path prefixes.
	 *
	 * When an image is sprite-sheeted, you can call $download_sprite_sheet(idOrIdList).
	 * This also makes $download_sprite_sheet_progress(idOrIdList) available which returns a float from 0 to 1. It returns
	 * -1 if one of the IDs passed hasn't had its download initialized yet.
	 *
	 * Sprite sheet downloads are always asynchronous.
	 *
	 * $download_image(key, url) must still be invoked with the original image path. But internally, $download_image
	 * will run instantaneously if the sprite sheet download is already completed. If the sprite sheet download was not
	 * initialized then $download_image will start the sprite sheet download but will not run synchronously on any platform.
	 * If the sprite sheet download was started but not finished, it will not have any effect other than assigning the image
	 * key to that path. $is_image_loaded ought to be used the way it normally is.
	 *
	 * Invoking $download_sprite_sheet multiple times on the same id(s) will not download it again.
	 *
	 * Final sprite sheet images are 1024x1024. If the files in that particular sprite sheet group exceed this amount, then
	 * multiple sprite sheet images are used. These are called spillover sprite sheets. Individual images will not get
	 * partitioned across multiple sprite sheets.
	 * 
	 * If tiling is enabled, this is partitioned into 16 256x256 tiles. This makes downloading
	 * on JavaScript much more manageable and makes the progress bar more reasonable.
	 *
	 * On OpenGL platforms, GL.MAX_TEXTURE_SIZE is used at runtime to combine as many sprite sheets as possible into one massive
	 * image to further eliminate the overhead of switching between texture contexts. The upper bound for this is 256 sprite sheets 
	 * on modern PCs.
	 *
	 * Images that are larger than 256x256 on both dimensions are excluded from the sprite sheet image, and are instead treated
	 * as its own spillover sprite sheet. Images that have a dimension that is larger than 1024 on either side is also treated
	 * as a spillover sprite sheet. If tiling is enabled these larger images are not partitioned.
	 */
	class SpriteSheetBuilder
	{
		private Dictionary<string, List<Image>> imagesById = new Dictionary<string, List<Image>>();
		private List<Image> images = new List<Image>();
		private List<string> spriteGroupIds = new List<string>();
		private Dictionary<string, List<string>> prefixesForId = new Dictionary<string, List<string>>();
		// The following mapping can have missing Bitmaps if no image touched that tile.
		private Dictionary<string, Dictionary<int, Bitmap>> finalTiles = new Dictionary<string, Dictionary<int, Bitmap>>();
		private Dictionary<string, Image> fileFinalDestination = new Dictionary<string, Image>();
		private List<string> finalTileImagePaths = new List<string>();

		private class Image
		{
			public Image(string spriteSheetId, string file, Bitmap bmp)
			{
				this.File = file.Replace('\\', '/');
				this.SheetID = spriteSheetId;
				this.bitmap = bmp;
				this.Width = bmp.Width;
				this.Height = bmp.Height;
				this.Solitary = this.Width > 1024 || this.Height > 1024 || (this.Width > 256 && this.Height > 256);
				if (this.Solitary)
				{
					this.GlobalX = 0;
					this.GlobalY = 0;
				}
			}

			private Bitmap bitmap;

			public string File { get; private set; }

			// User-assigned sprite sheet group ID.
			public string SheetID { get; private set; }

			public Bitmap Bitmap { get { return this.bitmap; } }
			public int Width { get; private set; }
			public int Height { get; private set; }

			// Do not include in the sprite sheet images. However it should be treated as though it belongs 
			// to this sprite sheet ID group.
			public bool Solitary { get; private set; }

			// When these are created they are treated like an image that is 1024 pixels wide and infinite pixels tall.
			// Then the sprite sheet is split into Ceiling(Height / 1024) individual sheets. 
			// Images are NEVER placed in such a way that they would straddle a split.
			// This is why there is a 1024 pixel hard limitation.
			public int GlobalX { get; private set; }
			public int GlobalY { get; private set; }
			public void SetGlobalPosition(int x, int y)
			{
				this.GlobalX = x;
				this.GlobalY = y;
			}

			private int spillOverIdOverride = -1;
			public int SpillOverID
			{
				get
				{
					if (this.spillOverIdOverride != -1) return this.spillOverIdOverride;
					return this.GlobalY / 1024;
				}
				set
				{
					this.spillOverIdOverride = value;
				}
			}

			public int SheetX { get { return this.GlobalX; } }
			public int SheetY { get { return this.GlobalY % 1024; } }

			private int tileIdOverride = -1;
			public int TileID
			{
				get
				{
					if (tileIdOverride != -1) return tileIdOverride;

					int row = this.GlobalY / 256;
					int col = this.GlobalX / 256;
					return col + row * 4;
				}
				set
				{
					this.tileIdOverride = value;
				}
			}

			public int TileX { get { return this.GlobalX % 256; } }
			public int TileY { get { return this.GlobalY % 256; } }
		}

		private BuildContext buildContext;

		public SpriteSheetBuilder(BuildContext buildContext)
		{
			this.buildContext = buildContext;
		}

		// ID's will be checked in the order that they are initially passed in to this function.
		public void AddPrefix(string id, string prefix)
		{
			List<string> prefixes;
			if (!this.prefixesForId.TryGetValue(id, out prefixes))
			{
				prefixes = new List<string>();
				this.prefixesForId.Add(id, prefixes);
				this.spriteGroupIds.Add(id);
			}

			prefixes.Add(prefix);
		}

		public void Generate(string generatedFilesFolder, ICollection<string> allFiles, List<string> outStringArgs, List<int[]> outIntArgs, Dictionary<string, FileOutput> fileOutput, HashSet<string> outFilesInSpriteSheet)
		{
			this.MatchAndCreateFiles(allFiles);
			this.AssignGlobalPositions();

			foreach (string id in this.spriteGroupIds)
			{
				List<Image> images = this.imagesById[id];
				if (images.Count == 0)
				{
					throw new InvalidOperationException("Sprite sheet '" + id + "' had no file matches. Please make sure the sheet prefixes do not contain any typos and point to directories that contain images.");
				}
				Dictionary<int, Bitmap> tiles = new Dictionary<int, Bitmap>();
				this.finalTiles[id] = tiles;
				foreach (Image image in images)
				{
					this.fileFinalDestination[image.File] = image;
				}
				this.CreateTiles(tiles, images);
			}

			Dictionary<string, int> sheetNameToId = this.GenerateManifestAndProduceSheetNameIdMapping(outStringArgs, outIntArgs);

			foreach (string file in this.fileFinalDestination.Keys)
			{
				outFilesInSpriteSheet.Add(file);
			}

			this.GenerateFiles(generatedFilesFolder, sheetNameToId, fileOutput);
		}

		private void GenerateFiles(string generatedFilesFolder, Dictionary<string, int> sheetNameToId, Dictionary<string, FileOutput> fileOutput)
		{
			foreach (string sheetName in sheetNameToId.Keys)
			{
				int sheetId = sheetNameToId[sheetName];
				foreach (int tileId in this.finalTiles[sheetName].Keys)
				{
					Bitmap bitmap = this.finalTiles[sheetName][tileId];
					fileOutput[generatedFilesFolder + "/spritesheets/" + sheetId + "_" + tileId + ".png"] = new FileOutput()
					{
						 Type = FileOutputType.Image,
						 Bitmap = bitmap
					};
				}
			}

			this.finalTileImagePaths.AddRange(fileOutput.Keys.OrderBy<string, string>(key => key.ToLowerInvariant()));
		}

		private Dictionary<string, int> GenerateManifestAndProduceSheetNameIdMapping(List<string> stringArgs, List<int[]> intArgs)
		{
			// The manifest is actually compiled as byte code.
			// There is only one sprite sheet manifest byte code command which has a string argument.
			// However the interpreter has a sub interpreter for each string command
			// Commands:
			//   Sheet declaration:
			//     0, {sprite sheet ID}
			//     string: {sprite sheet name}
			//
			//   Image declaration:
			//     1, {sprite sheet ID}, {tile ID}, {width}, {height}, {tileX}, {tileY}, {0|1 - solitary bit}
			//     string: {file path}

			// Images are saved as the sprite sheet ID (the incrementally allocated one) followed by an underscore followed by Tile ID.
			// These images are saved in a _crayon_gen_files folder.

			Dictionary<string, int> sheetNameToId = new Dictionary<string, int>();

			int id = 0;
			foreach (string name in this.spriteGroupIds)
			{
				sheetNameToId[name] = id;
				intArgs.Add(new int[] { 0, id });
				stringArgs.Add(name);

				foreach (Image image in this.imagesById[name])
				{
					stringArgs.Add(image.File);
					int tileId = image.TileID;
					intArgs.Add(new int[] { 
						1,
						id,
						tileId,
						image.Width,
						image.Height,
						image.TileX,
						image.TileY,
						image.Solitary ? 1 : 0
					});
				}

				++id;
			}

			return sheetNameToId;
		}

		private void CreateTiles(Dictionary<int, Bitmap> tilesById, ICollection<Image> images)
		{
			Dictionary<int, Graphics> graphicsById = new Dictionary<int, Graphics>();
			List<int> tileIds = new List<int>();
			List<int> drawX = new List<int>();
			List<int> drawY = new List<int>();
			List<Image> solitaries = new List<Image>();

			int maxTileId = 0;
			int maxSpillId = -1;
			foreach (Image image in images)
			{
				if (image.Solitary)
				{
					solitaries.Add(image);
				}
				else
				{
					int tileId = image.TileID;
					if (tileId > maxTileId) maxTileId = tileId;
					if (image.SpillOverID > maxSpillId) maxSpillId = image.SpillOverID;

					tileIds.Add(tileId);
					drawX.Add(image.TileX);
					drawY.Add(image.TileY);

					int tileColStart = image.SheetX / 256;
					int tileRowStart = image.SheetY / 256;
					int tileColEnd = (image.SheetX + image.Width - 1) / 256;
					int tileRowEnd = (image.SheetY + image.Height - 1) / 256;

					int rowOffset = 0;
					for (int y = tileRowStart; y <= tileRowEnd; ++y, ++rowOffset)
					{
						int colOffset = 0;
						for (int x = tileColStart; x <= tileColEnd; ++x, ++colOffset)
						{
							int thisTileId = tileId + rowOffset * 4 + colOffset;
							tileIds.Add(thisTileId);
							drawX.Add(image.SheetX - 256 * x);
							drawY.Add(image.SheetY - 256 * y);
						}
					}

					for (int i = 0; i < tileIds.Count; ++i)
					{
						int id = tileIds[i];

						if (!tilesById.ContainsKey(id))
						{
							tilesById[id] = new Bitmap(256, 256, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
							tilesById[id].SetResolution(96, 96);
							graphicsById[id] = Graphics.FromImage(tilesById[id]);
						}
						graphicsById[id].DrawImageUnscaled(image.Bitmap, drawX[i], drawY[i]);
					}

					tileIds.Clear();
					drawX.Clear();
					drawY.Clear();
				}
			}

			// Add solitary images to the end as if they are their own tile/spill sheet
			int solitaryTileId = (maxTileId / 16 + 1) * 16;
			int spillId = maxSpillId + 1;
			foreach (Image solitaryImage in solitaries)
			{
				Bitmap bmp = new Bitmap(solitaryImage.Width, solitaryImage.Height);
				bmp.SetResolution(96, 96);
				Graphics g = Graphics.FromImage(bmp);
				g.DrawImageUnscaled(solitaryImage.Bitmap, 0, 0);
				tilesById[solitaryTileId] = bmp;
				solitaryImage.TileID = solitaryTileId;
				solitaryImage.SpillOverID = spillId++;
				solitaryTileId++;
			}
		}

		private string GetSpriteSheetIdMatch(string filename)
		{
			foreach (string id in this.spriteGroupIds)
			{
				foreach (string prefix in this.prefixesForId[id])
				{
					if (filename.Replace('\\', '/').StartsWith(prefix) || prefix == "*")
					{
						return id;
					}
				}
			}
			return null;
		}

		public void MatchAndCreateFiles(ICollection<string> files)
		{
			foreach (string id in this.spriteGroupIds)
			{
				this.imagesById.Add(id, new List<Image>());
			}

			foreach (string file in files)
			{
				if (file.ToLowerInvariant().EndsWith(".png"))
				{
					string spriteSheetId = this.GetSpriteSheetIdMatch(file);
					if (spriteSheetId != null)
					{
						Bitmap bmp = null;
						try
						{
							bmp = new Bitmap(System.IO.Path.Combine(this.buildContext.SourceFolder, file));
							bmp.SetResolution(96, 96);
						}
						catch (Exception)
						{
							// Nope.
						}

						if (bmp != null)
						{
							this.imagesById[spriteSheetId].Add(new Image(spriteSheetId, file, bmp));
						}
					}
				}
			}
		}

		private void AssignGlobalPositions()
		{
			foreach (string id in this.spriteGroupIds)
			{
				this.AssignCoordinates(this.imagesById[id].Where<Image>(img => !img.Solitary));
			}
		}

		private void AssignCoordinates(IEnumerable<Image> images)
		{
			// Fill a 1024xN space with images.
			// Start in top left and and add images row by row left to right, top to bottom
			// Images are sorted by height.

			int rowMaxY = 0;
			int x = 0;
			int y = 0;
			int bottom = 0;
			foreach (Image image in images.OrderBy<Image, int>(img => img.Height))
			{
				// run out of room on the right? move to next row
				if (x + image.Width >= 1024)
				{
					y = rowMaxY;
					x = 0;
				}

				// does the next image spill into the next sheet?
				// move to the next spill sheet
				bottom = y + image.Height;
				if (bottom % 1024 < y % 1024 && bottom % 1024 != 0)
				{
					y = bottom - bottom % 1024;
					bottom = y + image.Height;
					x = 0;
					rowMaxY = bottom;
				}

				image.SetGlobalPosition(x, y);
				x += image.Width;

				if (bottom > rowMaxY)
				{
					rowMaxY = bottom;
				}
			}
		}

		public string[] FinalPaths
		{
			get { return this.finalTileImagePaths.ToArray(); }
		}
	}
}
