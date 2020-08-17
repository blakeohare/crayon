using Common;
using System.Collections.Generic;

namespace Build
{
    /*
     * Database of non-code files to be copied.
     */
    public class ResourceDatabase
    {
        public static Dictionary<string, FileCategory> KNOWN_FILE_EXTENSIONS = new Dictionary<string, FileCategory>() {

            { "cry", FileCategory.IGNORE_SILENT }, // Not interested in source code.

            { "ogg", FileCategory.AUDIO },

            { "jpg", FileCategory.IMAGE },
            { "jpeg", FileCategory.IMAGE },
            { "png", FileCategory.IMAGE },

            { "ttf", FileCategory.FONT },

            { "aac", FileCategory.IGNORE_AUDIO },
            { "aiff", FileCategory.IGNORE_AUDIO },
            { "au", FileCategory.IGNORE_AUDIO },
            { "mid", FileCategory.IGNORE_AUDIO },
            { "mp3", FileCategory.IGNORE_AUDIO },
            { "mpg", FileCategory.IGNORE_AUDIO },
            { "wav", FileCategory.IGNORE_AUDIO },
            { "wma", FileCategory.IGNORE_AUDIO },

            { "bmp", FileCategory.IGNORE_IMAGE },
            { "gif", FileCategory.IGNORE_IMAGE },
            { "ico", FileCategory.IGNORE_IMAGE },
            { "pcx", FileCategory.IGNORE_IMAGE },
            { "ppm", FileCategory.IGNORE_IMAGE },
            { "tga", FileCategory.IGNORE_IMAGE },
            { "tiff", FileCategory.IGNORE_IMAGE },

            { "ai", FileCategory.IGNORE_IMAGE_ASSET },
            { "cpt", FileCategory.IGNORE_IMAGE_ASSET },
            { "psd", FileCategory.IGNORE_IMAGE_ASSET },
            { "psp", FileCategory.IGNORE_IMAGE_ASSET },
            { "svg", FileCategory.IGNORE_IMAGE_ASSET },
            { "xcf", FileCategory.IGNORE_IMAGE_ASSET },
        };

        //public FileOutput ByteCodeFile { get; set; }
        public FileOutput ResourceManifestFile { get; set; }
        public FileOutput ImageSheetManifestFile { get; set; }

        public Dictionary<string, FileOutput> ImageSheetFiles { get; set; }

        public List<FileOutput> AudioResources { get; set; }
        public List<FileOutput> ImageResources { get; set; }
        public List<FileOutput> TextResources { get; set; }
        public List<FileOutput> BinaryResources { get; set; }
        public List<FileOutput> FontResources { get; set; }

        public enum FileCategory
        {
            TEXT,
            BINARY, // Not used yet.
            AUDIO,
            IMAGE,
            FONT,

            IGNORE_SILENT,
            IGNORE_AUDIO,
            IGNORE_IMAGE,
            IGNORE_IMAGE_ASSET,
        }

        public ResourceDatabase()
        {
            this.AudioResources = new List<FileOutput>();
            this.BinaryResources = new List<FileOutput>();
            this.ImageResources = new List<FileOutput>();
            this.TextResources = new List<FileOutput>();
            this.FontResources = new List<FileOutput>();
            this.ImageSheetFiles = new Dictionary<string, FileOutput>();
        }

        // TODO: move into its own helper class in the ImageSheets namespace.
        public void AddImageSheets(IList<ImageSheets.Sheet> sheets)
        {
            int tileCounter = 1;
            List<string> manifest = new List<string>();
            int sheetCounter = 0;
            foreach (ImageSheets.Sheet sheet in sheets)
            {
                manifest.Add("S," + sheetCounter++ + "," + sheet.ID);

                List<ImageSheets.Chunk> jpegChunks = new List<ImageSheets.Chunk>();
                foreach (ImageSheets.Chunk chunk in sheet.Chunks)
                {
                    foreach (ImageSheets.Tile tile in chunk.Tiles)
                    {
                        tile.GeneratedFilename = "t" + (tileCounter++) + (chunk.IsJPEG ? ".jpg" : ".png");
                    }

                    if (chunk.IsJPEG)
                    {
                        jpegChunks.Add(chunk);
                        foreach (ImageSheets.Tile tile in chunk.Tiles)
                        {
                            this.ImageSheetFiles[tile.GeneratedFilename] = new FileOutput()
                            {
                                Type = FileOutputType.Copy,
                                AbsoluteInputPath = tile.OriginalFile.AbsoluteInputPath,
                            };
                        }
                    }
                    else
                    {
                        int width = chunk.Width;
                        int height = chunk.Height;
                        if (width == 1024 && height == 1024)
                        {
                            width = 0;
                            height = 0;
                        }
                        manifest.Add("C," + width + "," + height);
                        foreach (ImageSheets.Tile tile in chunk.Tiles)
                        {
                            manifest.Add("T," + tile.GeneratedFilename + "," + tile.ChunkX + "," + tile.ChunkY + "," + tile.Width + "," + tile.Height + "," + tile.Bytes);

                            if (tile.IsDirectCopy)
                            {
                                this.ImageSheetFiles[tile.GeneratedFilename] = new FileOutput()
                                {
                                    Bitmap = tile.OriginalFile.Bitmap,
                                    Type = FileOutputType.Image,
                                };
                            }
                            else
                            {
                                this.ImageSheetFiles[tile.GeneratedFilename] = new FileOutput()
                                {
                                    Bitmap = tile.Bitmap,
                                    Type = FileOutputType.Image,
                                };
                            }
                        }

                        foreach (ImageSheets.Image image in chunk.Members)
                        {
                            manifest.Add("I," + image.ChunkX + "," + image.ChunkY + "," + image.Width + "," + image.Height + "," + image.OriginalPath);
                        }
                    }
                }

                foreach (ImageSheets.Chunk jpegChunk in jpegChunks)
                {
                    ImageSheets.Tile jpegTile = jpegChunk.Tiles[0];
                    manifest.Add("J," + jpegTile.GeneratedFilename + "," + jpegTile.Width + "," + jpegTile.Height + "," + jpegTile.Bytes + "," + jpegTile.OriginalFile.OriginalPath);
                }
            }

            if (manifest.Count > 0)
            {
                this.ImageSheetManifestFile = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = string.Join("\n", manifest),
                };
            }
        }

