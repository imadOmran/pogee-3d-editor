using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LineOfSightArea.Loader.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var zone = EQEmu.LineOfSightAreas.ZoneLineOfSightAreas.LoadFileBinary("misty.los");

            zone.Areas[0].AddVertex(new System.Windows.Media.Media3D.Point3D(50, 50, 0));
        }
    }
}
