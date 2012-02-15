using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using System.IO;
using System.Reflection;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public class ZoneSpawns : Database.ManageDatabase
    {
        private readonly MySqlConnection _connection;
        private string _zone;
        private int _version = 0;

        public string Zone
        {
            get { return _zone; }
        }

        public int Version
        {
            get { return _version; }
            set
            {
                _version = value;
            }
        }

        private ObservableCollection<Spawn2> _spawns = new ObservableCollection<Spawn2>();

        public ObservableCollection<Spawn2> Spawns
        {
            get
            {
                return _spawns;
            }
        }

        public ZoneSpawns(MySqlConnection conn, string zone, QueryConfig config, int version=0)
            : base(config)
        {
            _connection = conn;
            _zone = zone;
            _version = version;

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
                //string sql = String.Format("SELECT * FROM spawn2 WHERE zone='{0}'", zone);                
                //string sql = String.Format(_queries.SelectQuery, ResolveArgs(_queries.SelectArgs));
                string sql = String.Format(SelectString, SelectArgValues);
                MySqlCommand cmd = new MySqlCommand(sql, _connection);
                MySqlDataReader rdr = cmd.ExecuteReader();

                List<string> fields = new List<string>();
                for ( int i = 0; i < rdr.FieldCount; i++ ) {
                    fields.Add( rdr.GetName( i ) );
                }

                try
                {
                    Spawn2 s;

                    while (rdr.Read())
                    {
                        s = new Spawn2(_queryConfig);

                        foreach (var item in _queries.SelectQueryFields)
                        {
                            if (fields.Contains(item.Column))
                            {
                                SetProperty(s, item, rdr);
                            }
                        }

                        _spawns.Add(s);
                        s.Created();
                    }
                    rdr.Close();
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

        public void SaveQueryToFile(string file)
        {
            using (StreamWriter writer = new StreamWriter(file))
            {
                writer.Write(GetSQL());
                writer.Flush();
            }
        }

        public string GetSQL()
        {
            return GetQuery(Spawns);
        }

        public Spawn2 GetNewSpawn()
        {
            //TODO remove hardcoded queries - in case schema changes

            Spawn2 spawn = new Spawn2(_queryConfig);
            spawn.Zone = _zone;  

            int max = 1;
            if (Spawns.Count > 0)
            {
                max = Spawns.Max(x => x.Id) + 1;

                //test if this exists... if it does get the true max value from DB
                var query = String.Format("SELECT id FROM spawn2 WHERE id = {0}", max);
                bool exists = false;

                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    MySqlCommand cmd = new MySqlCommand(query, _connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();

                    try
                    {
                        while (rdr.Read())
                        {
                            exists = true;
                        }
                        rdr.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        if (rdr != null)
                        {
                            rdr.Close();
                        }
                    }

                    if (exists)
                    {
                        cmd = new MySqlCommand(
                            String.Format("SELECT MAX(id) AS id FROM spawn2"), _connection);
                        rdr = cmd.ExecuteReader();
                        try
                        {
                            if (rdr.Read())
                            {
                                max = rdr.GetInt32("id") + 1;
                            }
                            rdr.Close();
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
                }
            }
            spawn.Id = max;
            spawn.Version = _version;
            spawn.Created();
            return spawn;
        }

        public void AddSpawn(EQEmu.Spawns.Spawn2 spawn)
        {
        //    if (Spawns.Count > 0)
        //    {
        //        int max = Spawns.Max(x => x.Id);
        //        spawn.Id = max + 1;
        //    }
        //    else
        //    {
        //        spawn.Id = 1;
        //    }

            NeedsInserted.Add(spawn);
            Spawns.Add(spawn);
        }

        public void RemoveSpawn(Spawn2 spawn)
        {
            if (NeedsInserted.Contains(spawn))
            {
                NeedsInserted.Remove(spawn);
            }
            else
            {                
                NeedsDeleted.Add(spawn);
            }

            Spawns.Remove(spawn);
        }

        public Spawn2 GetNearestSpawn(Point3D p,double threshhold=2.0)
        {
            Spawn2 spawn = null;
            
            double min = 999.99;
            double dist = 0.0;

            foreach (Spawn2 sp in Spawns)
            {
                dist = Functions.Distance(p, new Point3D(sp.X, sp.Y, sp.Z));
                if (dist < min && dist <= threshhold)
                {
                    min = dist;
                    spawn = sp;
                }
            }

            return spawn;
        }

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Packs all currently loaded spawn entries by id field into the specified range; modifying their identifier field.
        /// i.e three spawns with id's (10,35,99) existing and PackTable called with parameters (5,10)
        /// will result in the three spawns having their id's updated to (5,6,7)
        /// </summary>
        /// <param name="start">the first id that will be used</param>
        /// <param name="end">the last id that will be used</param>
        public void PackTable(int start, int end)
        {
            if (start == end || start > end)
            {
                throw new ArgumentOutOfRangeException("Invalid range");
            }

            int range = end - start;
            if (_spawns.Count > range)
            {
                throw new ArgumentOutOfRangeException("Range specified not large enough");
            }

            IEnumerable<Spawn2> sorted = _spawns.OrderBy(x => x.Id);
            int i = start;
            NeedsInserted.Clear();
            foreach (var spawn in sorted)
            {
                //if we are going to potentially re-insert them all somewhere we might as well delete them
                //the update query generates the delete queries first so this works
                //create a spawn that keeps track of the identifier so we can delete it
                var copy = GetNewSpawn();
                copy.Id = spawn.Id;
                NeedsDeleted.Add(copy);

                spawn.Id = i;
                i += 1;
                NeedsInserted.Add(spawn);
            }
        }
    }
}
