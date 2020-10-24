using Common;
using CommonUtil.Images;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Build.ImageSheets
{
    internal static class ImageResourceAllocator
    {
        private static Image2[] GetAllImages(ResourceDatabase resDB)
        {
            // TODO: this needs to be adapted to run across all dependencies

            return resDB.ImageResources
                .Select(file =>
                {
                    string fullpath = "@:" + file.OriginalPath;
                    string lowerPath = fullpath.ToLowerInvariant();
                    return new Image2()
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

        private static IList<Chunk2> GetChunksForBigImages(IList<Image2> bigImages)
        {
            List<Chunk2> chunks = new List<Chunk2>();
            foreach (Image2 bigImage in bigImages)
            {
                Chunk2 chunk = new Chunk2(bigImage.Width, bigImage.Height);
                bool success = chunk.AttemptAllocation(new Image2[] { bigImage });
                if (!success) throw new Exception();
                chunks.Add(chunk);
            }
            return chunks;
        }

        private static IList<Chunk2> GetChunksForSkinnyImages(IList<Image2> skinnyImages, bool isWide)
        {
            int currentSize = 0;
            List<Image2> currentGroup = new List<Image2>();
            List<List<Image2>> groups = new List<List<Image2>>();
            groups.Add(currentGroup);
            foreach (Image2 skinnyImage in skinnyImages)
            {
                int size = isWide ? skinnyImage.Height : skinnyImage.Width;
                int nextSize = currentSize + size;
                if (nextSize > 1024)
                {
                    currentGroup = new List<Image2>() { skinnyImage };
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
                    Chunk2 chunk = new Chunk2(1024, 1024);
                    bool success = chunk.AttemptAllocation(group);
                    if (!success) throw new Exception(); // this shouldn't happen
                    return chunk;
                })
                .ToArray();
        }

        private static IList<Chunk2> GetChunksForSmallImages(IList<Image2> smallImages)
        {
            List<Chunk2> chunks = new List<Chunk2>();

            while (smallImages.Count > 0)
            {
                int passingLowEnd = 1;
                int failingHighEnd = smallImages.Count + 1;
                Chunk2 chunk;
                bool success;
                while (passingLowEnd + 1 < failingHighEnd)
                {
                    int mid = (passingLowEnd + failingHighEnd) / 2;
                    chunk = new Chunk2(1024, 1024);
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

                chunk = new Chunk2(1024, 1024);
                success = chunk.AttemptAllocation(smallImages.Take(passingLowEnd));
                smallImages = smallImages.Skip(passingLowEnd).ToList();
                if (!success) throw new Exception(); // this should not happen
                chunks.Add(chunk);
            }
            return chunks;
        }

        private static string ConvertChunksToManifest(IList<Chunk2> chunks)
        {
            string currentModule = "";
            string currentDirectory = null;
            List<string> lines = new List<string>();
            int chunkId = 1;
            foreach (Chunk2 chunk in chunks)
            {
                chunk.ID = chunkId++;
                lines.Add("C," + chunk.ID + "," + chunk.Width + "," + chunk.Height);

                Bitmap chunkBmp = new Bitmap(chunk.Width, chunk.Height);
                Bitmap.Graphics g = chunkBmp.MakeGraphics();
                for (int i = 0; i < chunk.Images.Count; ++i)
                {
                    Image2 image = chunk.Images[i];
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
            List<Image2> wideImages = new List<Image2>();
            List<Image2> tallImages = new List<Image2>();
            List<Image2> bigImages = new List<Image2>();
            List<Image2> smallImages = new List<Image2>();

            foreach (Image2 image in GetAllImages(resDB))
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

            List<Chunk2> chunks = new List<Chunk2>();
            chunks.AddRange(GetChunksForBigImages(bigImages));
            chunks.AddRange(GetChunksForSkinnyImages(wideImages, true));
            chunks.AddRange(GetChunksForSkinnyImages(tallImages, false));
            chunks.AddRange(GetChunksForSmallImages(smallImages));

            string manifest = ConvertChunksToManifest(chunks);
            resDB.Image2ResourceManifestFile = new FileOutput()
            {
                Type = FileOutputType.Text,
                TrimBomIfPresent = true,
                TextContent = manifest,
            };

            Dictionary<string, FileOutput> chunkImages = new Dictionary<string, FileOutput>();
            foreach (Chunk2 chunk in chunks)
            {
                chunkImages["ch_" + chunk.ID + ".png"] = new FileOutput()
                {
                    Type = FileOutputType.Image,
                    Bitmap = chunk.FinalizedBitmap,
                };
            }
            resDB.Image2ResourceFiles = chunkImages;
        }
    }
}
