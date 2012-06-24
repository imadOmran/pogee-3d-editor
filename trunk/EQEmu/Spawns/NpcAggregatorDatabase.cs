using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public class NpcAggregatorDatabase : NpcAggregator
    {
        private readonly MySqlConnection _connection;
        private TypeQueriesExtension _lookupByZone = null;
        private TypeQueriesExtension _maxIdForZone = null;

        public NpcAggregatorDatabase(MySqlConnection connection, QueryConfig config)
            : base(config)
        {
            _connection = connection;
            _lookupByZone = _queries.GetExtensionQuery("LookupByZone");
            _maxIdForZone = _queries.GetExtensionQuery("MaxIdForZone");
            //_lookupByZone = _queries.ExtensionQueries.FirstOrDefault(x => x.Name == "LookupByZone");
            //_maxIdForZone = _queries.ExtensionQueries.FirstOrDefault(x => x.Name == "MaxIdForZone");
        }

        public override void Lookup(string name)
        {
            FilterName = name;
            var sql = String.Format(Queries.SelectQuery, ResolveArgs(Queries.SelectArgs));
            var results = Database.QueryHelper.RunQuery(_connection, sql);
            
            NPCs.Clear();
            UnlockObject();
            foreach (var dictionary in results)
            {
                var npc = new Npc(_queryConfig);
                //npc.SetProperties(Queries, dictionary);
                npc.SetPropertiesFaster(Queries, dictionary);
                AddNPC(npc);
                npc.Created();
            }
            Created();
        }

        public override int GetNextIdForZone(string zone)
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

        public override void LookupByZone(string zone)
        {
            if (_lookupByZone != null)
            {
                var sql = String.Format(_lookupByZone.SelectQuery, zone);
                var results = QueryHelper.RunQuery(_connection, sql);

                NPCs.Clear();

                foreach (var dict in results)
                {
                    var npc = new Npc(_queryConfig);
                    npc.SetProperties(Queries, dict);
                    AddNPC(npc);
                    npc.Created();
                }
            }
        }
    }
}
