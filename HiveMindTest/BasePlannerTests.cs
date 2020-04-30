using FluentAssertions;
using HiveMind;
using NUnit.Framework;
using Point = System.Drawing.Point;

namespace HiveMindTest
{
    [TestFixture]
    public class BasePlannerTests
    {
        [Test]
        public void PlanPathFromAToBNoObstructions()
        {
            var sut = new BasePlanner(10, 10);

            sut.FindAndSavePath(new Point(5, 0), new Point(5, 5));

            sut.Grid[5, 1].Should().Be(4);
            sut.Grid[5, 2].Should().Be(4);
            sut.Grid[5, 3].Should().Be(4);
            sut.Grid[5, 4].Should().Be(4);
        }

        [Test]
        public void PlanPathFromBToANoObstructions()
        {
            var sut = new BasePlanner(10, 10);

            sut.FindAndSavePath(new Point(5, 2), new Point(5, 0));

            sut.Grid[5, 1].Should().Be(4);
        }

        [Test]
        public void PlanPathFromBToALongNoObstructions()
        {
            var sut = new BasePlanner(10, 10);

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
            var sut = new BasePlanner(10, 10);

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
            var sut = new BasePlanner(10, 10);

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
}