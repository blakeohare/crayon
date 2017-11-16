using System.Collections.Generic;
using System.Linq;

namespace Common.ImageSheets
{
    public static class RectangleAllocator
    {
        public static Chunk[] Allocate(IEnumerable<Image> images)
        {
            List<Image> tileableImages = new List<Image>();
            List<Chunk> solitaryChunks = new List<Chunk>();
            List<Chunk> tiledChunks = new List<Chunk>();

            // First figure out what needs to go into their own chunks i.e. JPEGs, really big images, etc.

            foreach (Image image in images)
            {
                string path = image.OriginalPath.ToLower();
                if (path.EndsWith(".jpg") || path.EndsWith(".jpeg"))
                {
                    solitaryChunks.Add(BuildChunkWithSingleImage(image, true));
                }
                else
                {
                    // Too big? Might kill the packing algorithm.
                    bool isTooBig = image.Width > 1024 || image.Height > 1024; // Tiled chunks are 1024x1024 so this won't work at all.
                    isTooBig = isTooBig || (image.Width * image.Height >= 256 * 256); // If it's bigger than a single tile, then it's not worth trying to incorporate into the packing algorithm.

                    if (isTooBig)
                    {
                        solitaryChunks.Add(BuildChunkWithSingleImage(image, false));
                    }
                    else
                    {
                        tileableImages.Add(image);
                    }
                }
            }

            if (tileableImages.Count > 0)
            {
                /*
                 * This algorithm simply allocates images to the top left corner of a 1024x1024 rectangle
                 * and goes sequentially across the top.
                 *
                 * Images are sorted by height (with filename as tie-breaker for deterministic behavior)
                 *
                 * Once it reaches the right side, it starts over at the y coordinate of the bottom of the
                 * first image.
                 *
                 * Once it runs out of room on the 1024x1024 chunk, it creates a new chunk.
                 *
                 * The chunk will attempt to tile itself into a 4x4 square of 256 x 256 pixel images each.
                 * As images are being added to the chunk, this 4x4 grid marks each tile as dirty when used.
                 * This will prevent empty tiles from being generated as images.
                 *
                 * TODO: tiles can be arbitrary size.
                 * Create some sort of threshold where tiles can be combined.
                 * Add a flag to disable tiling (i.e. 1 chunk = 1 tile) for non-JS platoforms.
                 */
                Image[] sortedImages = tileableImages.OrderBy(img => GetSortKey(img)).ToArray();
                Chunk currentChunk = null;

                int x = 0;
                int topY = 0;
                int bottomY = 0;
                for (int i = 0; i < sortedImages.Length; ++i)
                {
                    Image currentImage = sortedImages[i];
                    if (currentChunk == null)
                    {
                        currentChunk = new Chunk() { IsJPEG = false, Width = 1024, Height = 1024 };
                        tiledChunks.Add(currentChunk);
                        x = 0;
                        topY = 0;
                        bottomY = 0;
                    }

                    int right = x + currentImage.Width;
                    if (right > 1024)
                    {
                        // move to the next row.
                        x = 0;
                        var temp = topY;
                        topY = bottomY;
                        bottomY = temp;
                    }

                    bottomY = System.Math.Max(bottomY, topY + currentImage.Height);

                    if (bottomY > 1024)
                    {
                        // move to the next chunk.
                        --i;
                        currentChunk = null;
                        continue;
                    }

                    currentChunk.Members.Add(currentImage);
                    currentImage.ChunkX = x;
                    currentImage.ChunkY = topY;
                    x += currentImage.Width;
                }

                foreach (Chunk chunk in tiledChunks)
                {
                    chunk.GenerateTiles();
                }
            }

            foreach (Chunk chunk in solitaryChunks)
            {
                chunk.GenerateTiles();
            }

            tiledChunks.AddRange(solitaryChunks);
            return tiledChunks.ToArray();
        }

        private static Chunk BuildChunkWithSingleImage(Image image, bool isJpeg)
        {
            return new Chunk()
            {
                IsJPEG = isJpeg,
                Members = new List<Image>() { image },
                Width = image.Width,
                Height = image.Height,
                Tiles = new List<Tile>() {
                    new Tile() {
                        IsDirectCopy = true,
                        OriginalFile = image.OriginalFile,
                        Bitmap = image.Bitmap,
                        ChunkX = 0,
                        ChunkY = 0,
                        Bytes = 42, // TODO: bytes
                        Width = image.Width,
                        Height = image.Height,
                    }
                }
            };
        }

        private static string GetSortKey(Image image)
        {
            string key = (1024 - image.Height).ToString();
            while (key.Length < 10)
            {
                key = "0" + key;
            }

            key += ":" + image.OriginalPath;
            return key;
        }
    }
}
