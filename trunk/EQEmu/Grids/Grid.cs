﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

using EQEmu.Database;

namespace EQEmu.Grids
{
    public class Grid : ManageDatabase
    {
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

        public string ZoneName
        {
            get;
            set;
        }

        private string _npcName;
        public string NPCName
        {
            get { return _npcName; }
            set
            {
                _npcName = value;
                Dirtied();
            }
        }

        //private List<Waypoint> _waypoints = new List<Waypoint>();
        private ObservableCollection<Waypoint> _waypoints = new ObservableCollection<Waypoint>();
        public ObservableCollection<Waypoint> Waypoints
        {
            get { return _waypoints; }
        }

        private int _id;
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

        private int _zoneid;
        public int ZoneId
        {
            get { return _zoneid; }
            set { _zoneid = value; }
        }

        private WanderTypes _wanderType;
        public WanderTypes WanderType
        {
            get { return _wanderType; }
            set
            {
                _wanderType = value;
                Dirtied();
            }
        }

        private PauseTypes _pauseType;
        public PauseTypes PauseType
        {
            get { return _pauseType; }
            set
            {
                _pauseType = value;
                Dirtied();
            }
        }

        public Grid(QueryConfig config)
            : base(config)
        {
            NPCName = "";
        }

        public void AddWaypoint()
        {
            Waypoint wp = GetNewWaypoint();
            AddWaypoint(wp);
        }

        public void AddWaypoint(Waypoint wp)
        {
            if (wp.GridId != Id || wp.ZoneId != ZoneId || wp.Number < GetNextNewWaypointNumber())
            {
                throw new Exception("Attempted to add conflicting waypoint, request a waypoint before adding via GetNewWaypoint method");
            }

            wp.GridReference = this;
            Waypoints.Add(wp);

            // object was finalized, any changes now are recorded
            if (CreatedObj)
            {
                NeedsInserted.Add(wp);
            }
        }

        public void RemoveWaypoint(Waypoint wp)
        {
            if (CreatedObj)
            {
                if (NeedsInserted.Contains(wp))
                {
                    //this waypoint was not retrieved from the database
                    NeedsInserted.Remove(wp);
                }
                else
                {
                    //waypoint was in the database
                    NeedsDeleted.Add(wp);
                }
            }

            Waypoints.Remove(wp);
        }

        public void RemoveAllWaypoints()
        {
            if (CreatedObj)
            {
                foreach (Waypoint wp in Waypoints)
                {
                    if (NeedsInserted.Contains(wp))
                    {
                        //this waypoint was not retrieved from the database
                        NeedsInserted.Remove(wp);
                    }
                    else
                    {
                        //waypoint was in the database
                        NeedsDeleted.Add(wp);
                    }
                }
            }
            Waypoints.Clear();
        }

        public void RemoveNonPauseNodes()
        {
            List<Waypoint> nopause = Waypoints.Where(x => x.PauseTime == 0).ToList();
            foreach (Waypoint wp in nopause)
            {
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

            string sql = Environment.NewLine + "-- NPC connection" + Environment.NewLine;
            var q = _queries.ExtensionQueries.FirstOrDefault(x => x.Name == "QueryByNPCName");

            switch (type)
            {
                case QueryType.UPDATE:
                case QueryType.INSERT:
                    sql += String.Format(q.UpdateQuery, ResolveArgs(q.UpdateArgs) );

                    //sql += String.Format("UPDATE spawn2,npc_types,spawnentry SET spawn2.pathgrid = @GridID",Id) + Environment.NewLine;
                    //sql += String.Format("WHERE npc_types.name = \"{0}\" AND",NPCName) + Environment.NewLine;
                    //sql += String.Format("spawn2.zone = \"{0}\" AND", ZoneName) + Environment.NewLine;
                    //sql += "npc_types.id = spawnentry.npcID AND" + Environment.NewLine;
                    //sql += "spawn2.spawngroupID = spawnentry.spawngroupID;" + Environment.NewLine;
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
                string sql = String.Format("SET @GridID = {0};",Id) + Environment.NewLine;
                //string sql = "";
                if(Dirty)
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

        //public override string InsertString
        //{
        //    get
        //    {
        //        //string sql = GetWaypointsQuery();
        //        string sql = String.Format("SET @GridID = {0};", Id) + Environment.NewLine;                

        //        sql += "INSERT INTO grid (id,zoneid,type,type2) VALUES " +
        //                String.Format("(@GridID,{1},{2},{3});", Id, ZoneId, (int)WanderType, (int)PauseType) + Environment.NewLine;
        //        sql += GetQuery(Waypoints);
        //        sql += GenerateQueryForNPC(QueryType.INSERT);

        //        return sql;
        //    }
        //}

        //public override string UpdateString
        //{
        //    get
        //    {
        //        //string sql = GetWaypointsQuery();
        //        string sql = String.Format("SET @GridID = {0};", Id) + Environment.NewLine;

        //        if (Dirty)
        //        {
        //            sql += String.Format("UPDATE grid SET type = {0}, type2 = {1} WHERE id = @GridID AND zoneid = {3};",
        //                    (int)WanderType, (int)PauseType, Id, ZoneId) + Environment.NewLine;
        //        }
        //        sql += GetQuery(Waypoints);
        //        sql += GenerateQueryForNPC(QueryType.INSERT);
        //        return sql;
        //    }
        //}

        //public override string DeleteString
        //{
        //    get
        //    {
        //        //string sql = GetWaypointsQuery();
        //        string sql = String.Format("SET @GridID = {0};", Id) + Environment.NewLine;
        //        sql += String.Format("DELETE FROM grid WHERE id = @GridID AND zoneid = {1};", Id, ZoneId) + Environment.NewLine;
        //        sql += GetQuery(Waypoints);
        //        return sql;
        //    }
        //}

        public Waypoint GetNewWaypoint()
        {
            int number = GetNextNewWaypointNumber();
            Waypoint wp = new Waypoint(Id, ZoneId, number,_queryConfig);
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

        public override List<IDatabaseObject> DirtyComponents
        {
            get
            {
                return Waypoints.Where(x => x.Dirty).ToList<IDatabaseObject>();
            }
        }
    }        
}