using Builder.ImageSheets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wax;
using Wax.Util.Disk;
using Wax.Util.Images;

namespace Builder
{
    public static class ResourceDatabaseBuilder
    {
        private static HashSet<string> IGNORABLE_FILES = new HashSet<string>(new string[] {
            ".ds_store",
            "thumbs.db",
        });

        private enum FileCategory
        {
            TEXT,
            BINARY, // Not used yet.
            AUDIO,
            IMAGE,

            IGNORE,
        }

        private static Dictionary<string, FileCategory> KNOWN_FILE_EXTENSIONS = new Dictionary<string, FileCategory>() {

            { "cry", FileCategory.IGNORE },

            { "ogg", FileCategory.AUDIO },

            { "jpg", FileCategory.IMAGE },
            { "jpeg", FileCategory.IMAGE },
            { "png", FileCategory.IMAGE },

            { "txt", FileCategory.TEXT },
            { "xml", FileCategory.TEXT },
            { "json", FileCategory.TEXT },
        };

        internal static async Task<ResourceDatabase> PrepareResources(BuildContext buildContext)
        {
            // This really needs to go in a separate helper file.
            ResourceDatabase resourceDatabase = await CreateResourceDatabase(buildContext);

            GenerateResourceMapping(resourceDatabase);

            ImageResourceAllocator.PrepareImageResources(resourceDatabase);

            return resourceDatabase;
        }

        public static void GenerateResourceMapping(ResourceDatabase resDb)
        {
            // TODO: swap the order of the original path and the canonical name so that commas won't interfere with parsing

            List<string> manifest = new List<string>();
            int resourceId = 1;
            foreach (FileOutput textFile in resDb.TextResources)
            {
                textFile.CanonicalFileName = "txt" + (resourceId++) + ".txt";
                manifest.Add("TXT," + textFile.OriginalPath + "," + textFile.CanonicalFileName);
            }

            foreach (FileOutput imageFile in resDb.ImageResources)
            {
                manifest.Add("IMG," + imageFile.OriginalPath + ","); // images are atlas'd
            }

            foreach (FileOutput audioFile in resDb.AudioResources)
            {
                audioFile.CanonicalFileName = "snd" + (resourceId++) + ".ogg";
                manifest.Add("SND," + audioFile.OriginalPath + "," + audioFile.CanonicalFileName);
            }

            foreach (FileOutput binaryFile in resDb.BinaryResources)
            {
                binaryFile.CanonicalFileName = "bin" + (resourceId++);
                manifest.Add("BIN," + binaryFile.OriginalPath + "," + binaryFile.CanonicalFileName);
            }

            resDb.ResourceManifestFile = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n", manifest),
            };
        }

        internal static async Task<ResourceDatabase> CreateResourceDatabase(BuildContext buildContext)
        {
            ResourceDatabase resDb = new ResourceDatabase();

            List<FileOutput> audioResources = new List<FileOutput>();
            List<FileOutput> imageResources = new List<FileOutput>();
            List<FileOutput> textResources = new List<FileOutput>();
            List<FileOutput> binaryResources = new List<FileOutput>();
            foreach (ProjectFilePath sourceRoot in buildContext.SourceFolders)
            {
                string[] relativePaths = await FileUtil.GetAllFilePathsRelativeToRootAsync(sourceRoot.AbsolutePath);

                // Everything is just a basic copy resource at first.
                foreach (string relativeFilePath in relativePaths)
                {
                    string absolutePath = FileUtil.GetCanonicalizeUniversalPath(sourceRoot.AbsolutePath + "/" + relativeFilePath);
                    string relativePath = sourceRoot.GetRelativePath(absolutePath);
                    string fileName = DiskUtil.GetFileName(absolutePath);
                    string extension = FileUtil.GetCanonicalExtension(fileName) ?? "";

                    FileCategory category;
                    if (IGNORABLE_FILES.Contains(fileName.ToLowerInvariant()))
                    {
                        // Common system generated files that no one would ever want. (thumbs.db, .DS_Store)
                        category = FileCategory.IGNORE;
                    }
                    else if (KNOWN_FILE_EXTENSIONS.ContainsKey(extension))
                    {
                        category = KNOWN_FILE_EXTENSIONS[extension];
                    }
                    else
                    {
                        category = FileCategory.BINARY;
                    }

                    switch (category)
                    {
                        case FileCategory.IGNORE:
                            break;

                        case FileCategory.AUDIO:
                            audioResources.Add(new FileOutput()
                            {
                                Type = FileOutputType.Copy,
                                RelativeInputPath = relativePath,
                                OriginalPath = relativePath,
                                AbsoluteInputPath = absolutePath,
                            });
                            break;

                        case FileCategory.BINARY:
                            binaryResources.Add(new FileOutput()
                            {
                                Type = FileOutputType.Copy,
                                RelativeInputPath = relativePath,
                                OriginalPath = relativePath,
                                AbsoluteInputPath = absolutePath,
                            });
                            break;

                        case FileCategory.TEXT:
                            string content = await buildContext.DiskUtil.FileReadText(absolutePath);
                            textResources.Add(new FileOutput()
                            {
                                Type = FileOutputType.Text,
                                TextContent = content,
                                OriginalPath = relativePath,
                                AbsoluteInputPath = absolutePath,
                            });
                            break;

                        case FileCategory.IMAGE:
                            if (extension == "png")
                            {
                                // Re-encode PNGs into a common format/palette configuration since there are some issues
                                // with obscure format PNGs on some platforms. Luckily the compiler is pretty good with
                                // reading these. Besides, you're going to be opening most of these files anyway since
                                // the user should be using image sheets.
                                imageResources.Add(new FileOutput()
                                {
                                    Type = FileOutputType.Image,
                                    Bitmap = new UniversalBitmap(absolutePath),
                                    OriginalPath = relativePath,
                                    AbsoluteInputPath = absolutePath,
                                });
                            }
                            else if (extension == "jpg" || extension == "jpeg")
                            {
                                imageResources.Add(new FileOutput()
                                {
                                    Type = FileOutputType.Image,
                                    Bitmap = new UniversalBitmap(absolutePath),
                                    OriginalPath = relativePath,
                                    AbsoluteInputPath = absolutePath,
                                    IsLossy = true,
                                });
                            }
                            else
                            {
                                imageResources.Add(new FileOutput()
                                {
                                    Type = FileOutputType.Copy,
                                    Bitmap = new UniversalBitmap(absolutePath),
                                    RelativeInputPath = relativePath,
                                    OriginalPath = relativePath,
                                    AbsoluteInputPath = absolutePath,
                                });
                            }
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            resDb.ImageResources = imageResources.ToArray();
            resDb.AudioResources = audioResources.ToArray();
            resDb.TextResources = textResources.ToArray();
            resDb.BinaryResources = binaryResources.ToArray();

            return resDb;
        }
    }
}
