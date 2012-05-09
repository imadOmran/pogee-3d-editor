using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public class ZoneSpawnsDatabase : ZoneSpawns
    {
        private readonly MySqlConnection _connection = null;
        private readonly TypeQueriesExtension _allVersions = null;
        private readonly TypeQueriesExtension _getMaxZoneId = null;
        private readonly TypeQueriesExtension _getMaxId = null;

        public ZoneSpawnsDatabase(MySqlConnection connection, QueryConfig config)
            : base(config)
        {
            _connection = connection;
            _allVersions = Queries.ExtensionQueries.FirstOrDefault(x => x.Name == "GetAllVersions");
            _getMaxZoneId = Queries.ExtensionQueries.FirstOrDefault(x => x.Name == "GetMaxZoneID");
            _getMaxId = Queries.ExtensionQueries.FirstOrDefault(x => x.Name == "GetMaxID");
        }

        public void Lookup(string zone, int version)
        {
            _zone = zone;
            _version = version;

            UnlockObject();

            string sql = "";

            if (_version == -1)
            {
                sql = String.Format(_allVersions.SelectQuery, ResolveArgs(_allVersions.SelectArgs));
            }
            else
            {
                sql = String.Format(SelectString, SelectArgValues);
            }

            var results = Database.QueryHelper.RunQuery(_connection, sql);
            if (results != null)
            {
                foreach (var row in results)
                {
                    Spawn2 s = new Spawn2(_queryConfig);
                    s.SetProperties(Queries, row);
                    s.Created();
                    Spawns.Add(s);
                }
            }

            Created();
        }

        public override Spawn2 GetNewSpawn()
        {
            Spawn2 spawn = new Spawn2(_queryConfig);
            spawn.Zone = _zone;

            int max = 1;
            if (Spawns.Count > 0) max = Spawns.Max(x => x.Id) + 1;

            //test if this exists... if it does get the true max value from DB
            bool exists = false;

            var sql = String.Format(_getMaxZoneId.SelectQuery, max);
            var results = Database.QueryHelper.RunQuery(_connection, sql);
            if (results != null)
            {
                if (results.Count > 0)
                {
                    exists = true;
                }

                if (exists)
                {
                    sql = String.Format(_getMaxId.SelectQuery);
                    results = Database.QueryHelper.RunQuery(_connection, sql);
                    if (results != null)
                    {
                        if (results.Count > 0)
                        {
                            if (results.ElementAt(0).ContainsKey("id"))
                            {
                                max = Int32.Parse(results.ElementAt(0)["id"].ToString()) + 1;
                            }
                        }
                    }
                }
            }

            spawn.Id = max;
            spawn.Version = _version;
            return spawn;
        }
    }
}
