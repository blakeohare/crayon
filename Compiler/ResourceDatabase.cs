using System;
using System.Collections.Generic;

namespace Crayon
{
    /*
     * Database of non-code files to be copied.
     */
    class ResourceDatabase
    {
        private static Dictionary<string, FileCategory> KNOWN_FILE_EXTENSIONS = new Dictionary<string, FileCategory>() {

            { "cry", FileCategory.IGNORE_SILENT }, // Not interested in source code.

            { "ogg", FileCategory.AUDIO },

            { "jpg", FileCategory.IMAGE },
            { "jpeg", FileCategory.IMAGE },
            { "png", FileCategory.IMAGE },

            { "fon", FileCategory.FONT },
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

        private static HashSet<string> IGNORABLE_FILES = new HashSet<string>(new string[] {
            ".ds_store",
            "thumbs.db",
        });

        public FileOutput ByteCodeFile { get; set; }
        public ByteBuffer ByteCodeRawData { get; set; }
        public FileOutput ResourceManifestFile { get; set; }
        public FileOutput ImageSheetManifestFile { get; set; }

        public Dictionary<string, FileOutput> ImageSheetFiles { get; set; }
        public List<FileOutput> FontSheetFiles { get; set; }

        public List<FileOutput> AudioResources { get; set; }
        public List<FileOutput> ImageResources { get; set; }
        public List<FileOutput> TextResources { get; set; }
        public List<FileOutput> BinaryResources { get; set; }
        public List<FileOutput> FontResources { get; set; }

        private enum FileCategory
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

        public ResourceDatabase(ICollection<string> files, string sourceRoot)
        {
            this.AudioResources = new List<FileOutput>();
            this.BinaryResources = new List<FileOutput>();
            this.ImageResources = new List<FileOutput>();
            this.TextResources = new List<FileOutput>();
            this.FontResources = new List<FileOutput>();
            this.ImageSheetFiles = new Dictionary<string, FileOutput>();
            this.FontSheetFiles = new List<FileOutput>();

            // Everything is just a basic copy resource at first.
            foreach (string originalRawFilepath in files)
            {
                string originalFilepath = originalRawFilepath.Replace('\\', '/');
                string extension = FileUtil.GetCanonicalExtension(originalFilepath) ?? "";

                FileCategory category;
                if (IGNORABLE_FILES.Contains(System.IO.Path.GetFileName(originalFilepath).ToLowerInvariant()))
                {
                    // Common system generated files that no one would ever want.
                    category = FileCategory.IGNORE_SILENT;
                }
                else
                {
                    if (KNOWN_FILE_EXTENSIONS.ContainsKey(extension))
                    {
                        category = KNOWN_FILE_EXTENSIONS[extension];
                    }
                    else
                    {
                        // TODO: build file should define which files are binary resources and which are text.
                        category = FileCategory.TEXT;
                    }
                }

                switch (category)
                {
                    case FileCategory.IGNORE_SILENT:
                        break;

                    case FileCategory.IGNORE_IMAGE:
                        System.Console.WriteLine(originalFilepath + " is not a usable image type and is being ignored. Consider converting to PNG or JPEG.");
                        break;
                    case FileCategory.IGNORE_AUDIO:
                        System.Console.WriteLine(originalFilepath + " is not a usable audio format and is being ignored. Consider converting to OGG.");
                        break;
                    case FileCategory.IGNORE_IMAGE_ASSET:
                        System.Console.WriteLine(originalFilepath + " is an image asset container file type and is being ignored. Consider moving original assets outside of the source folder.");
                        break;

                    case FileCategory.AUDIO:
                        this.AudioResources.Add(new FileOutput()
                        {
                            Type = FileOutputType.Copy,
                            RelativeInputPath = originalFilepath,
                            OriginalPath = originalFilepath,
                        });
                        break;

                    case FileCategory.BINARY:
                        this.AudioResources.Add(new FileOutput()
                        {
                            Type = FileOutputType.Copy,
                            RelativeInputPath = originalFilepath,
                            OriginalPath = originalFilepath,
                        });
                        break;

                    case FileCategory.TEXT:
                        string content = FileUtil.ReadFileText(FileUtil.JoinPath(sourceRoot, originalFilepath));
                        this.TextResources.Add(new FileOutput()
                        {
                            Type = FileOutputType.Text,
                            TextContent = content,
                            OriginalPath = originalFilepath,
                        });
                        break;

                    case FileCategory.IMAGE:
                        if (extension == "png")
                        {
                            // Re-encode PNGs into a common format/palette configuration since there are some issues
                            // with obscure format PNGs on some platforms. Luckily the compiler is pretty good with
                            // reading these. Besides, you're going to be opening most of these files anyway since
                            // the user should be using image sheets.
                            this.ImageResources.Add(new FileOutput()
                            {
                                Type = FileOutputType.Image,
                                Bitmap = new SystemBitmap(FileUtil.JoinPath(sourceRoot, originalFilepath)),
                                OriginalPath = originalFilepath,
                            });
                        }
                        else
                        {
                            // don't re-encode JPEGs
                            this.ImageResources.Add(new FileOutput()
                            {
                                Type = FileOutputType.Copy,
                                RelativeInputPath = FileUtil.JoinPath(sourceRoot, originalFilepath),
                                OriginalPath = originalFilepath,
                            });
                        }
                        break;

                    case FileCategory.FONT:
                        this.FontResources.Add(new FileOutput()
                        {
                            Type = FileOutputType.Binary,
                            OriginalPath = originalFilepath,
                        });
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
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
                    if (chunk.IsJPEG)
                    {
                        jpegChunks.Add(chunk);
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
                            tile.GeneratedFilename = "t" + (tileCounter++) + (chunk.IsJPEG ? ".jpg" : ".png");
                            manifest.Add("T," + tile.GeneratedFilename + "," + tile.ChunkX + "," + tile.ChunkY + "," + tile.Width + "," + tile.Height + "," + tile.Bytes);

                            if (tile.IsDirectCopy)
                            {
                                this.ImageSheetFiles[tile.GeneratedFilename] = tile.OriginalFile;
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
                    manifest.Add("J," + jpegTile.GeneratedFilename + "," + jpegTile.Width + "," + jpegTile.Height + "," + jpegTile.Bytes + "," + jpegTile.OriginalFile.RelativeInputPath);
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
                    bool isPng = imageFile.OriginalPath.ToLower().EndsWith(".png");
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
                manifest.Add("SND," + audioFile.OriginalPath + "," + audioFile.CanonicalFileName);
            }

            this.ResourceManifestFile = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n", manifest),
            };
        }
    }
}
