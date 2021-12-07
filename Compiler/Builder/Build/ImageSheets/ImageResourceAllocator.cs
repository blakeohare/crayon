using System;
using System.Collections.Generic;
using System.Linq;
using Wax;
using Wax.Util.Images;

namespace Builder.ImageSheets
{
    internal static class ImageResourceAllocator
    {
        private static Image[] GetAllImages(ResourceDatabase resDB)
        {
            // TODO: this needs to be adapted to run across all dependencies

            return resDB.ImageResources
                .Select(file =>
                {
                    string fullpath = "@:" + file.OriginalPath;
                    string lowerPath = fullpath.ToLowerInvariant();
                    return new Image()
                    {
                        Path = fullpath,
                        File = file,
                        Width = file.Bitmap.Width,
                        Height = file.Bitmap.Height,
                        IsJpeg = lowerPath.EndsWith(".jpg") || lowerPath.EndsWith(".jpeg"),
                    };
                })
                .OrderBy(img => img.Path)
                .ToArray();
        }

        private static IList<Chunk> GetChunksForBigImages(IList<Image> bigImages)
        {
            List<Chunk> chunks = new List<Chunk>();
            foreach (Image bigImage in bigImages)
            {
                Chunk chunk = new Chunk(bigImage.Width, bigImage.Height);
                bool success = chunk.AttemptAllocation(new Image[] { bigImage });
                if (!success) throw new Exception();
                chunks.Add(chunk);
            }
            return chunks;
        }

        private static IList<Chunk> GetChunksForSkinnyImages(IList<Image> skinnyImages, bool isWide)
        {
            int currentSize = 0;
            List<Image> currentGroup = new List<Image>();
            List<List<Image>> groups = new List<List<Image>>();
            groups.Add(currentGroup);
            foreach (Image skinnyImage in skinnyImages)
            {
                int size = isWide ? skinnyImage.Height : skinnyImage.Width;
                int nextSize = currentSize + size;
                if (nextSize > 1024)
                {
                    currentGroup = new List<Image>() { skinnyImage };
                    groups.Add(currentGroup);
                    nextSize = size;
                }
                else
                {
                    currentGroup.Add(skinnyImage);
                }
                currentSize = nextSize;
            }

            return groups
                .Where(group => group.Count > 0)
                .Select(group =>
                {
                    Chunk chunk = new Chunk(1024, 1024);
                    bool success = chunk.AttemptAllocation(group);
                    if (!success) throw new Exception(); // this shouldn't happen
                    return chunk;
                })
                .ToArray();
        }

        private static IList<Chunk> GetChunksForSmallImages(IList<Image> smallImages)
        {
            List<Chunk> chunks = new List<Chunk>();

            while (smallImages.Count > 0)
            {
                int passingLowEnd = 1;
                int failingHighEnd = smallImages.Count + 1;
                Chunk chunk;
                bool success;
                while (passingLowEnd + 1 < failingHighEnd)
                {
                    int mid = (passingLowEnd + failingHighEnd) / 2;
                    chunk = new Chunk(1024, 1024);
                    success = chunk.AttemptAllocation(smallImages.Take(mid));
                    if (success)
                    {
                        passingLowEnd = mid;
                    }
                    else
                    {
                        failingHighEnd = mid;
                    }
                }

                chunk = new Chunk(1024, 1024);
                success = chunk.AttemptAllocation(smallImages.Take(passingLowEnd));
                smallImages = smallImages.Skip(passingLowEnd).ToList();
                if (!success) throw new Exception(); // this should not happen
                chunks.Add(chunk);
            }
            return chunks;
        }

        private static string ConvertChunksToManifest(IList<Chunk> chunks)
        {
            string currentModule = "";
            string currentDirectory = null;
            List<string> lines = new List<string>();
            int chunkId = 1;
            foreach (Chunk chunk in chunks)
            {
                chunk.ID = chunkId++;
                lines.Add("C," + chunk.ID + "," + chunk.Width + "," + chunk.Height);

                UniversalBitmap chunkBmp = new UniversalBitmap(chunk.Width, chunk.Height);
                UniversalBitmap.DrawingSession g = chunkBmp.CreateNewDrawingSession();
                for (int i = 0; i < chunk.Images.Count; ++i)
                {
                    Image image = chunk.Images[i];
                    string path = image.File.OriginalPath;
                    string dir;
                    string filename;
                    int lastSlash = path.LastIndexOf('/');
                    if (lastSlash == -1)
                    {
                        dir = "/";
                        filename = path;
                    }
                    else
                    {
                        dir = path.Substring(0, lastSlash);
                        filename = path.Substring(lastSlash + 1);
                    }

                    int x = chunk.X[i];
                    int y = chunk.Y[i];
                    g.Blit(image.File.Bitmap, x, y);
                    if (image.Module != currentModule)
                    {
                        currentModule = image.Module;
                        lines.Add("M," + currentModule);
                    }
                    if (dir != currentDirectory)
                    {
                        currentDirectory = dir;
                        lines.Add("D," + currentDirectory);
                    }
                    lines.Add("F," + x + "," + y + "," + image.Width + "," + image.Height + "," + filename);
                }
                chunk.FinalizedBitmap = chunkBmp;
            }
            return string.Join('\n', lines);
        }

        public static void PrepareImageResources(ResourceDatabase resDB)
        {
            List<Image> wideImages = new List<Image>();
            List<Image> tallImages = new List<Image>();
            List<Image> bigImages = new List<Image>();
            List<Image> smallImages = new List<Image>();

            foreach (Image image in GetAllImages(resDB))
            {
                if (image.IsJpeg)
                {
                    bigImages.Add(image);
                }
                else if (image.Width > 512 && image.Height < 100)
                {
                    wideImages.Add(image);
                }
                else if (image.Height > 512 && image.Width < 100)
                {
                    tallImages.Add(image);
                }
                else if (image.Width * image.Height > 256 * 256)
                {
                    bigImages.Add(image);
                }
                else
                {
                    smallImages.Add(image);
                }
            }

            List<Chunk> chunks = new List<Chunk>();
            chunks.AddRange(GetChunksForBigImages(bigImages));
            chunks.AddRange(GetChunksForSkinnyImages(wideImages, true));
            chunks.AddRange(GetChunksForSkinnyImages(tallImages, false));
            chunks.AddRange(GetChunksForSmallImages(smallImages));

            string manifest = ConvertChunksToManifest(chunks);
            resDB.ImageResourceManifestFile = new FileOutput()
            {
                Type = FileOutputType.Text,
                TrimBomIfPresent = true,
                TextContent = manifest,
            };

            Dictionary<string, FileOutput> chunkImages = new Dictionary<string, FileOutput>();
            foreach (Chunk chunk in chunks)
            {
                chunkImages["ch_" + chunk.ID + ".png"] = new FileOutput()
                {
                    Type = FileOutputType.Image,
                    Bitmap = chunk.FinalizedBitmap,
                };
            }
            resDB.ImageResourceFiles = chunkImages;
        }
    }
}
