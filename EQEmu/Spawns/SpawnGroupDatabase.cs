using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public class SpawnGroupDatabase : SpawnGroup
    {
        private readonly MySqlConnection _connection;
        private readonly TypeQueriesExtension _getSpawn2;

        public SpawnGroupDatabase(MySqlConnection connection, QueryConfig config)
            : base(config)
        {
            _connection = connection;
            _getSpawn2 = Queries.ExtensionQueries.FirstOrDefault(x => x.Name == "GetSpawn2");
        }

        public override IEnumerable<Spawn2> GetLinkedSpawn2()
        {
            List<Spawn2> spawns = new List<Spawn2>();

            var query = _getSpawn2;
            if (query != null)
            {
                string sql = String.Format(query.SelectQuery, new string[] { Id.ToString() });
                var results = Database.QueryHelper.RunQuery(_connection, sql);
                foreach (var row in results)
                {
                    var spawn = new Spawn2(_queryConfig);
                    spawn.SetProperties(query, row);
                    spawns.Add(spawn);
                }
            }
            return spawns;
        }

        public override void GetEntries()
        {
            var sql = String.Format(SelectString, SelectArgValues);
            var results = Database.QueryHelper.RunQuery(_connection, sql);

            foreach (var row in results)
            {
                var entry = new SpawnEntry(_queryConfig);
                entry.SetProperties(Queries, row);

                if (_entries.Where(x => x.NpcID == entry.NpcID).FirstOrDefault() == null)
                {
                    _entries.Add(entry);
                    entry.ObjectDirtied += new Database.ObjectDirtiedHandler(entry_ObjectDirtied);
                    entry.Created();
                    OnSpawnChanceTotalChanged();
                }
            }
        }
    }
}
