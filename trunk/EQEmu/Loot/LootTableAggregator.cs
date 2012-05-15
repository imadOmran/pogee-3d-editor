using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Dynamic;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Loot
{
    public abstract class LootTableAggregator : ManageDatabase
    {
        protected ObservableCollection<LootTable> _lootTables = new ObservableCollection<LootTable>();

        public LootTableAggregator(QueryConfig config):base(config)
        {
            
        }

        public abstract void Lookup(int id);
        public abstract IEnumerable<Item> LookupItems(string name);
        public abstract LootTable CreateLootTable();
        public abstract LootDrop CreateLootDrop();
        public abstract IEnumerable<object> LookupTables(string name);

        public ObservableCollection<LootTable> LootTables
        {
            get { return _lootTables; }
        }

        public void AddLootTable(LootTable table)
        {
            if (_lootTables.Contains(table)) return;

            AddObject(table);
            _lootTables.Add(table);
        }

        public void RemoveLootTable(LootTable table)
        {
            if (!_lootTables.Contains(table)) return;
            RemoveObject(table);
            _lootTables.Remove(table);
        }

        public void ClearCache()
        {
            ClearObjects();
            _lootTables.Clear();
        }

        public string GetSQL()
        {
            var sql = GetQuery(LootTables);
            return sql;
        }
    
        public override List<IDatabaseObject>  DirtyComponents
        {
	        get { throw new NotImplementedException(); }
        }
    }
}
