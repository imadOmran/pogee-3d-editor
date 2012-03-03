using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace ApplicationCore.Helpers
{
    public static class Math
    {
        public static double Distance(Point3D p1, Point3D p2)
        {
            return System.Math.Sqrt((System.Math.Pow(p2.X - p1.X, 2) + System.Math.Pow(p2.Y - p1.Y, 2) + System.Math.Pow(p2.Z - p1.Z, 2)));
        }
    }
}
