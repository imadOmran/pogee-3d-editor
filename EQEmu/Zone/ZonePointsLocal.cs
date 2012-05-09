using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EQEmu.Database;

namespace EQEmu.Zone
{
    public class ZonePointsLocal : ZonePoints
    {
        public ZonePointsLocal(string zone,QueryConfig config)
            : base(zone,config)
        {
            Created();
        }
    }
}
