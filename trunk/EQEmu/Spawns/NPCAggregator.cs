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
        private TypeQueriesExtension _maxIdForZone = null;

        public NPCAggregator(MySqlConnection connection, Database.QueryConfig config)
            : base(config)
        {
            if (connection == null) throw new Exception("must provide database connection");
            _connection = connection;

            _lookupByZone = _queries.ExtensionQueries.FirstOrDefault(x => x.Name == "LookupByZone");
            _maxIdForZone = _queries.ExtensionQueries.FirstOrDefault(x => x.Name == "MaxIdForZone");

            /*
            _templates.Add(new LDONNpcTemplate());
            _templates.Add(new LDONRujTemplate());
            _templates.Add(new CastersTemplate());
            _templates.Add(new LDONNormalToHighRisk());
            */
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
            NeedsDeleted.Clear();
            NeedsInserted.Clear();
        }

        public void AddNPC(NPC npc)
        {
            AddObject(npc);
            _npcs.Add(npc);

            if (_cache.FirstOrDefault(x => x.Id == npc.Id) != null) return;
            _cache.Add(npc);
        }

        public void RemoveNPC(NPC npc)
        {
            RemoveObject(npc);
            _npcs.Remove(npc);
            _cache.Remove(npc);
        }

        public NPC CreateNPC()
        {
            return new NPC(_queryConfig);
        }

        public string GetSQL()
        {
            return GetQuery(_cache);
        }

        public void Lookup(string name)
        {
            FilterName = name;
            var sql = String.Format(Queries.SelectQuery, ResolveArgs(Queries.SelectArgs));
            var results = Database.QueryHelper.RunQuery(_connection, sql);

            _npcs.Clear();

            foreach (var dictionary in results)
            {
                var npc = new NPC(_queryConfig);
                npc.SetProperties(Queries, dictionary);
                AddNPC(npc);
                npc.Created();
            }
        }

        public int GetNextIdForZone(string zone)
        {
            if (_maxIdForZone != null)
            {
                var sql = String.Format(_maxIdForZone.SelectQuery, zone);
                var results = QueryHelper.TryRunQuery(_connection, sql);
                if (results != null)
                {
                    if (results.Count > 0)
                    {
                        if (results.ElementAt(0)["id"].GetType() != typeof(System.DBNull))
                        {
                            return (int)results.ElementAt(0)["id"] + 1;
                        }
                    }
                }
            }

            return 1;
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
}
