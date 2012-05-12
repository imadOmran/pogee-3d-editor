using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public class SpawnGroupAggregatorDatabase : SpawnGroupAggregator
    {
        private readonly MySqlConnection _connection;

        public SpawnGroupAggregatorDatabase(MySqlConnection connection,QueryConfig config)
            : base(config)
        {
            _connection = connection;
        }

        public override SpawnGroup CreateSpawnGroup()
        {
            SpawnGroup sg = null;

            var qconf = Queries.ExtensionQueries.FirstOrDefault(x => { return x.Name == "GetMaxID"; });
            var results = Database.QueryHelper.RunQuery(_connection, qconf.SelectQuery);

            sg = new SpawnGroupDatabase(_connection, _queryConfig);
            if (results != null)
            {
                var row = results.FirstOrDefault();
                if (row != null)
                {
                    sg.SetProperties(qconf, row);
                    sg.Id += 1;
                }
            }

            if (SpawnGroups.Count() > 0)
            {
                //in case we created/generated an entry not yet in the database 
                //look in the collection for a max Id
                var maxLoaded = SpawnGroups.Max(x => { return x.Id; });
                if (maxLoaded >= sg.Id)
                {
                    sg.Id = maxLoaded + 1;
                }
            }

            sg.Created();
            //AddSpawnGroup(sg);
            return sg;
        }

        protected override SpawnGroup Lookup(int id)
        {
            var sg = base.Lookup(id);
            if (sg != null) return sg;

            //this is dependant on the FilterId property - per config.xml...I don't like this
            string sql = String.Format(SelectString, SelectArgValues);
            var results = Database.QueryHelper.RunQuery(_connection, sql);
            var row = results.FirstOrDefault();
            if (row != null)
            {
                sg = new SpawnGroupDatabase(_connection, _queryConfig);
                sg.SetProperties(Queries, row);                
                _spawnGroups.Add(sg);
                sg.Created();
                sg.GetEntries();
            }

            return sg;
        }

        public IEnumerable<SpawnGroup> LookupByZone(string zone)
        {
            ClearCache();

            var query = Queries.ExtensionQueries.FirstOrDefault(x => x.Name == "GetByZone");
            if (query != null)
            {
                string sql = String.Format(query.SelectQuery, new string[] { zone });
                var results = Database.QueryHelper.RunQuery(_connection, sql);
                foreach (var row in results)
                {
                    if (row.ContainsKey("spawngroupid"))
                    {
                        FilterById = (int)row["spawngroupid"];
                    }
                }
                return _spawnGroups;
            }
            else return null;
        }
    }
}
