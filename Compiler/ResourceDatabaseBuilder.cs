﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace Crayon
{
    class ResourceDatabaseBuilder
    {
        private static HashSet<string> IGNORABLE_FILES = new HashSet<string>(new string[] {
            ".ds_store",
            "thumbs.db",
        });

        public static ResourceDatabase CreateResourceDatabase(BuildContext buildContext)
        {
            using (new PerformanceSection("ResourceDatabaseBuilder.CreateResourceDatabase"))
            {
                ResourceDatabase resDb = new ResourceDatabase();

                foreach (FilePath sourceRoot in buildContext.SourceFolders)
                {
                    string[] relativePaths = FileUtil.GetAllFilePathsRelativeToRoot(sourceRoot.AbsolutePath);

                    // Everything is just a basic copy resource at first.
                    foreach (string relativeFilePath in relativePaths)
                    {
                        string absolutePath = FileUtil.GetCanonicalizeUniversalPath(sourceRoot.AbsolutePath + "/" + relativeFilePath);
                        string aliasedPath = sourceRoot.GetAliasedOrRelativePathh(absolutePath);
                        string fileName = System.IO.Path.GetFileName(absolutePath);
                        string extension = FileUtil.GetCanonicalExtension(fileName) ?? "";

                        ResourceDatabase.FileCategory category;
                        if (IGNORABLE_FILES.Contains(fileName.ToLowerInvariant()))
                        {
                            // Common system generated files that no one would ever want.
                            category = ResourceDatabase.FileCategory.IGNORE_SILENT;
                        }
                        else if (ResourceDatabase.KNOWN_FILE_EXTENSIONS.ContainsKey(extension))
                        {
                            category = ResourceDatabase.KNOWN_FILE_EXTENSIONS[extension];
                        }
                        else
                        {
                            TODO.BuildFileShouldIndicateWhichResourcesAreTextVsBinary();
                            category = ResourceDatabase.FileCategory.TEXT;
                        }

                        switch (category)
                        {
                            case ResourceDatabase.FileCategory.IGNORE_SILENT:
                                break;

                            case ResourceDatabase.FileCategory.IGNORE_IMAGE:
                                System.Console.WriteLine(aliasedPath + " is not a usable image type and is being ignored. Consider converting to PNG or JPEG.");
                                break;
                            case ResourceDatabase.FileCategory.IGNORE_AUDIO:
                                System.Console.WriteLine(aliasedPath + " is not a usable audio format and is being ignored. Consider converting to OGG.");
                                break;
                            case ResourceDatabase.FileCategory.IGNORE_IMAGE_ASSET:
                                System.Console.WriteLine(aliasedPath + " is an image asset container file type and is being ignored. Consider moving original assets outside of the source folder.");
                                break;

                            case ResourceDatabase.FileCategory.AUDIO:
                                resDb.AudioResources.Add(new FileOutput()
                                {
                                    Type = FileOutputType.Copy,
                                    RelativeInputPath = aliasedPath,
                                    OriginalPath = aliasedPath,
                                    AbsoluteInputPath = absolutePath,
                                });
                                break;

                            case ResourceDatabase.FileCategory.BINARY:
                                resDb.AudioResources.Add(new FileOutput()
                                {
                                    Type = FileOutputType.Copy,
                                    RelativeInputPath = aliasedPath,
                                    OriginalPath = aliasedPath,
                                    AbsoluteInputPath = absolutePath,
                                });
                                break;

                            case ResourceDatabase.FileCategory.TEXT:
                                string content = FileUtil.ReadFileText(absolutePath);
                                resDb.TextResources.Add(new FileOutput()
                                {
                                    Type = FileOutputType.Text,
                                    TextContent = content,
                                    OriginalPath = aliasedPath,
                                    AbsoluteInputPath = absolutePath,
                                });
                                break;

                            case ResourceDatabase.FileCategory.IMAGE:
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
                                        Bitmap = new SystemBitmap(absolutePath),
                                        OriginalPath = aliasedPath,
                                        AbsoluteInputPath = absolutePath,
                                    });
                                }
                                else if (extension == "jpg" || extension == "jpeg")
                                {
                                    resDb.ImageResources.Add(new FileOutput()
                                    {
                                        Type = FileOutputType.Image,
                                        Bitmap = new SystemBitmap(absolutePath),
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
                                        Bitmap = new SystemBitmap(absolutePath),
                                        RelativeInputPath = aliasedPath,
                                        OriginalPath = aliasedPath,
                                        AbsoluteInputPath = absolutePath,
                                    });
                                }
                                break;

                            case ResourceDatabase.FileCategory.FONT:
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
}
