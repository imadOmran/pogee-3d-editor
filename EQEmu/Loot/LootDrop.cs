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
    public class LootDrop : ManageDatabase
    {
        private int _id;
        private string _name;

        //bridge table
        private int _multiplier;
        private int _probability;

        private bool _newCreation = false;
        private TypeQueriesExtension _newQuery = null;

        private ObservableCollection<LootDropEntry> _entries = new ObservableCollection<LootDropEntry>();

        private readonly MySqlConnection _connection;

        public LootDrop(int id,MySqlConnection connection, QueryConfig config)
            : base(config)
        {            
            _connection = connection;
            _id = id;
            Lookup(id);
        }

        public LootDrop(MySqlConnection connection, QueryConfig config)
            : base(config)
        {
            _newQuery = _queries.ExtensionQueries.FirstOrDefault(x => x.Name == "NewInsert");
            _connection = connection;
        }

        [Browsable(false)]
        public ObservableCollection<LootDropEntry> Entries
        {
            get { return _entries; }
        }

        public void CreateForInsert()
        {
            _newCreation = true;
        }

        public int Multiplier
        {
            get { return _multiplier; }
            set
            {
                _multiplier = value;
                Dirtied();
            }
        }

        public int Probability
        {
            get { return _probability; }
            set
            {
                _probability = value;
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

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                Dirtied();
            }
        }

        private int _lootTableId;
        public int LootTableId
        {
            get { return _lootTableId; }
            set
            {
                _lootTableId = value;
                Dirtied();
            }
        }

        public void AddLootDropEntry(LootDropEntry entry)
        {
            if (Entries.Count(x => x.ItemId == entry.ItemId && x.LootDropId == entry.LootDropId) > 0)
            {
                return;
            }

            if (CreatedObj)
            {
                NeedsInserted.Add(entry);
            }
            _entries.Add(entry);
        }

        public void RemoveLootDropEntry(LootDropEntry entry)
        {
            if (CreatedObj)
            {
                if( !NeedsInserted.Contains(entry) )
                {
                    NeedsDeleted.Add(entry);
                }
            }
            _entries.Remove(entry);
        }

        public void Lookup(int id)
        {
            _id = id;

            var sql = String.Format(SelectString, SelectArgValues);
            var results = QueryHelper.TryRunQuery(_connection, sql);
            foreach (var row in results)
            {
                var entry = new LootDropEntry(_connection, _queryConfig);
                entry.SetProperties(Queries, row);
                entry.Created();

                AddLootDropEntry(entry);
            }
            Created();
        }

        public LootDropEntry Create()
        {
            var entry = new LootDropEntry(_connection, _queryConfig);
            entry.LootDropId = Id;
            entry.Created();
            return entry;
        }

        public void BalanceEntries(IEnumerable<LootDropEntry> entries)
        {
            if (entries == null) return;

            int count = Entries.Sum(x => x.Chance);
            int units = 100 - count;
            if (units <= 0) return;

            while (units > 0)
            {
                foreach (var entry in entries)
                {
                    entry.Chance += 1;
                    units--;
                    if (units <= 0) return;
                }
            }
        }

        public string GetSQL()
        {
            string sql = SQLSetVariable();

            if (_newCreation)
            {
                sql += String.Format(_newQuery.InsertQuery, ResolveArgs(_newQuery.InsertArgs));
            }
            sql += GetQuery(Entries);            

            return sql;     
        }

        public LootDrop Clone()
        {
            return (LootDrop)MemberwiseClone();
        }
        
        public override string UpdateString
        {
            get
            {
                string sql = SQLSetVariable();
                if(Dirty)
                {
                    sql += base.UpdateString;
                }
                sql += GetQuery(Entries);
                return sql;
            }
        }

        public override string DeleteString
        {
            get
            {
                string sql = SQLSetVariable();
                sql += base.DeleteString;
                sql += GetQuery(Entries);
                return sql;
            }
        }

        private string SQLSetVariable()
        {
            return String.Format("SET @LootDropID = {0};", Id);
        }

        public override string InsertString
        {
            get
            {
                string sql = SQLSetVariable();
                if (_newCreation) sql += String.Format(_newQuery.InsertQuery, ResolveArgs(_newQuery.InsertArgs));
                sql += base.InsertString;
                sql += GetQuery(Entries);
                return sql;
            }
        }

        public override List<IDatabaseObject> DirtyComponents
        {
            get
            {
                return Entries.Where(x => x.Dirty).ToList<IDatabaseObject>();
            }
        }

        public override string ToString()
        {
            return Id.ToString() + " " + Name;
        }
    }

    public class LootDropEntry : DatabaseObject
    {
        private int _lootdropId;
        private int _itemId;
        private int _itemCharges = 1;

        private bool _equipItem;
        private int _chance;

        private string _itemName;

        private readonly MySqlConnection _connection;

        public LootDropEntry(MySqlConnection connection, QueryConfig _config)
            : base(_config)
        {
            _connection = connection;
        }

        public int LootDropId
        {
            get { return _lootdropId; }
            set
            {
                _lootdropId = value;
                Dirtied();
            }
        }

        public int ItemId
        {
            get { return _itemId; }
            set
            {
                _itemId = value;
                Dirtied();
            }
        }

        public string ItemName
        {
            get { return _itemName; }
            set
            {
                _itemName = value;
                Dirtied();
            }
        }

        public int ItemCharges
        {
            get { return _itemCharges; }
            set
            {
                _itemCharges = value;
                Dirtied();
            }
        }

        public bool EquipItem
        {
            get { return _equipItem; }
            set
            {
                _equipItem = value;
                Dirtied();
            }
        }

        public int Chance
        {
            get { return _chance; }
            set
            {
                if (value > 100) _chance = 100;
                else if (value < 0) _chance = 0;
                else _chance = value;
                Dirtied();
            }
        }

        public override string ToString()
        {
            return this.ItemId.ToString() + " " + ItemName;
        }
    }
}
