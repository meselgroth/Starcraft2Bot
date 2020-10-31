using SC2APIProtocol;

namespace HiveMind
{
    public static class GridConverter
    {
        public static Ground[,] ToGroundTypeMatrix(ImageData placementGrid, ImageData pathingGrid, RectangleI playableArea)
        {
            var playableGrid = new Ground[playableArea.P1.X, playableArea.P1.Y];

            for (var x = playableArea.P0.X; x < playableArea.P1.X; x++)
            {
                for (var y = playableArea.P0.Y; y < playableArea.P1.Y; y++)
                {
                    var isPath = GetDataValueBit(pathingGrid, x, y);
                    var isPlace = GetDataValueBit(placementGrid, x, y);

                    playableGrid[x, y] = isPlace == 1 ? Ground.BuildingPlacable : (Ground)isPath;
                }
            }
            return playableGrid;
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
    }
}
