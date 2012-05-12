using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using EQEmu.Database;

namespace EQEmu.Spawns
{    
    public class SpawnGroupLocal : SpawnGroup
    {
        private SpawnGroupLocal()
        {
        }

        public SpawnGroupLocal(QueryConfig config)
            : base(config)
        {

        }

        public override IEnumerable<Spawn2> GetLinkedSpawn2()
        {
            return new List<Spawn2>();
        }

        public override void GetEntries()
        {
            //throw new NotImplementedException();
        }
    }
}
