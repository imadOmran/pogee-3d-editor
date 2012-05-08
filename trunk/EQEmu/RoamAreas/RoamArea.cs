using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.RoamAreas
{
    public class RoamArea : Database.ManageDatabase
    {
        private int _id = 0;
        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                Dirtied();
            }
        }

        private bool _spawnRandom = true;
        public bool SpawnRandom
        {
            get{ return _spawnRandom; }
            set
            {
                _spawnRandom = value;
                Dirtied();
            }
        }

        private double _minZ = 0.0;
        public double MinZ
        {
            get { return _minZ; }
            set
            {
                _minZ = value;
                Dirtied();
            }
        }

        private double _maxZ = 0.0;
        public double MaxZ
        {
            get { return _maxZ; }
            set
            {
                _maxZ = value;
                Dirtied();
            }
        }

        private int _pauseTime = 0;
        public int PauseTime
        {
            get { return _pauseTime; }
            set
            {
                _pauseTime = value;
                Dirtied();
            }
        }

        private int _pauseVariance = 0;
        public int PauseVariance
        {
            get { return _pauseVariance; }
            set
            {
                _pauseVariance = value;
                Dirtied();
            }
        }

        private string _zone = "";
        public string Zone
        {
            get { return _zone; }
            set
            {
                _zone = value;
                Dirtied();
            }
        }

        private ObservableCollection<RoamAreaEntry> _vertices = new ObservableCollection<RoamAreaEntry>();
        public ObservableCollection<RoamAreaEntry> Vertices
        {
            get { return _vertices; }
        }

        public RoamArea(int id, QueryConfig config)
            : base(config)
        {
            _id = id;
        }

        public RoamArea(QueryConfig config)
            : base(config)
        {

        }

        public void AddEntry(Point3D point)
        {
            RoamAreaEntry entry = GetNewEntry();
            entry.X = point.X;
            entry.Y = point.Y;
            AddEntry(entry);        
        }

        public int GetNextNewEntryNumber()
        {
            int number = 0;

            if (Vertices.Count > 0)
            {
                number = Vertices.Max(x => x.Number) + 1;
            }

            return number;
        }

        public RoamAreaEntry GetNewEntry()
        {
            int newId = GetNextNewEntryNumber();
            RoamAreaEntry entry = new RoamAreaEntry(newId, this.Id, _zone, _queryConfig);
            entry.Created();

            return entry;
        }

        public void AddEntry()
        {
            RoamAreaEntry entry = new RoamAreaEntry(GetNextNewEntryNumber(), this.Id, _zone,_queryConfig);
            AddEntry(entry);
        }

        public void AddEntry(RoamAreaEntry entry)
        {
            if (entry.RoamAreaId != Id || entry.Zone != this.Zone || entry.Number < GetNextNewEntryNumber())
            {
                throw new Exception("Attempted to add conflicting roam area entry, request a roam area entry before adding via GetNewEntry method");
            }

            this.Vertices.Add(entry);
            
            // object was finalized, any changes now are recorded
            if (CreatedObj)
            {                
                NeedsInserted.Add(entry);
            }
        }

        public void RemoveAllEntries()
        {
            if (CreatedObj)
            {
                foreach (RoamAreaEntry entry in Vertices)
                {
                    if (NeedsInserted.Contains(entry))
                    {                     
                        NeedsInserted.Remove(entry);
                    }
                    else
                    {                        
                        NeedsDeleted.Add(entry);
                    }
                }
            }
            Vertices.Clear();
        }

        public void RemoveEntry(RoamAreaEntry entry)
        {
            if (CreatedObj)
            {
                if (NeedsInserted.Contains(entry))
                {
                    //this waypoint was not retrieved from the database
                    NeedsInserted.Remove(entry);
                }
                else
                {
                    //waypoint was in the database
                    NeedsDeleted.Add(entry);
                }
            }
            Vertices.Remove(entry);
        }

        public RoamAreaEntry GetClosestVertex(Point3D input)
        {            
            RoamAreaEntry vert = null;

            foreach (var point in Vertices)
            {                
                if (Functions.Distance(new Point3D(point.X, point.Y, 0), new Point3D(input.X, input.Y, 0)) < 5.0)
                {
                    vert = point;
                    break;
                }
            }

            return vert;
        }

        public override string DeleteString
        {
            get
            {
                string sql = String.Format("SET @RoamAreaID = {0};", Id) + Environment.NewLine;
                sql += base.DeleteString;
                sql += GetQuery(Vertices);
                return sql;
            }
        }

        public override string InsertString
        {
            get
            {
                string sql = String.Format("SET @RoamAreaID = {0};", Id) + Environment.NewLine;
                sql += base.InsertString;
                sql += GetQuery(Vertices);
                return sql;                
            }
        }

        public override string UpdateString
        {
            get
            {
                string sql = String.Format("SET @RoamAreaID = {0};", Id) + Environment.NewLine;
                //string sql = "";
                if (Dirty)
                {
                    sql += base.UpdateString;
                }
                sql += GetQuery(this.Vertices);
                return sql;
            }
        }

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get
            {
                return Vertices.Where(x => x.Dirty).ToList<Database.IDatabaseObject>();
            }
        }

        public string GetSQL()
        {
            throw new NotImplementedException();
        }
    }
}