        public void GenerateResourceMapping()
        {
            List<string> manifest = new List<string>();

            int i = 1;
            foreach (FileOutput textFile in this.TextResources)
            {
                textFile.CanonicalFileName = "txt" + (i++) + ".txt";
                manifest.Add("TXT," + textFile.OriginalPath + "," + textFile.CanonicalFileName);
            }

            List<string> imageSheetManifestFileAdditions = new List<string>();
            i = 1;
            foreach (FileOutput imageFile in this.ImageResources)
            {
                if (imageFile.Type == FileOutputType.Ghost)
                {
                    manifest.Add("IMGSH," + imageFile.OriginalPath + ",," + imageFile.ImageSheetId);
                }
                else
                {
                    bool isPng = imageFile.OriginalPath.ToLowerInvariant().EndsWith(".png");
                    imageFile.CanonicalFileName = "i" + (i++) + (isPng ? ".png" : ".jpg");
                    manifest.Add("IMG," + imageFile.OriginalPath + "," + imageFile.CanonicalFileName);
                    imageSheetManifestFileAdditions.Add("A," + imageFile.CanonicalFileName + "," + imageFile.Bitmap.Width + "," + imageFile.Bitmap.Height + "," + imageFile.OriginalPath);
                }
            }

            if (imageSheetManifestFileAdditions.Count > 0)
            {
                if (this.ImageSheetManifestFile == null)
                {
                    this.ImageSheetManifestFile = new FileOutput()
                    {
                        Type = FileOutputType.Text,
                        TextContent = string.Join("\n", imageSheetManifestFileAdditions),
                    };
                }
                else
                {
                    this.ImageSheetManifestFile.TextContent += "\n" + string.Join("\n", imageSheetManifestFileAdditions);
                }
            }

            i = 1;
            foreach (FileOutput audioFile in this.AudioResources)
            {
                audioFile.CanonicalFileName = "snd" + (i++) + ".ogg";
                // TODO: swap the order of the original path and the canonical name
                manifest.Add("SND," + audioFile.OriginalPath + "," + audioFile.CanonicalFileName);
            }

            i = 1;
            foreach (FileOutput fontFile in this.FontResources)
            {
                fontFile.CanonicalFileName = "ttf" + (i++) + ".ttf";
                manifest.Add("TTF," + fontFile.OriginalPath + "," + fontFile.CanonicalFileName);
            }

            this.ResourceManifestFile = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n", manifest),
            };
        }

        public void PopulateFileOutputContextForCbx(Dictionary<string, FileOutput> output)
        {
            foreach (FileOutput txtResource in this.TextResources)
            {
                output["res/txt/" + txtResource.CanonicalFileName] = txtResource;
            }
            foreach (FileOutput sndResource in this.AudioResources)
            {
                output["res/snd/" + sndResource.CanonicalFileName] = sndResource;
            }
            foreach (FileOutput fontResource in this.FontResources)
            {
                output["res/ttf/" + fontResource.CanonicalFileName] = fontResource;
            }
            foreach (FileOutput binResource in this.BinaryResources)
            {
                output["res/bin/" + binResource.CanonicalFileName] = binResource;
            }
            foreach (FileOutput imgResource in this.ImageResources)
            {
                output["res/img/" + imgResource.CanonicalFileName] = imgResource;
            }
            foreach (string key in this.ImageSheetFiles.Keys)
            {
                output["res/img/" + key] = this.ImageSheetFiles[key];
            }
        }
    }
}
