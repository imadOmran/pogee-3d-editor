using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.RoamAreas
{
    public class ZoneRoamAreas : Database.ManageDatabase
    {
        private readonly MySqlConnection _connection = null;
        private readonly string _zone = "";

        private ObservableCollection<RoamArea> _roamAreas = new ObservableCollection<RoamArea>();
        public ObservableCollection<RoamArea> RoamAreas
        {
            get
            {
                return _roamAreas;
            }
        }

        public string Zone
        {
            get { return _zone; }
        }

        public void SaveQueryToFile(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(GetSQL());
                writer.Flush();
            }
        }

        public string GetSQL()
        {
            return GetQuery(RoamAreas);
        }

        public int GetNextRoamAreaID()
        {
            int number = 1;

            if (RoamAreas.Count > 0)
            {
                number = RoamAreas.Max(x => x.Id) + 1;
            }

            return number;
        }

        public RoamArea NewArea()
        {
            int newId = GetNextRoamAreaID();
            var area = new RoamArea(newId, _queryConfig);
            area.Zone = this.Zone;
            area.Created();            
            return area;
        }

        public void AddArea(RoamArea area)
        {
            if (RoamAreas.FirstOrDefault(x => x.Id == area.Id) != null || area.Zone != this.Zone)
            {
                throw new Exception("Roam Area ID/Zone conflict");
            }

            RoamAreas.Add(area);

            // object was finalized, any changes now are recorded
            if (CreatedObj)
            {
                NeedsInserted.Add(area);
            }
        }

        public ZoneRoamAreas(MySqlConnection connection, string zone, QueryConfig config)
            : base(config)
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
                    //string sql = String.Format("SELECT id,min_z,max_z,spawn_random,pause_time,pause_variance FROM roam_areas WHERE zone = '{0}';", zone);
                    string sql = String.Format(SelectString, SelectArgValues);
                    MySqlCommand cmd = new MySqlCommand(sql, _connection);
                    rdr = cmd.ExecuteReader();

                    List<string> fields = new List<string>();
                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        fields.Add(rdr.GetName(i));
                    }

                    RoamArea ra;

                    while (rdr.Read())
                    {
                        ra = new RoamArea(_queryConfig);
                        //ra = new RoamArea(rdr.GetInt32("id"),_queryConfig)
                        //{
                        //    MaxZ = rdr.GetDouble("max_z"),
                        //    MinZ = rdr.GetDouble("min_z"),
                        //    PauseTime = rdr.GetInt32("pause_time"),
                        //    PauseVariance = rdr.GetInt32("pause_variance"),
                        //    SpawnRandom = rdr.GetBoolean("spawn_random")
                        //};

                        foreach (var item in _queries.SelectQueryFields)
                        {
                            if (fields.Contains(item.Column))
                            {
                                SetProperty(ra, item, rdr);
                            }
                        }

                        ra.Created();
                        RoamAreas.Add(ra);
                    }
                    rdr.Close();

                    foreach (var area in RoamAreas)
                    {
                        //sql = String.Format("SELECT x,y,num,roam_area_id FROM roam_area_vertices WHERE roam_area_id = '{0}' AND zone = '{1}' ORDER BY num;", area.Id, _zone);
                        sql = String.Format(area.SelectString, area.SelectArgValues);
                        cmd = new MySqlCommand(sql, _connection);
                        rdr = cmd.ExecuteReader();

                        fields.Clear();
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            fields.Add(rdr.GetName(i));
                        }

                        while (rdr.Read())
                        {
                            //RoamAreaEntry entry = new RoamAreaEntry(rdr.GetInt32("num"),rdr.GetInt32("roam_area_id"),_zone,_queryConfig)
                            //{
                            //    X = rdr.GetDouble("x"),
                            //    Y = rdr.GetDouble("y")
                            //};
                            var entry = new RoamAreaEntry(_queryConfig);

                            foreach (var item in area.Queries.SelectQueryFields)
                            {
                                if (fields.Contains(item.Column))
                                {
                                    SetProperty(entry, item, rdr);
                                }
                            }

                            entry.Created();
                            area.Vertices.Add(entry);
                        }
                        area.Created();

                        rdr.Close();
                    }
                    this.Created();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
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

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }

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
    }
}
