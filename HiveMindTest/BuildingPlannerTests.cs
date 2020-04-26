using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Point = System.Drawing.Point;

namespace HiveMindTest
{
    [TestFixture]
    public class BuildingPlannerTests
    {
        [Test]
        public void PlanPathFromAToBNoObstructions()
        {
            var sut = new BuildingPlanner(10, 10);

            sut.FindAndSavePath(new Point(5, 0), new Point(5, 5));

            sut.Grid[5, 1].Should().Be(4);
            sut.Grid[5, 2].Should().Be(4);
            sut.Grid[5, 3].Should().Be(4);
            sut.Grid[5, 4].Should().Be(4);
        }

        [Test]
        public void PlanPathFromBToANoObstructions()
        {
            var sut = new BuildingPlanner(10, 10);

            sut.FindAndSavePath(new Point(5, 2), new Point(5, 0));

            sut.Grid[5, 1].Should().Be(4);
        }

        [Test]
        public void PlanPathFromBToALongNoObstructions()
        {
            var sut = new BuildingPlanner(10, 10);

            sut.FindAndSavePath(new Point(5, 5), new Point(5, 0));

            sut.Grid[5, 4].Should().Be(4);
            sut.Grid[5, 3].Should().Be(4);
            sut.Grid[5, 2].Should().Be(4);
            sut.Grid[5, 1].Should().Be(4);
        }

        [Test]
        public void PlanPathFromAToBDiagonalNoObstructions()
        {
            // -----s-----  s=start
            // -----x-----  x=pathway
            // -----x-----
            // -----x-----
            // -----xxxxxx  
            // ----------f  f=finish
            var sut = new BuildingPlanner(10, 10);

            sut.FindAndSavePath(new Point(5, 5), new Point(10, 10));

            sut.Grid[6, 5].Should().Be(4);
            sut.Grid[7, 5].Should().Be(4);
            sut.Grid[8, 5].Should().Be(4);
            sut.Grid[9, 5].Should().Be(4);
            sut.Grid[9, 6].Should().Be(4);
            sut.Grid[9, 7].Should().Be(4);
            sut.Grid[9, 8].Should().Be(4);
            sut.Grid[9, 9].Should().Be(4);
        }
        [Test]
        public void PlanPathFromBToADiagonalNoObstructions()
        {
            // f----------  f=finish
            // -x---------
            // -x---------
            // -x---------
            // -xxxxs-----  s=start
            var sut = new BuildingPlanner(10, 10);

            sut.FindAndSavePath(new Point(5, 5), new Point(0, 0));

            sut.Grid[4, 5].Should().Be(4);
            sut.Grid[3, 5].Should().Be(4);
            sut.Grid[2, 5].Should().Be(4);
            sut.Grid[1, 5].Should().Be(4);
            sut.Grid[1, 4].Should().Be(4);
            sut.Grid[1, 3].Should().Be(4);
            sut.Grid[1, 2].Should().Be(4);
            sut.Grid[1, 1].Should().Be(4);
        }
    }

    public class BuildingPlanner
    {
        public int[,] Grid; // 0=free, 1=building, 2=minerals, 3=geyser, 4=path

        public BuildingPlanner(int xAxisLength, int yAxisLength)
        {
            Grid = new int[xAxisLength+1, yAxisLength+1];
        }

        public List<Point> FindAndSavePath(Point start, Point finish)
        {
            var xDistance = finish.X - start.X; // >0 = right, <0 = left
            var yDistance = finish.Y - start.Y; // >0 = down, <0 = up

            var path = new List<Point>();
            int nextX = start.X;
            for (var x = 1; x < Math.Abs(xDistance); x++)
            {
                nextX = xDistance > 0 ? start.X + x : start.X - x;
                path.Add(new Point(nextX, start.Y));
            }
            for (var y = 1; y < Math.Abs(yDistance); y++)
            {
                var nextY = yDistance > 0 ? start.Y + y : start.Y - y;
                path.Add(new Point(nextX, nextY));
            }


            path.ForEach(p => Grid[p.X, p.Y] = 4);
            return path;
        }
    }
}