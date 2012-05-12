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
            Created();
        }

        public override SpawnGroup CreateSpawnGroup()
        {
            int id = 1;
            var sg = new SpawnGroupLocal(_queryConfig);

            if (SpawnGroups.Count() > 0)
            {
                id = SpawnGroups.Max(x => x.Id) + 1;
            }

            sg.Id = id;
            sg.Created();
            //AddSpawnGroup(sg);

            return sg;
        }
    }
}
