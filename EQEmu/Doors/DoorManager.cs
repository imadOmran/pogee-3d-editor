using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

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

        public Door GetClosestDoor(Point3D point,double tolerance=10.0)
        {
            Door door = null;

            foreach (var d in Doors)
            {
                if (Functions.Distance(point, new Point3D(d.X, d.Y, d.Z)) < tolerance)
                {
                    return d;
                }
            }

            return door;
        }

        public string Zone
        {
            get { return _zone; }
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
                    _doors.Add(door);
                }
            }
            Created();
        }

        public Door DoorFactory()
        {
            Door door = null;
            int id = 0;
            int doorid = 0;

            if (Doors.Count > 0)
            {
                id = Doors.Max(x => x.Id) + 1;
                doorid = Doors.Max(x => x.DoorId) + 1;
            }

            door = new Door(_queryConfig);
            door.Id = id;
            door.DoorId = doorid;
            door.Zone = Zone;
            door.Created();            

            return door;
        }

        public void AddDoor(Door door)
        {
            //adds it from the database collections, ie what needs inserted/deleted/updated)
            AddObject(door);
            Doors.Add(door);
        }

        public void RemoveDoor(Door door)
        {
            //removes it from the database collections, ie what needs inserted/deleted/updated)
            RemoveObject(door);
            Doors.Remove(door);
        }

        public override void SaveXML(string dir)
        {
            using (var fs = new FileStream(dir + "\\" + this.Zone + "." + "0" + ".xml", FileMode.Create))
            {
                var ary = _doors.ToArray();
                var x = new XmlSerializer(ary.GetType());
                x.Serialize(fs, ary);
            }
        }

        public string GetSQL()
        {
            return GetQuery(Doors);
        }

        public override void LoadXML(string file)
        {
            //base.LoadXML(file);
            /*
            var filename = System.IO.Path.GetFileName(file);
            int period1 = filename.IndexOf('.', 0);
            int period2 = filename.IndexOf('.', period1 + 1);

            Zone = filename.Substring(0, period1);
            Version = int.Parse(filename.Substring(period1 + 1, period2 - period1 - 1));

            Spawn2[] spawns;
            using (var fs = new FileStream(file, FileMode.Open))
            {
                var x = new XmlSerializer(_spawns.ToArray().GetType());
                var obj = x.Deserialize(fs);
                spawns = obj as Spawn2[];
            }


            if (spawns != null)
            {
                ClearObjects();
                UnlockObject();
                foreach (var sp in spawns)
                {
                    AddSpawn(sp);
                    sp.Created();
                    sp.InitConfig(_queryConfig);
                }
                Created();
            }
            */
        }

        public override List<IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }
    }
}
