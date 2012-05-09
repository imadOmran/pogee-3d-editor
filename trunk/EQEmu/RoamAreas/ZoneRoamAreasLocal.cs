using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EQEmu.Database;

namespace EQEmu.RoamAreas
{
    public class ZoneRoamAreasLocal : ZoneRoamAreas
    {
        public ZoneRoamAreasLocal(string zone, QueryConfig config)
            : base(zone, config)
        {
            Created();
        }
    }
}
