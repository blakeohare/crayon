using System.Collections.Generic;
using System.Linq;
using Common;

namespace Common.ImageSheets
{
    public class Chunk
    {
        public Chunk()
        {
            this.Members = new List<Image>();
            this.PleaseTileMe = false;
        }

        public List<Image> Members { get; set; }
        public List<Tile> Tiles { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsJPEG { get; set; }
        public bool PleaseTileMe { get; set; }

        public void GenerateTiles()
        {
            if (this.IsJPEG)
            {
                return;
            }

            if (this.Width != 1024 || this.Height != 1024)
            {
                return;
            }

            Tile[] tiles = new Tile[16];
            SystemBitmap.Graphics[] graphics = new SystemBitmap.Graphics[16];
            foreach (Image member in this.Members)
            {
                int startX = member.ChunkX;
                int startY = member.ChunkY;
                int endX = startX + member.Width;
                int endY = startY + member.Height;
                int tileStartX = startX / 256;
                int tileStartY = startY / 256;
                int tileEndX = (endX - 1) / 256;
                int tileEndY = (endY - 1) / 256;

                for (int tileY = tileStartY; tileY <= tileEndY; ++tileY)
                {
                    for (int tileX = tileStartX; tileX <= tileEndX; ++tileX)
                    {
                        int tileIndex = tileX + tileY * 4;
                        SystemBitmap.Graphics tile = graphics[tileIndex];
                        if (tile == null)
                        {
                            tiles[tileIndex] = new Tile()
                            {
                                Bitmap = new SystemBitmap(256, 256),
                                ChunkX = 256 * tileX,
                                ChunkY = 256 * tileY,
                                Width = 256,
                                Height = 256,
                            };
                            tile = tiles[tileIndex].Bitmap.MakeGraphics();
                            graphics[tileIndex] = tile;
                        }

                        tile.Blit(member.Bitmap, member.ChunkX - tileX * 256, member.ChunkY - tileY * 256);
                    }
                }

                this.Tiles = new List<Tile>(tiles.Where(t => t != null));
            }

            foreach (SystemBitmap.Graphics graphicsInstance in graphics)
            {
                if (graphicsInstance != null)
                {
                    graphicsInstance.Cleanup();
                }
            }
        }
    }
}
