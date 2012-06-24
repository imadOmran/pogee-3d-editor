using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public class NpcAggregatorLocal : NpcAggregator
    {
        public NpcAggregatorLocal(QueryConfig config)
            : base(config)
        {

        }

        public override void Lookup(string name)
        {
            var lname = name.ToLower();

            NPCs.Clear();
            foreach (var npc in CachedNpcs)
            {                
                if (npc.Name.ToLower().Contains(lname))
                {
                    AddNPC(npc);
                }
            }
        }

        public override int GetNextIdForZone(string zone)
        {
            int id = 1;
            if (NPCs.Count > 0)
            {
                id += NPCs.Max(x => x.Id);
            }
            return id;
        }

        public override void LookupByZone(string zone)
        {
            foreach (var npc in CachedNpcs)
            {
                AddNPC(npc);
            }
        }
    }
}
