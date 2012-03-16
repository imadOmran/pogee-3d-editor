using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public class NPCAggregator : Database.ManageDatabase
    {
        private ObservableCollection<NPC> _npcs = new ObservableCollection<NPC>();
        private List<NPC> _cache = new List<NPC>();
        private readonly MySqlConnection _connection;

        private TypeQueriesExtension _lookupByZone = null;

        public NPCAggregator(MySqlConnection connection, Database.QueryConfig config)
            : base(config)
        {
            if (connection == null) throw new Exception("must provide database connection");
            _connection = connection;

            _lookupByZone = _queries.ExtensionQueries.FirstOrDefault(x => x.Name == "LookupByZone");
        }

        public ObservableCollection<NPC> NPCs
        {
            get { return _npcs; }
        }

        //this is kind of a pointless property... only used for reflection property setting in query
        public string FilterName
        {
            get;
            set;
        }

        public void ClearCache()
        {
            NPCs.Clear();
            _cache.Clear();
        }

        private void AddNPC(NPC npc)
        {
            _npcs.Add(npc);

            if (_cache.FirstOrDefault(x => x.Id == npc.Id) != null) return;
            _cache.Add(npc);
        }

        public string GetSQL()
        {
            return GetQuery(_cache);
        }
        
        public void Lookup(string name)
        {
            FilterName = name;
            var sql = String.Format(Queries.SelectQuery,ResolveArgs(Queries.SelectArgs));            
            var results = Database.QueryHelper.RunQuery(_connection,sql);

            _npcs.Clear();

            foreach (var dictionary in results)
            {
                var npc = new NPC(_queryConfig);
                npc.SetProperties(Queries,dictionary);
                AddNPC(npc);
                npc.Created();
            }
        }

        public void LookupByZone(string zone)
        {
            if (_lookupByZone != null)
            {
                var sql = String.Format(_lookupByZone.SelectQuery, zone);
                var results = QueryHelper.RunQuery(_connection, sql);

                _npcs.Clear();

                foreach (var dict in results)
                {
                    var npc = new NPC(_queryConfig);
                    npc.SetProperties(Queries, dict);
                    AddNPC(npc);
                    npc.Created();
                }
            }
        }

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class NPC : Database.DatabaseObject
    {
        private string _name;
        private int _level;
        private int _id;
        private int _lootTableId;

        public NPC(Database.QueryConfig conf)
            : base(conf)
        {

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

        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
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

        public int LootTableId
        {
            get { return _lootTableId; }
            set
            {
                _lootTableId = value;
                Dirtied();
            }
        }
    }
}
