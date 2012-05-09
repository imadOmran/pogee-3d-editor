using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Grids
{
    public class ZoneGridsDatabase : ZoneGrids
    {
        private readonly MySqlConnection _connection;

        public ZoneGridsDatabase(string zone, MySqlConnection connection, QueryConfig config)
            : base(zone, config)
        {
            _connection = connection;
            Lookup();
            Created();
        }

        private void Lookup()
        {
            var sql = String.Format(SelectString, SelectArgValues);
            var results = Database.QueryHelper.RunQuery(_connection, sql);
            var v = _queries.SelectQueryFields.FirstOrDefault(x => x.Property == "ZoneId");
            string zoneIdField = null;
            if (v != null)
            {
                zoneIdField = v.Column;
            }

            if (results != null)
            {
                foreach (var row in results)
                {
                    Grid g = new Grid(_queryConfig);
                    if (row.ContainsKey(zoneIdField) && results.First() == row)
                    {
                        ZoneId = Int32.Parse(row[zoneIdField].ToString());
                    }

                    g.SetProperties(Queries, row);
                    Grids.Add(g);
                    g.Created();
                }

                foreach (var grid in Grids)
                {
                    sql = String.Format(grid.SelectString, grid.SelectArgValues);
                    results = Database.QueryHelper.RunQuery(_connection, sql);
                    foreach (var row in results)
                    {
                        var wp = new Waypoint(_queryConfig);

                        wp.SetProperties(grid.Queries, row);

                        wp.GridReference = grid;
                        grid.Waypoints.Add(wp);
                        wp.Created();
                    }
                }
            }
        }
    }
}
