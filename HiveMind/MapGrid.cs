using System;
using System.Drawing;
using SC2APIProtocol;
using Point = SC2APIProtocol.Point;

namespace HiveMind
{
    public class MapGrid
    {
        private BasePlanner _basePlanner;
        public Ground[,] PlayableGrid;
        public MapGrid(ImageData placementGrid, ImageData pathingGrid, RectangleI playableArea)
        {
            PlayableGrid = new Ground[playableArea.P1.X, playableArea.P1.Y];

            for (var x = playableArea.P0.X; x < playableArea.P1.X; x++)
            {
                for (var y = playableArea.P0.Y; y < playableArea.P1.Y; y++)
                {
                    var isPath = GetDataValueBit(pathingGrid, x, y);
                    var isPlace = GetDataValueBit(placementGrid, x, y);

                    PlayableGrid[x, y] = isPlace == 1 ? Ground.BuildingPlacable : (Ground)isPath;
                }
            }

        }
        // 0 0 0 0 0 5px
        // 0 0 0 x 0  x 2px, 0 based position(3,1) is 8th byte
        public static int GetDataValueBit(ImageData data, int x, int y)
        {
            int pixelID = x + y * data.Size.X;  //8
            int byteLocation = pixelID / 8;  // 1 (2nd byte): 1bit per pixel, 8bits per byte
            int bitLocation = pixelID % 8;  // 0 (1st bit)
            int bit = data.Data[byteLocation] & (1 << (7 - bitLocation)); // Test binary rep of byte(128=1 0 0 0 0 0 0 0) is same as binary rep of (1 left shift 7 bits to get to first bit 1 0 0 0 0 0 0 0, which = 1)
            return bit == 0 ? 0 : 1;
        }

        public Point2D GetAvailableMainBaseDiamond()
        {
            var top = 0;
            var left = 0;
            var right = 0;
            var bottom = 0;

            for (int x = 0; x < PlayableGrid.GetUpperBound(0); x++)
            {
                for (int y = 0; y < PlayableGrid.GetUpperBound(1); y++)
                {
                    if (PlayableGrid[x, y] == Ground.BuildingPlacable)
                    {
                        top = x + 6;
                        left = y - 3;
                        right = y + 3;
                        bottom = x;

                        for (int i = bottom; i < top; i++)
                        {
                            for (int j = left; j < right; j++)
                            {
                                if (PlayableGrid[x, y] != Ground.BuildingPlacable)
                                {
                                    goto here;
                                }
                            }
                        }
                        return new Point2D { X = x, Y = y };
                    }
                    here:;
                }
            }
            return null;
        }

        public RectangleF SetBasePlateau(Point2D startRawStartLocation)
        {
            return new RectangleF(startRawStartLocation.X, startRawStartLocation.Y, 10, 10);
        }
    }

    public enum Ground
    {
        Airspace,
        Pathable,
        BuildingPlacable
    }
}