using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Loot
{
    public class LootTable : ManageDatabase
    {
        private int _id;
        private string _name;
        private int _minCash = 0;
        private int _maxCash = 0;

        private readonly MySqlConnection _connection;

        private ObservableCollection<LootDrop> _lootDrops = new ObservableCollection<LootDrop>();

        /*
        public LootTable(int id,MySqlConnection connection, QueryConfig _config) : base(_config)
        {
            _connection = connection;
            _id = id;
            Lookup(id);
        }
        */

        public LootTable(MySqlConnection connection, QueryConfig config)
            : base(config)
        {
            _connection = connection;
        }

        [Browsable(false)]
        public ObservableCollection<LootDrop> LootDrops
        {
            get { return _lootDrops; }
        }

        public void AddLootDrop(LootDrop lootdrop)
        {
            if (LootDrops.Count(x => x.Id == lootdrop.Id) > 0) return;
            lootdrop.LootTableId = Id;

            if (CreatedObj)
            {
                NeedsInserted.Add(lootdrop);
            }

            _lootDrops.Add(lootdrop);
        }

        public void RemoveLootDrop(LootDrop lootdrop)
        {
            if (CreatedObj)
            {
                if (!NeedsInserted.Contains(lootdrop))
                {
                    NeedsDeleted.Add(lootdrop);
                }
                else
                {
                    NeedsInserted.Remove(lootdrop);
                }
            }
            _lootDrops.Remove(lootdrop);
        }

        public void Lookup(int id)
        {
            _id = id;

            string sql = String.Format(SelectString, SelectArgValues);
            var results = Database.QueryHelper.TryRunQuery(_connection, sql);
            foreach (var row in results)
            {
                var lootdrop = new LootDrop(_connection, _queryConfig);
                lootdrop.SetProperties(Queries, row);
                lootdrop.Lookup(lootdrop.Id);                

                //hack job
                if (row.ContainsKey("mincash"))
                {
                    _minCash = Int32.Parse(row["mincash"].ToString());
                }

                if (row.ContainsKey("maxcash"))
                {
                    _minCash = Int32.Parse(row["maxcash"].ToString());
                }

                if (row.ContainsKey("name"))
                {
                    _name = row["name"].ToString();
                }
                lootdrop.UnlockObject();
                AddLootDrop(lootdrop);
                lootdrop.Created();
            }            

            Created();
        }
        
        public int MaxCash
        {
            get { return _maxCash; }
            set
            {
                _maxCash = value;
                Dirtied();
            }
        }

        public int MinCash
        {
            get { return _minCash; }
            set
            {
                _minCash = value;
                Dirtied();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                Dirtied();
            }
        }

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                Dirtied();
            }
        }

        public override string InsertString
        {
            get
            {
                string sql = base.InsertString;
                sql += GetQuery(LootDrops);
                return sql;
            }
        }

        public override string UpdateString
        {
            get
            {
                string sql = "";
                if (Dirty)
                {
                    sql += base.UpdateString;
                }

                sql += GetQuery(LootDrops);
                return sql;
            }
        }

        public override List<IDatabaseObject> DirtyComponents
        {
            get
            {
                return LootDrops.Where(x => x.Dirty || x.DirtyComponents.Count > 0).ToList<IDatabaseObject>();
            }
        }

        public override string ToString()
        {
            return "Id:" + this.Id.ToString();
        }

        public string GetSQL()
        {
            throw new NotImplementedException();
        }
    }
}
