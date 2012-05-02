using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

using MySql.Data.MySqlClient;

namespace EQEmu.Grids
{
    public delegate void GridDataLoadedHandler(object sender, GridDataLoadedEventArgs e);
    public class GridDataLoadedEventArgs : EventArgs
    {
        public GridDataLoadedEventArgs(int zoneid, string zonename)
        {
            ZoneId = zoneid;
            ZoneName = zonename;
        }

        public int ZoneId { get; private set; }
        public string ZoneName { get; private set; }
    }

    public class ZoneGrids : EQEmu.Database.ManageDatabase
    {
        private readonly MySqlConnection _connection;
        private string _zone;

        public event GridDataLoadedHandler GridDataLoaded;
        private void OnGridDataLoaded(string zonename, int zoneid)
        {
            var e = GridDataLoaded;
            if (e != null)
            {
                GridDataLoaded(this, new GridDataLoadedEventArgs(zoneid, zonename));
            }
        }

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
                    foreach (var wp in g.Waypoints)
                    {
                        wp.ZoneId = value;
                    }
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
            var results = Database.QueryHelper.TryRunQuery(_connection, sql);
            var v = _queries.SelectQueryFields.FirstOrDefault(x => x.Property == "ZoneId");
            string zoneIdField = null;
            if (v != null)
            {
                zoneIdField = v.Column;
            }

            if (results != null)
            {

                foreach (var row in results)
                {
                    Grid g = new Grid(_queryConfig);
                    if (row.ContainsKey(zoneIdField) && results.First() == row)
                    {
                        ZoneId = Int32.Parse(row[zoneIdField].ToString());
                    }

                    g.SetProperties(Queries, row);
                    Grids.Add(g);
                    g.Created();
                }

                foreach (var grid in Grids)
                {
                    sql = String.Format(grid.SelectString, grid.SelectArgValues);
                    results = Database.QueryHelper.RunQuery(_connection, sql);
                    foreach (var row in results)
                    {
                        var wp = new Waypoint(_queryConfig);

                        wp.SetProperties(grid.Queries, row);

                        wp.GridReference = grid;
                        grid.Waypoints.Add(wp);
                        wp.Created();
                    }
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

        /// <summary>
        /// Provide a directory path to save to
        /// </summary>
        /// <param name="file"></param>
        public override void SaveXML(string dir)
        {
            using (var fs = new FileStream(dir + "\\" + this.Zone + ".grids.xml", FileMode.Create))
            {
                var ary = _grids.ToArray();
                var x = new XmlSerializer(ary.GetType());
                x.Serialize(fs, ary);
            }

            using (var fs = new FileStream(dir + "\\" + this.Zone + ".waypoints.xml", FileMode.Create))
            {
                List<Waypoint> allwaypoints = new List<Waypoint>();
                foreach (var g in _grids)
                {
                    allwaypoints.AddRange(g.Waypoints);
                }

                var ary = allwaypoints.ToArray();
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

            Zone = filename.Substring(0, period1);
            
            Grid[] grids;
            using (var fs = new FileStream(file, FileMode.Open))
            {
                var x = new XmlSerializer(_grids.ToArray().GetType());
                var obj = x.Deserialize(fs);
                grids = obj as Grid[];
            }

            if (grids != null)
            {
                ClearObjects();
                Grids.Clear();
                Created();
                foreach (var grid in grids)
                {
                    Grids.Add(grid);
                    AddObject(grid);
                    grid.InitConfig(_queryConfig);
                    grid.Created();
                }
            }

            Waypoint[] wps;
            var wpfile = dir + "\\" + Zone + ".waypoints.xml";

            if (grids.Count() > 0 && System.IO.File.Exists(wpfile) )
            {
                this._zone = grids.ElementAt(0).ZoneName;
                this._zoneId = grids.ElementAt(0).ZoneId;

                using (var fs = new FileStream(wpfile, FileMode.Open))
                {
                    var x = new XmlSerializer(grids.ElementAt(0).Waypoints.ToArray().GetType());
                    var obj = x.Deserialize(fs);
                    wps = obj as Waypoint[];
                }

                foreach (var wp in wps)
                {
                    var grid = grids.FirstOrDefault(x => x.Id == wp.GridId);
                    if (grid != null)
                    {
                        grid.AddWaypoint(wp);
                        wp.Created();
                        wp.InitConfig(_queryConfig);
                    }
                }
            }
            OnGridDataLoaded(Zone, ZoneId);
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
