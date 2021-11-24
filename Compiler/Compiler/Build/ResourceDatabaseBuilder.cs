using Build.ImageSheets;
using Common;
using CommonUtil.Disk;
using CommonUtil.Images;
using System;
using System.Collections.Generic;
using Wax;

namespace Build
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
            FONT,

            IGNORE_SILENT,
            IGNORE_AUDIO,
            IGNORE_IMAGE,
            IGNORE_IMAGE_ASSET,
        }

        private static Dictionary<string, FileCategory> KNOWN_FILE_EXTENSIONS = new Dictionary<string, FileCategory>() {

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

        public static ResourceDatabase PrepareResources(BuildContext buildContext)
        {
            // This really needs to go in a separate helper file.
            ResourceDatabase resourceDatabase = CreateResourceDatabase(buildContext);

            resourceDatabase.GenerateResourceMapping();

            ImageResourceAllocator.PrepareImageResources(resourceDatabase);

            return resourceDatabase;
        }

        public static ResourceDatabase CreateResourceDatabase(BuildContext buildContext)
        {
            ResourceDatabase resDb = new ResourceDatabase();

            foreach (FilePath sourceRoot in buildContext.SourceFolders)
            {
                string[] relativePaths = FileUtil.GetAllFilePathsRelativeToRoot(sourceRoot.AbsolutePath);

                // Everything is just a basic copy resource at first.
                foreach (string relativeFilePath in relativePaths)
                {
                    string absolutePath = FileUtil.GetCanonicalizeUniversalPath(sourceRoot.AbsolutePath + "/" + relativeFilePath);
                    string aliasedPath = sourceRoot.GetAliasedOrRelativePath(absolutePath);
                    string fileName = Path.GetFileName(absolutePath);
                    string extension = FileUtil.GetCanonicalExtension(fileName) ?? "";

                    FileCategory category;
                    if (IGNORABLE_FILES.Contains(fileName.ToLowerInvariant()))
                    {
                        // Common system generated files that no one would ever want.
                        category = FileCategory.IGNORE_SILENT;
                    }
                    else if (KNOWN_FILE_EXTENSIONS.ContainsKey(extension))
                    {
                        category = KNOWN_FILE_EXTENSIONS[extension];
                    }
                    else
                    {
                        TODO.BuildFileShouldIndicateWhichResourcesAreTextVsBinary();
                        category = FileCategory.TEXT;
                    }

                    switch (category)
                    {
                        case FileCategory.IGNORE_SILENT:
                            break;

                        case FileCategory.IGNORE_IMAGE:
                            ConsoleWriter.Print(
                                ConsoleMessageType.BUILD_WARNING,
                                aliasedPath + " is not a usable image type and is being ignored. Consider converting to PNG or JPEG.");
                            break;

                        case FileCategory.IGNORE_AUDIO:
                            ConsoleWriter.Print(
                                ConsoleMessageType.BUILD_WARNING,
                                aliasedPath + " is not a usable audio format and is being ignored. Consider converting to OGG.");
                            break;

                        case FileCategory.IGNORE_IMAGE_ASSET:
                            ConsoleWriter.Print(
                                ConsoleMessageType.BUILD_WARNING,
                                aliasedPath + " is an image asset container file type and is being ignored. Consider moving original assets outside of the source folder.");
                            break;

                        case FileCategory.AUDIO:
                            resDb.AudioResources.Add(new FileOutput()
                            {
                                Type = FileOutputType.Copy,
                                RelativeInputPath = aliasedPath,
                                OriginalPath = aliasedPath,
                                AbsoluteInputPath = absolutePath,
                            });
                            break;

                        case FileCategory.BINARY:
                            resDb.AudioResources.Add(new FileOutput()
                            {
                                Type = FileOutputType.Copy,
                                RelativeInputPath = aliasedPath,
                                OriginalPath = aliasedPath,
                                AbsoluteInputPath = absolutePath,
                            });
                            break;

                        case FileCategory.TEXT:
                            string content = FileUtil.ReadFileText(absolutePath);
                            resDb.TextResources.Add(new FileOutput()
                            {
                                Type = FileOutputType.Text,
                                TextContent = content,
                                OriginalPath = aliasedPath,
                                AbsoluteInputPath = absolutePath,
                            });
                            break;

                        case FileCategory.IMAGE:
                            TODO.GetImageDimensionsFromFirstFewBytesInsteadOfLoadingIntoMemory();

                            if (extension == "png")
                            {
                                // Re-encode PNGs into a common format/palette configuration since there are some issues
                                // with obscure format PNGs on some platforms. Luckily the compiler is pretty good with
                                // reading these. Besides, you're going to be opening most of these files anyway since
                                // the user should be using image sheets.
                                resDb.ImageResources.Add(new FileOutput()
                                {
                                    Type = FileOutputType.Image,
                                    Bitmap = new Bitmap(absolutePath),
                                    OriginalPath = aliasedPath,
                                    AbsoluteInputPath = absolutePath,
                                });
                            }
                            else if (extension == "jpg" || extension == "jpeg")
                            {
                                resDb.ImageResources.Add(new FileOutput()
                                {
                                    Type = FileOutputType.Image,
                                    Bitmap = new Bitmap(absolutePath),
                                    OriginalPath = aliasedPath,
                                    AbsoluteInputPath = absolutePath,
                                    IsLossy = true,
                                });
                            }
                            else
                            {
                                TODO.PutImageWidthAndHeightIntoFileOutputPropertiesSoThatBitmapDoesntNeedToBePersistedInMemory();

                                resDb.ImageResources.Add(new FileOutput()
                                {
                                    Type = FileOutputType.Copy,
                                    Bitmap = new Bitmap(absolutePath),
                                    RelativeInputPath = aliasedPath,
                                    OriginalPath = aliasedPath,
                                    AbsoluteInputPath = absolutePath,
                                });
                            }
                            break;

                        case FileCategory.FONT:
                            resDb.FontResources.Add(new FileOutput()
                            {
                                Type = FileOutputType.Copy,
                                RelativeInputPath = aliasedPath,
                                OriginalPath = aliasedPath,
                                AbsoluteInputPath = absolutePath,
                            });
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }
            }
            return resDb;
        }
    }
}
