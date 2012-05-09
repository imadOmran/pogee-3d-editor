using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Zone
{
    public class ZonePointsDatabase : ZonePoints
    {
        private readonly MySqlConnection _connection;

        public ZonePointsDatabase(string zone, MySqlConnection connection, QueryConfig config)
            : base(zone,config)
        {
            _connection = connection;
            string sql = String.Format(SelectString, SelectArgValues);
            var results = QueryHelper.RunQuery(_connection, sql);

            ZonePoint zp;

            if (results != null)
            {
                foreach (Dictionary<string, object> row in results)
                {
                    zp = new ZonePoint(_queryConfig);
                    zp.SetProperties(Queries, row);
                    zp.Created();
                    Points.Add(zp);
                }
            }

            Created();
        }
    }
}
