using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Doors
{
    public class DoorManagerDatabase : DoorManager
    {
        private readonly MySqlConnection _connection;

        public DoorManagerDatabase(string zone, int version, MySqlConnection connection, QueryConfig config) 
            : base(zone, version, config)
        {
            _connection = connection;
            RetrieveDoors(zone);
            Created();
        }

        public void RetrieveDoors(string zone)
        {
            string sql = String.Format(SelectString, SelectArgValues);
            var results = QueryHelper.TryRunQuery(_connection, sql);

            if (results != null)
            {
                Door door;
                foreach (Dictionary<string, object> row in results)
                {
                    door = new Door(_queryConfig);
                    door.SetProperties(Queries, row);
                    door.Created();
                    Doors.Add(door);
                }
            }
        }
    }
}
