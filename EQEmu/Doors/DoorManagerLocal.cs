using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EQEmu.Database;

namespace EQEmu.Doors
{
    public class DoorManagerLocal : DoorManager
    {
        public DoorManagerLocal(string zone, int version, QueryConfig config)
            : base(zone, version, config)
        {
            Created();
        }
    }
}
