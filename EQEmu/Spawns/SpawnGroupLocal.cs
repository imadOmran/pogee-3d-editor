using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public class SpawnGroupLocal : SpawnGroup
    {
        public SpawnGroupLocal(QueryConfig config)
            : base(config)
        {

        }

        public override IEnumerable<Spawn2> GetLinkedSpawn2()
        {
            throw new NotImplementedException();
        }

        public override void GetEntries()
        {
            throw new NotImplementedException();
        }
    }
}
