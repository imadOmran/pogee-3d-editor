using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using System.Xml.Serialization;

using EQEmu.Database;

namespace EQEmu.Grids
{
    public class Grid : ManageDatabase
    {
        private bool _longOperationInProgress = false;
        private string _npcName = "";
        private ObservableCollection<Waypoint> _waypoints = new ObservableCollection<Waypoint>();

        private int _id;
        private int _zoneid;

        private WanderTypes _wanderType;
        private PauseTypes _pauseType;

        private readonly TypeQueriesExtension _queryByNpcName = null;

        public enum WanderTypes
        {
            Circular = 0,
            RandomNearest10Waypoints,
            Random,
            Patrol
        }

        public enum PauseTypes
        {
            RandomHalf = 0,
            Full,
            Random
        }

        private Grid()
            : base(null)
        {

        }

        public Grid(QueryConfig config)
            : base(config)
        {
            _queryByNpcName = _queries.GetExtensionQuery("QueryByNPCName");
            //_queryByNpcName = _queries.ExtensionQueries.FirstOrDefault(x => x.Name == "QueryByNPCName");
        }

        public void AddWaypoint()
        {
            Waypoint wp = GetNewWaypoint();
            AddWaypoint(wp);
        }

        public void AddWaypoint(Waypoint wp)
        {
            if (Waypoints.Contains(wp)) return;

            if (wp.GridId != Id || wp.ZoneId != ZoneId || wp.Number < GetNextNewWaypointNumber())
            {
                throw new Exception("Attempted to add conflicting waypoint, request a waypoint before adding via GetNewWaypoint method");
            }

            wp.GridReference = this;
            AddObject(wp);
            Waypoints.Add(wp);
        }

        public void RemoveWaypoint(Waypoint wp)
        {
            if (!Waypoints.Contains(wp)) return;

            RemoveObject(wp);
            Waypoints.Remove(wp);
        }

        public void RemoveWaypoints(IEnumerable<Waypoint> wps)
        {
            _longOperationInProgress = true;
            foreach (var wp in wps)
            {
                if (wp == wps.Last())
                {
                    _longOperationInProgress = false;
                }
                RemoveWaypoint(wp);
            }
        }

        public void RemoveAllWaypoints()
        {
            RemoveWaypoints(Waypoints.ToArray());
        }

        public void RemoveNonPauseNodes()
        {
            List<Waypoint> nopause = Waypoints.Where(x => x.PauseTime == 0).ToList();
            _longOperationInProgress = true;
            foreach (Waypoint wp in nopause)
            {
                if (wp == nopause.Last())
                {
                    _longOperationInProgress = false;
                }
                RemoveWaypoint(wp);
            }
        }

        public Waypoint GetNearestWaypoint(Point3D p, double threshhold = 5.0)
        {
            Waypoint wp = null;

            double min = 999.99;
            double dist = 0.0;

            foreach (Waypoint waypoint in Waypoints)
            {
                dist = Functions.Distance(p, new Point3D(waypoint.X, waypoint.Y, waypoint.Z));
                if (dist < min && dist <= threshhold)
                {
                    min = dist;
                    wp = waypoint;
                }
            }

            return wp;
        }

        public string GenerateQueryForNPC(QueryType type)
        {
            if (NPCName == "") return "";
            var q = _queryByNpcName;
            if (q == null) return "";

            string sql = Environment.NewLine + "-- NPC connection" + Environment.NewLine;
            switch (type)
            {
                case QueryType.UPDATE:
                case QueryType.INSERT:
                    sql += String.Format(q.UpdateQuery, ResolveArgs(q.UpdateArgs));
                    break;
                case QueryType.DELETE:
                    break;
            }

            return sql;
        }

        public override string ToString()
        {
            return string.Format("Grid: {0}, WP: {1}", Id, Waypoints.Count);
        }

        public override string InsertString
        {
            get
            {
                string sql = base.InsertString;
                sql += GetQuery(Waypoints);
                sql += GenerateQueryForNPC(QueryType.INSERT);
                return sql;
            }
        }

        public override string UpdateString
        {
            get
            {
                string sql = String.Format("SET @GridID = {0};", Id) + Environment.NewLine;
                //string sql = "";
                if (Dirty)
                {
                    sql += base.UpdateString;
                }
                sql += GetQuery(Waypoints);
                sql += GenerateQueryForNPC(QueryType.UPDATE);

                return sql;
            }
        }

        public override string DeleteString
        {
            get
            {
                string sql = String.Format("SET @GridID = {0};", Id) + Environment.NewLine;
                sql += base.DeleteString;
                sql += GetQuery(Waypoints);
                return sql;
            }
        }

        public Waypoint GetNewWaypoint()
        {
            int number = GetNextNewWaypointNumber();
            Waypoint wp = new Waypoint(Id, ZoneId, number, _queryConfig);
            wp.GridReference = this;
            wp.Created();

            return wp;
        }

        public int GetNextNewWaypointNumber()
        {
            int number = 0;

            if (Waypoints.Count > 0)
            {
                number = Waypoints.Max(x => x.Number) + 1;
            }

            return number;
        }

        [XmlIgnore]
        public override List<IDatabaseObject> DirtyComponents
        {
            get
            {
                return Waypoints.Where(x => x.Dirty).ToList<IDatabaseObject>();
            }
        }

        public string GetSQL()
        {
            throw new NotImplementedException();
        }

        public string ZoneName
        {
            get;
            set;
        }

        [Browsable(false)]
        [XmlIgnore]
        public ObservableCollection<Waypoint> Waypoints
        {
            get { return _waypoints; }
        }

        [Browsable(false)]
        public bool LongOperationInProgress
        {
            get { return _longOperationInProgress; }
        }

        public string NPCName
        {
            get { return _npcName; }
            set
            {
                _npcName = value;
                Dirtied();
            }
        }

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                foreach (Waypoint wp in Waypoints)
                {
                    wp.GridId = value;
                }
            }
        }

        public int ZoneId
        {
            get { return _zoneid; }
            set { _zoneid = value; }
        }

        
        public WanderTypes WanderType
        {
            get { return _wanderType; }
            set
            {
                _wanderType = value;
                Dirtied();
            }
        }

        public PauseTypes PauseType
        {
            get { return _pauseType; }
            set
            {
                _pauseType = value;
                Dirtied();
            }
        }
    }     
}
