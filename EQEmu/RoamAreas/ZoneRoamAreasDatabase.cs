using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.RoamAreas
{
    public class ZoneRoamAreasDatabase : ZoneRoamAreas
    {
        private readonly MySqlConnection _connection = null;

        public ZoneRoamAreasDatabase(string zone, MySqlConnection connection, QueryConfig config)
            : base(zone, config)
        {
            var sql = String.Format(SelectString, SelectArgValues);
            var results = Database.QueryHelper.RunQuery(_connection, sql);

            foreach (var row in results)
            {
                var ra = new RoamArea(_queryConfig);
                ra.SetProperties(Queries, row);
                RoamAreas.Add(ra);
            }

            foreach (var area in RoamAreas)
            {
                sql = String.Format(area.SelectString, area.SelectArgValues);
                results = Database.QueryHelper.RunQuery(_connection, sql);
                foreach (var row in results)
                {
                    var entry = new RoamAreaEntry(_queryConfig);
                    entry.SetProperties(area.Queries, row);
                    area.Vertices.Add(entry);
                    entry.Created();
                }

                area.Created();
            }

            this.Created();
        }
    }
}
