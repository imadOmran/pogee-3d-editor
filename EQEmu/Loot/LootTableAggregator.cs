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
    public class LootTableAggregator : ManageDatabase
    {
        private readonly MySqlConnection _connection;

        private ObservableCollection<LootTable> _lootTables = new ObservableCollection<LootTable>();

        public LootTableAggregator(MySqlConnection connection, QueryConfig config):base(config)
        {
            _connection = connection;
        }

        public void Lookup(int id)
        {
            if (id >= 0 && _lootTables.Where(x => x.Id == id).Count() == 0)
            {
                var table = new LootTable(_connection, _queryConfig);
                table.Id = -1;
                table.Lookup(id);
                _lootTables.Add(table);
            }
        }

        public IEnumerable<Item> LookupItems(string name)
        {
            List<Item> items = new List<Item>();
            if (name != null)
            {
                var sql = String.Format("SELECT * FROM items WHERE name LIKE '%{0}%';",name);
                var results = Database.QueryHelper.TryRunQuery(_connection,sql);

                if (results == null) return items;

                foreach (var row in results)
                {
                    var item = new Item( Int32.Parse( row["id"].ToString() ), row["Name"].ToString());
                    items.Add(item);
                }
            }
            return items;
        }

        public IEnumerable<object> LookupTables(string name)
        {
            List<dynamic> items = new List<dynamic>();

            if (name != null)
            {
                var sql = String.Format("SELECT * FROM loottable WHERE name LIKE '%{0}%';", name);
                var results = Database.QueryHelper.TryRunQuery(_connection, sql);
                foreach (var row in results)
                {
                    dynamic table = new ExpandoObject();
                    table.Id = Int32.Parse(row["id"].ToString());
                    table.Name = row["name"].ToString();
                    items.Add(table);
                }
            }

            return items;
        }

        public ObservableCollection<LootTable> LootTables
        {
            get { return _lootTables; }
        }

        public void AddLootTable(LootTable table)
        {
            if (CreatedObj)
            {
                NeedsInserted.Add(table);
            }
            _lootTables.Add(table);
        }

        public void RemoveLootTable(LootTable table)
        {
            if (CreatedObj)
            {
                if (!NeedsInserted.Contains(table))
                {
                    NeedsDeleted.Add(table);
                }
            }
            _lootTables.Remove(table);
        }

        public void ClearCache()
        {
            NeedsDeleted.Clear();
            NeedsInserted.Clear();
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

        public LootTable CreateLootTable(bool getValidId = false)
        {
            LootTable table = new LootTable(_connection, _queryConfig);
            int id = 0;

            if (getValidId)
            {
                var results = QueryHelper.TryRunQuery(_connection, "SELECT Max(id) as id FROM loottable;");
                if (results.Count > 0)
                {
                    id = Int32.Parse(results.ElementAt(0)["id"].ToString()) + 1;
                }

                //now check if our working set has an id greater then this...
                //caching this might make sense but for now it's not happening
                if (LootTables.Count > 0)
                {
                    int curMax = LootTables.Max(x => x.Id);
                    if (id <= curMax)
                    {
                        id = curMax + 1;
                    }
                }
            }
            table.Created();
            table.Id = id;            
            return table;
        }

        public LootDrop CreateLootDrop(bool getValidId=false)
        {
            LootDrop drop = new LootDrop(_connection, _queryConfig);
            int id = 0;

            if (getValidId)
            {
                var results = QueryHelper.TryRunQuery(_connection, "SELECT Max(id) as id FROM lootdrop;");
                if (results.Count > 0)
                {
                    id = Int32.Parse(results.ElementAt(0)["id"].ToString()) + 1;
                }
            }
            drop.Id = id;            

            return drop;
        }
    }
}
