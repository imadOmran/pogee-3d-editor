using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EQEmu.Database;

namespace EQEmu.Loot
{
    public class LootTableAggregatorLocal : LootTableAggregator
    {
        public LootTableAggregatorLocal(QueryConfig config)
            : base(config)
        {

        }

        public override void Lookup(int id)
        {
            throw new NotImplementedException();
        }

        public override LootTable CreateLootTable()
        {
            throw new NotImplementedException();
        }

        public override LootDrop CreateLootDrop()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Item> LookupItems(string name)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object> LookupTables(string name)
        {
            throw new NotImplementedException();
        }
    }
}
