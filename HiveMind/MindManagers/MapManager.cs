using SC2APIProtocol;
using System;
using System.Linq;
using Point = SC2APIProtocol.Point;

namespace HiveMind
{
    public class MapManager
    {
        // private BasePlanner _basePlanner; //TODO see here for path within main base
        public Ground[,] MapGrid; // [x,y]
        private readonly Point _startLocation = new Point { X = 0, Y = 0, Z = 0 };

        public MapManager(Ground[,] mapGrid, Point mainBaseCenterLocation = null)
        {
            MapGrid = mapGrid;
            if (mainBaseCenterLocation != null)
            {
                _startLocation = mainBaseCenterLocation;
                // Store location of mainbase (5x5 size)
                StoreNewBuilding(mainBaseCenterLocation, 5, 5);
            }
        }

        public void StoreNewBuilding(Point location, double width, double height)
        {
            StoreNewBuilding(location.X, location.Y, width, height);
        }
        public void StoreNewBuilding(Point2D location, double width, double height)
        {
            StoreNewBuilding(location.X, location.Y, width, height);
        }

        private void StoreNewBuilding(float locationX, float locationY, double width, double height)
        {
            // (x,y) is center of building
            for (int y = (int)locationY - (int)(height / 2); y < locationY + (height / 2); y++)
            {
                for (int x = (int)locationX - (int)(width / 2); x < locationX + (width / 2); x++)
                {
                    MapGrid[x, y] = Ground.Building;
                }
            }
            //for (double x = -width / 2; x < width / 2; x++)
            //{
            //    for (double y = -height / 2; y < height / 2; y++)
            //    {
            //        MapGrid[(int)(locationX + x), (int)(locationY + y)] = Ground.Building;
            //    }
            //}
        }

        public ulong GetMineralInMainBase()
        {
            var mineral = Game.ResponseObservation.Observation.RawData.Units.FirstOrDefault(
                u => u.MineralContents > 0
                    && u.Pos.X < _startLocation.X + 20 && u.Pos.X > _startLocation.X - 20
                    && u.Pos.Y < _startLocation.Y + 20 && u.Pos.Y > _startLocation.Y - 20);
            if (mineral != null)
            {
                return mineral.Tag;
            }
            return null;
        }

        public Point2D GetAvailableDiamondInMainBase(int width, int height)
        {
            var xChange = -1;
            if (_startLocation.X < MapGrid.GetUpperBound(1) / 2)
            {
                // Base is left side of map, build to the right
                xChange = 1;
            }
            var yChange = -1;
            if (_startLocation.Y < MapGrid.GetUpperBound(0) / 2)
            {
                // Base is bottom side of map, build to the top (Y axis starts at bottom of map)
                yChange = 1;
            }

            float mainBaseRightBorder = _startLocation.X + 15; // TODO: Find real border when initialising MapManager
            float mainBaseLeftBorder = _startLocation.X - 15;
            if (mainBaseLeftBorder < 0)
            {
                mainBaseLeftBorder = 0;
            }
            if (mainBaseRightBorder > MapGrid.GetUpperBound(1))
            {
                mainBaseRightBorder = MapGrid.GetUpperBound(1);
            }

            // Start at base location
            for (int y = (int)_startLocation.Y; y < MapGrid.GetUpperBound(0) && y >= 0; y += yChange)
            {
                for (int x = (int)_startLocation.X; x >= mainBaseLeftBorder && x <= mainBaseRightBorder; x += xChange)
                {
                    if (MapGrid[x, y] == Ground.BuildingPlacable)
                    {
                        //width--;  // Turn into position offset
                        //height--;  // Turn into position offset

                        int top = x + height;
                        int left = y;
                        int right = y + width;
                        int bottom = x;
                        for (int i = bottom; i < top; i++)
                        {
                            for (int j = left; j < right; j++)
                            {
                                if (MapGrid[i, j] != Ground.BuildingPlacable)
                                {
                                    goto here;
                                }
                            }
                        }
                        var point2D = new Point2D { X = x + ((width - 1) / 2F), Y = y + ((height - 1) / 2F) };
                        // Store this point as a new building, so won't suggest this point again
                        StoreNewBuilding(point2D, width, height);
                        return point2D;
                    }
                    here:;
                }
            }
            Console.WriteLine($"No available build area found of size: {width}x{height}");
            return StartLocation2D;
        }

        private Point2D StartLocation2D => new Point2D { X = _startLocation.X, Y = _startLocation.Y };
    }

    public enum Ground
    {
        Airspace,
        Pathable,
        BuildingPlacable,
        Building
    }
}