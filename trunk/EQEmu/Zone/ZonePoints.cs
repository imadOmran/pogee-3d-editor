using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using System.IO;
using System.Xml.Serialization;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Zone
{
    public delegate void ZonePointDataLoadedHandler(object sender, ZonePointDataLoadedEventArgs e);
    public class ZonePointDataLoadedEventArgs : EventArgs
    {
        public ZonePointDataLoadedEventArgs(string zonename)
        {
            ZoneName = zonename;
        }
        public string ZoneName { get; private set; }
    }    

    public class ZonePoints : Database.ManageDatabase
    {
        private readonly MySqlConnection _connection;
        private string _zone;        
        private ObservableCollection<ZonePoint> _zonePoints = new ObservableCollection<ZonePoint>();

        public event ZonePointDataLoadedHandler DataLoaded;
        private void OnZonePointDataLoaded(string zone)
        {
            var e = DataLoaded;
            if (e != null)
            {
                e(this, new ZonePointDataLoadedEventArgs(zone));
            }
        }

        public ZonePoints(MySqlConnection conn, string zone, QueryConfig config)
            : base(config)
        {
            _connection = conn;
            _zone = zone;

            string sql = String.Format(SelectString, SelectArgValues);
            var results = QueryHelper.TryRunQuery(conn,sql);

            ZonePoint zp;

            if (results != null)
            {
                foreach (Dictionary<string, object> row in results)
                {
                    zp = new ZonePoint(_queryConfig);
                    zp.SetProperties(Queries, row);
                    zp.Created();
                    _zonePoints.Add(zp);
                }
            }

            Created();
        }

        public ObservableCollection<ZonePoint> Points
        {
            get { return _zonePoints; }
        }

        public string Zone 
        {
            get { return _zone; }
        }

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }

        public ZonePoint GetClosestPoint(Point3D p,bool destCounts=false)
        {
            ZonePoint zp = null;
            double closest = 25;
            double dist;
            double destDist;

            foreach (var pt in _zonePoints)
            {
                dist = Functions.Distance(p,new Point3D(pt.X,pt.Y,pt.Z));
                if (destCounts)
                {
                    destDist = Functions.Distance(p, new Point3D(pt.TargetX, pt.TargetY, pt.TargetZ));
                    if (destDist < dist)
                    {
                        dist = destDist;
                    }
                }

                if ( dist < closest )
                {
                    closest = dist;
                    zp = pt;
                }
            }

            return zp;
        }

        public ZonePoint Create()
        {
            ZonePoint zp = new ZonePoint(_queryConfig);
            zp.Zone = Zone;
            if (_zonePoints.Count > 0)
            {
                zp.Number = _zonePoints.Max(x => x.Number) + 1;
            }
            else zp.Number = 1;
            zp.Created();
            Add(zp);

            return zp;
        }

        public void Add(ZonePoint zp)
        {
            NeedsInserted.Add(zp);
            _zonePoints.Add(zp);
        }

        public void Remove(ZonePoint zp)
        {
            if (NeedsInserted.Contains(zp))
            {
                NeedsInserted.Remove(zp);
            }
            else
            {
                NeedsDeleted.Add(zp);
            }

            _zonePoints.Remove(zp);
        }

        public string GetSQL()
        {
            return GetQuery(Points);
        }

        /// <summary>
        /// Save as XML to directory
        /// </summary>
        /// <param name="dir"></param>
        public override void SaveXML(string dir)
        {
            using (var fs = new FileStream(dir + "\\" + this.Zone + ".zonepoints.xml", FileMode.Create))
            {
                var ary = _zonePoints.ToArray();
                var x = new XmlSerializer(ary.GetType());
                x.Serialize(fs, ary);
            }
        }

        public override void LoadXML(string file)
        {
            var dir = System.IO.Path.GetDirectoryName(file);
            var filename = System.IO.Path.GetFileName(file);
            int period1 = filename.IndexOf('.', 0);
            int period2 = filename.IndexOf('.', period1 + 1);

            _zone = filename.Substring(0, period1);

            ZonePoint[] zps;
            using (var fs = new FileStream(file, FileMode.Open))
            {
                var x = new XmlSerializer(_zonePoints.ToArray().GetType());
                var obj = x.Deserialize(fs);
                zps = obj as ZonePoint[];
            }

            if (zps.Count() > 0)
            {
                ClearObjects();
                _zonePoints.Clear();
                Created();
                foreach (var zp in zps)
                {
                    AddObject(zp);
                    _zonePoints.Add(zp);
                    zp.InitConfig(_queryConfig);
                    zp.Created();
                }
                OnZonePointDataLoaded(_zone);
            }
        }

    }
}
