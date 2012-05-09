using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public class ZoneSpawnsLocal : ZoneSpawns
    {
        public ZoneSpawnsLocal(QueryConfig config)
            : base(config)
        {
            Created();
        }

        public override Spawn2 GetNewSpawn()
        {
            Spawn2 spawn = new Spawn2(_queryConfig);
            int id = 1;

            if (Spawns.Count > 0)
            {
                id = Spawns.Max(x => x.Id) + 1;
            }

            spawn.Zone = _zone;
            spawn.Version = _version;
            spawn.Id = id;

            return spawn;
        }
    }
}
