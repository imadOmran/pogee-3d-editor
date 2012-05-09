using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

using MySql.Data.MySqlClient;

using EQEmu.Database;

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

    public abstract class ZoneGrids : EQEmu.Database.ManageDatabase
    {        
        private string _zone;
        private int _zoneId;

        private ObservableCollection<Grid> _grids = new ObservableCollection<Grid>();

        public event GridDataLoadedHandler GridDataLoaded;

        public ZoneGrids(string zone,QueryConfig config)
            : base(config)
        {
            _zone = zone;
        }

        public void AddGrid(Grid grid)
        {
            if (Grids.Contains(grid)) return;

            if (Grids.Count > 0)
            {
                int max = Grids.Max(x => x.Id);
                grid.Id = max + 1;
            }
            else
            {
                grid.Id = 1;
            }

            AddObject(grid);
            Grids.Add(grid);
        }

        public void RemoveGrid(Grid grid)
        {
            if (!Grids.Contains(grid)) return;
            RemoveObject(grid);
            grid.RemoveAllWaypoints();
            Grids.Remove(grid);            
        }
        
        private void OnGridDataLoaded(string zonename, int zoneid)
        {
            var e = GridDataLoaded;
            if (e != null)
            {
                GridDataLoaded(this, new GridDataLoadedEventArgs(zoneid, zonename));
            }
        }
        
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

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }
    }        
}
