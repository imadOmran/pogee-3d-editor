using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Collections.ObjectModel;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Doors
{
    public class DoorManager : ManageDatabase
    {
        private readonly MySqlConnection _connection;
        private string _zone;

        private ObservableCollection<Door> _doors = new ObservableCollection<Door>();
        public ObservableCollection<Door> Doors
        {
            get
            {
                return _doors;
            }
        }

        public DoorManager(string zone,MySqlConnection connection,QueryConfig config) : base(config)
        {
            _zone = zone;
            _connection = connection;
            RetrieveDoors(zone);
        }

        public string Zone
        {
            get { return _zone; }
        }

        public void RetrieveDoors(string zone)
        {
            string sql = String.Format(SelectString, SelectArgValues);
            var results = QueryHelper.RunQuery(_connection, sql);

            Door door;
            foreach (Dictionary<string, object> row in results)
            {
                door = new Door(_queryConfig);
                door.SetProperties(Queries, row);
                door.Created();
                _doors.Add(door);
            }
        }

        public override List<IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }
    }
}
