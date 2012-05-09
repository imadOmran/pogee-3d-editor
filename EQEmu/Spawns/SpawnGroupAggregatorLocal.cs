using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public class SpawnGroupAggregatorLocal : SpawnGroupAggregator
    {
        public SpawnGroupAggregatorLocal(QueryConfig config)
            : base(config)
        {

        }

        public override SpawnGroup CreateSpawnGroup()
        {
            throw new NotImplementedException();
        }
    }
}
