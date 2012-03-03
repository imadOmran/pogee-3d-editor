using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Zone
{
    public class ZonePoint : Database.DatabaseObject
    {
        private int _id;
        private int _version;
        private int _number;
        private int _clientVersionMask;
        private int _targetZoneId;
        private int _targetInstance;

        private string _zone;
        
        private float _x;
        private float _y;
        private float _z;
        private float _targetX;
        private float _targetY;
        private float _targetZ;
        private float _heading;
        private float _targetHeading;
        private float _buffer;

        public ZonePoint(QueryConfig config) : base(config)
        {

        }

        public float Buffer
        {
            get { return _buffer; }
            set
            {
                _buffer = value;
                Dirtied();
            }
        }

        public float TargetHeading
        {
            get { return _targetHeading; }
            set
            {
                _targetHeading = value;
                Dirtied();
            }
        }

        public float Heading
        {
            get { return _heading; }
            set
            {
                _heading = value;
                Dirtied();
            }
        }

        public float X
        {
            get { return _x; }
            set
            {
                _x = value;
                Dirtied();
            }
        }

        public float Y
        {
            get { return _y; }
            set
            {
                _y = value;
                Dirtied();
            }
        }

        public float Z
        {
            get { return _z; }
            set
            {
                _z = value;
                Dirtied();
            }
        }

        public float TargetX
        {
            get { return _targetX; }
            set
            {
                _targetX = value;
                Dirtied();
            }
        }

        public float TargetY
        {
            get { return _targetY; }
            set
            {
                _targetY = value;
                Dirtied();
            }
        }

        public float TargetZ
        {
            get { return _targetZ; }
            set
            {
                _targetZ = value;
                Dirtied();
            }
        }
        
        public string Zone
        {
            get { return _zone; }
            set
            {
                _zone = value;
                Dirtied();
            }
        }

        public int TargetInstance
        {
            get { return _targetInstance; }
            set
            {
                _targetInstance = value;
                Dirtied();
            }
        }

        public int TargetZoneId
        {
            get { return _targetZoneId; }
            set
            {
                _targetZoneId = value;
                Dirtied();
            }
        }

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                Dirtied();
            }
        }

        public int Version
        {
            get { return _version; }
            set
            {
                _version = value;
                Dirtied();
            }
        }

        public int Number
        {
            get { return _number; }
            set
            {
                _number = value;
                Dirtied();
            }
        }

        public int ClientVersionMask
        {
            get { return _clientVersionMask; }
            set
            {
                _clientVersionMask = value;
                Dirtied();
            }
        }

        public override string ToString()
        {
            return this.Zone.ToString() + " to " + this.TargetZoneId.ToString();
        }
    }

    public class ZonePoints : Database.ManageDatabase
    {
        private readonly MySqlConnection _connection;
        private readonly string _zone;        
        private ObservableCollection<ZonePoint> _zonePoints = new ObservableCollection<ZonePoint>();

        public ZonePoints(MySqlConnection conn, string zone, QueryConfig config)
            : base(config)
        {
            _connection = conn;
            _zone = zone;

            string sql = String.Format(SelectString, SelectArgValues);
            var results = QueryHelper.RunQuery(conn,sql);

            ZonePoint zp;
            foreach (Dictionary<string, object> row in results)
            {
                zp = new ZonePoint(_queryConfig);
                zp.SetProperties(Queries, row);
                zp.Created();
                _zonePoints.Add(zp);
            }
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

    }
}
