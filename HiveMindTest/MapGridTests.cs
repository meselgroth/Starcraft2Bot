using FluentAssertions;
using HiveMind;
using NUnit.Framework;
using SC2APIProtocol;

namespace HiveMindTest
{
    public class MapGridTests
    {
        private Ground[,] _mapGrid;

        [SetUp]
        public void Setup()
        {
            // 0 0 0 0 0
            // 0 0 0 1 1
            // 0 0 1 1 1
            // 0 1 1 1 1
            // 0 1 1 1 1
            // 0 0 0 1 1
            _mapGrid = new Ground[6, 5]
                {
                    { Ground.Airspace,Ground.Airspace,Ground.Airspace,Ground.Airspace,Ground.Airspace },
                    { Ground.Airspace,Ground.Airspace,Ground.Airspace,Ground.BuildingPlacable,Ground.BuildingPlacable },
                    { Ground.Airspace,Ground.Airspace,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable },
                    { Ground.Airspace,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable },
                    { Ground.Airspace,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable },
                    { Ground.Airspace,Ground.Airspace,Ground.Airspace,Ground.BuildingPlacable,Ground.BuildingPlacable }
                };
        }

        [Test]
        public void GetAvailableDiamondInMainBaseTest()
        {
            var sut = new MapManager(_mapGrid);

            var point = sut.GetAvailableDiamondInMainBase(1, 1);

            point.X.Should().Be(3);
            point.Y.Should().Be(1);
        }
        [Test]
        public void GetAvailableDiamondInMainBaseForSmallBuilding()
        {
            var sut = new MapManager(_mapGrid);

            var point = sut.GetAvailableDiamondInMainBase(2, 2);

            // Returns center point
            point.X.Should().Be(3.5F);
            point.Y.Should().Be(1.5F);
        }
        [Test]
        public void GetAvailableDiamondInMainBaseForMediumBuilding()
        {
            var sut = new MapManager(_mapGrid);

            var point = sut.GetAvailableDiamondInMainBase(3, 3);

            // Returns center point
            point.X.Should().Be(3F);
            point.Y.Should().Be(3F);
        }
        [Test]
        public void GetAvailableDiamondForMediumBuildingWhenMainBaseIsBottomRight()
        {
            // Y is flipped, y=0 is bottom

            // 2 2 2 2 2 2 2 2
            // 2 2 2 3 3 3 3 3
            // 2 m 2 3 3 3 3 3 
            // x 2 2 3 3 b 3 3 
            // 2 2 2 3 3 3 3 3 
            // 2 2 2 3 3 3 3 3 

            // b is center of main base (and starting point of search)
            // x is first found available spot, m is the middle of 3x3 building
            _mapGrid = new Ground[8, 6]
                {
                    { Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable },
                    { Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable },
                    { Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable },
                    { Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable },
                    { Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable },
                    { Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable },
                    { Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable },
                    { Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable,Ground.BuildingPlacable }
                };

            var sut = new MapManager(_mapGrid, new Point { X = 5, Y = 2, Z = 0 });  // Y is flipped, 0 is bottom

            var point = sut.GetAvailableDiamondInMainBase(3, 3);

            // Returns center point
            point.X.Should().Be(1F);
            point.Y.Should().Be(3F);
        }
    }
}
