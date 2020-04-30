using System;
using System.Collections.Generic;
using System.Drawing;

namespace HiveMind
{
    public class BasePlanner
    {
        public int[,] Grid; // 0=free, 1=building, 2=minerals, 3=geyser, 4=path

        public BasePlanner(int xAxisLength, int yAxisLength)
        {
            Grid = new int[xAxisLength+1, yAxisLength+1];
        }

        public List<Point> FindAndSavePath(Point start, Point finish)
        {
            var xDistance = finish.X - start.X; // >0 = right, <0 = left
            var yDistance = finish.Y - start.Y; // >0 = down, <0 = up

            var path = new List<Point>();
            var nextX = start.X;
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