using System.Drawing;
using SC2APIProtocol;

namespace HiveMind
{
    public class MapGrid
    {
        private BasePlanner _basePlanner;
        public Ground[,] PlayableGrid; 
        public MapGrid(ImageData placementGrid, ImageData pathingGrid, RectangleI playableArea)
        {
            PlayableGrid = new Ground[playableArea.P1.X,playableArea.P1.Y];

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
        
        public static int GetDataValueBit(ImageData data, int x, int y)
        {
            int pixelID = x + y * data.Size.X;
            int byteLocation = pixelID / 8;
            int bitLocation = pixelID % 8;
            return ((data.Data[byteLocation] & 1 << (7 - bitLocation)) == 0) ? 0 : 1;
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