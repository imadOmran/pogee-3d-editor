using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EQEmu.Database;

namespace EQEmu.Grids
{
    public class ZoneGridsLocal : ZoneGrids
    {
        public ZoneGridsLocal(string zone, QueryConfig config)
            : base(zone, config)
        {
            Created();
        }
    }
}
