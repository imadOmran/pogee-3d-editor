using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using MySql.Data.MySqlClient;
using EQEmu.Database;

namespace EQEmu.Loot
{
    public class LootTableAggregatorDatabase : LootTableAggregator
    {
        private readonly MySqlConnection _connection;

        public LootTableAggregatorDatabase(MySqlConnection connection, QueryConfig config)
            : base(config)
        {
            _connection = connection;
        }

        public override IEnumerable<object> LookupTables(string name)
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

        public override void Lookup(int id)
        {
            if (id >= 0 && _lootTables.Where(x => x.Id == id).Count() == 0)
            {
                var table = new LootTable(_connection, _queryConfig);
                table.Id = -1;
                table.Lookup(id);
                _lootTables.Add(table);
            }
        }

        public override IEnumerable<Item> LookupItems(string name)
        {
            List<Item> items = new List<Item>();
            if (name != null)
            {
                var sql = String.Format("SELECT * FROM items WHERE name LIKE '%{0}%';", name);
                var results = Database.QueryHelper.TryRunQuery(_connection, sql);

                if (results == null) return items;

                foreach (var row in results)
                {
                    var item = new Item(Int32.Parse(row["id"].ToString()), row["Name"].ToString());
                    items.Add(item);
                }
            }
            return items;
        }

        public override LootTable CreateLootTable()
        {
            LootTable table = new LootTable(_connection, _queryConfig);
            int id = 0;

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
            
            table.Created();
            table.Id = id;
            return table;
        }

        public override LootDrop CreateLootDrop()
        {
            LootDrop drop = new LootDrop(_connection, _queryConfig);
            int id = 0;

            var results = QueryHelper.TryRunQuery(_connection, "SELECT Max(id) as id FROM lootdrop;");
            if (results.Count > 0)
            {
                id = Int32.Parse(results.ElementAt(0)["id"].ToString()) + 1;
            }
            drop.Id = id;

            return drop;
        }
    }
}
