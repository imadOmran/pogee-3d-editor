using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;

using MySql.Data.MySqlClient;

namespace EQEmu.Grids
{
    public class ZoneGrids : EQEmu.Database.ManageDatabase
    {
        private readonly MySqlConnection _connection;
        private string _zone;        

        //private List<Grid> _grids = new List<Grid>();
        private ObservableCollection<Grid> _grids = new ObservableCollection<Grid>();
        public ObservableCollection<Grid> Grids
        {
            get { return _grids; }
        }

        public string Zone
        {
            get { return _zone; }
            set 
            { 
                _zone = value;
                foreach (Grid g in Grids)
                {
                    g.ZoneName = value;
                }
            }
        }

        private int _zoneId;
        public int ZoneId
        {
            get { return _zoneId; }
            set
            {
                _zoneId = value;
                foreach (Grid g in Grids)
                {
                    g.ZoneId = value;
                }
            }
        }

        public ZoneGrids(EQEmu.Database.QueryConfig config)
            :base(config)
        {

        }

        public ZoneGrids(MySqlConnection connection, string zone, EQEmu.Database.QueryConfig config)
            :base(config)
        {
            _connection = connection;
            _zone = zone;

            if (_connection == null)
            {
                throw new NullReferenceException();
            }

            var sql = String.Format(SelectString, SelectArgValues);
            var results = Database.QueryHelper.RunQuery(_connection, sql);
            var v = _queries.SelectQueryFields.FirstOrDefault(x => x.Property == "ZoneId");
            string zoneIdField = null;
            if (v != null)
            {
                zoneIdField = v.Column;
            }

            foreach (var row in results)
            {
                Grid g = new Grid(_queryConfig);
                if (row.ContainsKey(zoneIdField) && results.First() == row)
                {
                    ZoneId = Int32.Parse( row[zoneIdField].ToString() );
                }

                g.SetProperties(Queries, row);
                Grids.Add(g);
                g.Created();
            }

            foreach(var grid in Grids)
            {
                sql = String.Format(grid.SelectString,grid.SelectArgValues);
                results = Database.QueryHelper.RunQuery(_connection,sql);
                foreach(var row in results)
                {
                    var wp = new Waypoint(_queryConfig);

                    wp.SetProperties(grid.Queries,row);

                    wp.GridReference = grid;
                    grid.Waypoints.Add(wp);
                    wp.Created();
                }
            }
        }

        public void AddGrid(Grid grid)
        {
            if (Grids.Count > 0)
            {
                int max = Grids.Max(x => x.Id);
                grid.Id = max + 1;
            }
            else
            {
                grid.Id = 1;
            }

            NeedsInserted.Add(grid);
            Grids.Add(grid);
        }

        public void RemoveGrid(Grid grid)
        {
            if (NeedsInserted.Contains(grid))
            {
                NeedsInserted.Remove(grid);
            }
            else
            {
                grid.RemoveAllWaypoints();
                NeedsDeleted.Add(grid);
            }
            
            Grids.Remove(grid);
        }

        public string GetSQL()
        {
            return GetQuery(Grids);
        }

        public string GetSQL(Grid grid)
        {
            List<Grid> g = new List<Grid>();
            g.Add(grid);

            return GetQuery(g);
        }
        
        public Grid GetNewGrid()
        {
            Grid grid = new Grid(_queryConfig);
            grid.ZoneId = ZoneId;
            grid.ZoneName = Zone;

            int max = 1;
            if (Grids.Count > 0)
            {
                max = Grids.Max(x => x.Id) + 1;
            }

            grid.Created();
            return grid;
        }

        public void SaveQueryToFile(string file)
        {
            using (StreamWriter writer = new StreamWriter(file))
            {
                writer.Write(GetQuery(Grids));
                writer.Flush();
            }
        }

        /*
        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get
            {
                return this.Grids.Where( x => Dirty
            }
        }
        */

        //public override string InsertString
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public override string UpdateString
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public override string DeleteString
        //{
        //    get { throw new NotImplementedException(); }
        //}

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }
    }        
}
