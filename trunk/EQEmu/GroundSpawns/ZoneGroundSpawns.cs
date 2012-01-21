using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

using MySql.Data.MySqlClient;

namespace EQEmu.GroundSpawns
{
    public class ZoneGroundSpawns : EQEmu.Database.ManageDatabase
    {
        private int _zoneId;
        private string _zone;
        private MySqlConnection _connection;
        private ObservableCollection<GroundSpawn> _groundSpawns = new ObservableCollection<GroundSpawn>();

        public ObservableCollection<GroundSpawn> GroundSpawns
        {
            get { return _groundSpawns; }
        }

        public string Zone
        {
            get { return _zone; }
            set
            {
                _zone = value;
            }
        }

        private void SubscribeToChanges()
        {
            _groundSpawns.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_groundSpawns_CollectionChanged);
        }

        void _groundSpawns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (GroundSpawn spawn in e.NewItems.Cast<GroundSpawn>())
                    {
                        spawn.ItemIDChanged += new GroundSpawn.ItemIDChangedHandler(spawn_ItemIDChanged);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (GroundSpawn spawn in e.OldItems.Cast<GroundSpawn>())
                    {
                        spawn.ItemIDChanged -= spawn_ItemIDChanged;
                    }
                    break;
                default:
                    break;
            }
        }

        public string GetSQL()
        {
            return GetQuery(GroundSpawns);
        }

        void spawn_ItemIDChanged(object sender, ItemIDChangedEventArgs args)
        {
            var spawn = sender as GroundSpawn;
            if (spawn == null) return;
            
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                MySqlDataReader rdr = null;
                try
                {                    
                    //string sql = String.Format(SelectString, SelectArgValues);
                    var q = spawn.Queries.ExtensionQueries.FirstOrDefault( x => x.Name == "GetItemName" );
                    string sql = String.Format(q.SelectQuery, spawn.ResolveArgs(q.SelectArgs) );

                    MySqlCommand cmd = new MySqlCommand(sql, _connection);
                    rdr = cmd.ExecuteReader();

                    List<string> fields = new List<string>();
                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        fields.Add(rdr.GetName(i));
                    }

                    while (rdr.Read())
                    {
                        foreach (var item in q.SelectQueryFields)
                        {
                            if (fields.Contains(item.Column))
                            {
                                SetProperty(spawn, item, rdr);
                            }
                        }
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

        public ZoneGroundSpawns(MySqlConnection connection, string zone, Database.QueryConfig config) :
            base(config)
        {
            SubscribeToChanges();

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
                    string sql = String.Format(SelectString, SelectArgValues);
                    MySqlCommand cmd = new MySqlCommand(sql, _connection);
                    rdr = cmd.ExecuteReader();

                    List<string> fields = new List<string>();
                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        fields.Add(rdr.GetName(i));
                    }

                    GroundSpawn gspawn;
                    while (rdr.Read())
                    {
                        _zoneId = rdr.GetInt32(
                            _queries.SelectQueryFields.FirstOrDefault(x => x.Property == "ZoneId").Column);

                        gspawn = new GroundSpawn(_queryConfig);

                        foreach (var item in _queries.SelectQueryFields)
                        {
                            if (fields.Contains(item.Column))
                            {
                                SetProperty(gspawn, item, rdr);
                            }
                        }

                        _groundSpawns.Add(gspawn);
                        gspawn.Created();
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

        public GroundSpawn GetClosestGroundSpawn(Point3D point, double threshhold=2.0)
        {
            GroundSpawn g = null;

            foreach (var item in _groundSpawns)
            {
                var middle = item.MidPoint;
                if (point.X <= item.MaxX + 2 && point.X >= item.MinX - 2 &&
                    point.Y <= item.MaxY + 2 && point.Y >= item.MinY - 2 &&
                    point.Z <= item.MaxZ + 2 && point.Z >= item.MaxZ - 2)
                {
                    var dist = Functions.Distance(point, item.MidPoint);
                    if (dist <= threshhold)
                    {
                        if (g == null) g = item;
                        else
                        {
                            if (dist < Functions.Distance(point, g.MidPoint))
                            {
                                g = item;
                            }
                        }
                    }
                }
            }

            return g;
        }

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }
    }
}
