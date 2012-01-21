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

            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }

            if (_connection.State == System.Data.ConnectionState.Open)
            {
                MySqlDataReader rdr = null;
                try
                {
                    //string sql = String.Format("SELECT grid.* FROM grid INNER JOIN zone ON grid.zoneid = zone.zoneidnumber WHERE zone.short_name='{0}'", zone);
                    string sql = String.Format(SelectString, SelectArgValues);

                    MySqlCommand cmd = new MySqlCommand(sql, _connection);
                    rdr = cmd.ExecuteReader();

                    List<string> fields = new List<string>();
                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        fields.Add(rdr.GetName(i));
                    }

                    Grid g;

                    while (rdr.Read())
                    {
                        //ugly
                        //ZoneId = rdr.GetInt32("zoneid");
                        ZoneId = rdr.GetInt32(
                            _queries.SelectQueryFields.FirstOrDefault(x => x.Property == "ZoneId").Column );

                        g = new Grid(_queryConfig);

                        foreach (var item in _queries.SelectQueryFields)
                        {
                            if (fields.Contains(item.Column))
                            {
                                SetProperty(g, item, rdr);
                            }
                        }

                        //g = new Grid(_queryConfig)
                        //{
                        //    ZoneId = rdr.GetInt32("zoneid"),
                        //    ZoneName = zone,
                        //    WanderType = (Grid.WanderTypes)rdr.GetInt16("type"),
                        //    PauseType = (Grid.PauseTypes)rdr.GetInt16("type2"),
                        //    Id = rdr.GetInt32("id")
                        //};
                        Grids.Add(g);
                        g.Created();
                    }
                    rdr.Close();

                    foreach (Grid grid in Grids)
                    {
                        //sql = String.Format("SELECT * FROM grid_entries WHERE gridid = {0} AND zoneid = {1}", grid.Id, grid.ZoneId);
                        sql = String.Format(grid.SelectString, grid.SelectArgValues);
                        cmd = new MySqlCommand(sql, _connection);
                        rdr = cmd.ExecuteReader();

                        fields.Clear();
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            fields.Add(rdr.GetName(i));
                        }

                        Waypoint wp;

                        while (rdr.Read())
                        {
                            wp = new Waypoint(_queryConfig);
                            foreach (var item in grid.Queries.SelectQueryFields)
                            {
                                if (fields.Contains(item.Column))
                                {
                                    SetProperty(wp, item, rdr);
                                }
                            }

                            //wp = new Waypoint(rdr.GetInt32("gridid"), rdr.GetInt32("zoneid"), rdr.GetInt32("number"),_queryConfig)
                            //{
                            //    X = rdr.GetFloat("x"),
                            //    Y = rdr.GetFloat("y"),
                            //    Z = rdr.GetFloat("z"),
                            //    Heading = rdr.GetFloat("heading"),
                            //    PauseTime = rdr.GetInt32("pause"),
                            //    GridReference = grid,                                
                            //    Name = rdr.GetString("name"),
                            //    Running = rdr.GetInt32("running") > 0 ? true : false
                            //};

                            wp.GridReference = grid;

                            grid.Waypoints.Add(wp);
                            wp.Created();
                        }
                        rdr.Close();
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (rdr != null)
                    {
                        rdr.Close();
                    }
                }
            }
            else
            {
                throw new Exception("Unknown Connection State");
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
