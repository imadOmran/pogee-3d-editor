using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

namespace EQEmu.Spawns
{
    public class SpawnGroupAggregator : Database.ManageDatabase
    {
        private readonly MySqlConnection _connection;

        private int _filterById;
        public int FilterById
        {
            get { return _filterById; }
            set
            {
                _filterById = value;
                Lookup(value);
            }
        }

        private List<SpawnGroup> _spawnGroups = new List<SpawnGroup>();
        public IEnumerable<SpawnGroup> SpawnGroups
        {
            get { return _spawnGroups; }
        }

        public SpawnGroupAggregator(MySqlConnection connection,Database.QueryConfig queryConfig)
            : base(queryConfig)
        {
            if (connection == null) 
                throw new NullReferenceException("Database connection ref cannot be null");
            _connection = connection;
        }

        public void RemoveSpawnGroup(SpawnGroup group)
        {
            if (NeedsInserted.Contains(group))
            {
                NeedsInserted.Remove(group);
            }
            else
            {
                NeedsDeleted.Add(group);
            }
            group.RemoveAllEntries();
            _spawnGroups.Remove(group);
        }

        public void AddSpawnGroup(SpawnGroup group)
        {
            NeedsInserted.Add(group);
            _spawnGroups.Add(group);
        }

        public void ClearCache()
        {
            NeedsDeleted.Clear();
            NeedsInserted.Clear();
            _spawnGroups.Clear();
        }

        public SpawnGroup CreateSpawnGroup()
        {
            SpawnGroup sg = null;

            var qconf = Queries.ExtensionQueries.FirstOrDefault(x => { return x.Name == "GetMaxID"; });
            var results = Database.QueryHelper.RunQuery(_connection, qconf.SelectQuery);
            var row = results.FirstOrDefault();
            if (row != null)
            {
                sg = new SpawnGroup(_connection, _queryConfig);
                sg.SetProperties(qconf, row);
                sg.Id += 1;

                if (SpawnGroups.Count() > 0)
                {
                    //in case we created/generated an entry not yet in the database 
                    //look in the collection for a max Id
                    var maxLoaded = SpawnGroups.Max(x => { return x.Id; });
                    if (maxLoaded >= sg.Id)
                    {
                        sg.Id = maxLoaded + 1;
                    }
                }
                
                sg.Created();
                AddSpawnGroup(sg);
            }          

            return sg;
        }
        
        private SpawnGroup Lookup(int id)
        {
            SpawnGroup sg = _spawnGroups.Where(x => { return x.Id == id; }).FirstOrDefault();
            if (sg != null) return sg;

            string sql = String.Format(SelectString, SelectArgValues);
            var results = Database.QueryHelper.RunQuery(_connection, sql);
            var row = results.FirstOrDefault();
            if (row != null)
            {
                sg = new SpawnGroup(_connection, _queryConfig);
                sg.SetProperties(Queries, row);
                _spawnGroups.Add(sg);
                sg.Created();
                sg.GetEntries();
            }

            return sg;
        }

        public string GetSQL()
        {
            return GetQuery(SpawnGroups);
        }

        //public SpawnGroup Lookup(string name)
        //{
        //    SpawnGroup sg = null;

        //    if (_connection.State != System.Data.ConnectionState.Open)
        //    {
        //        _connection.Open();
        //    }

        //    if (_connection.State == System.Data.ConnectionState.Open)
        //    {
        //        MySqlDataReader rdr = null;
        //        try
        //        {
        //            string sql = String.Format(SelectString, SelectArgValues);
        //            MySqlCommand cmd = new MySqlCommand(sql, _connection);
        //            rdr = cmd.ExecuteReader();

        //            List<string> fields = new List<string>();
        //            for (int i = 0; i < rdr.FieldCount; i++)
        //            {
        //                fields.Add(rdr.GetName(i));
        //            }

        //            while (rdr.Read())
        //            {
        //                sg = new SpawnGroup(_queryConfig);

        //                foreach (var item in _queries.SelectQueryFields)
        //                {
        //                    if (fields.Contains(item.Column))
        //                    {
        //                        SetProperty(sg, item, rdr);
        //                    }
        //                }

        //                _spawnGroups.Add(sg);
        //                sg.Created();
        //            }
        //            rdr.Close();

        //        }
        //        catch (Exception e)
        //        {
        //            //TODO log this
        //            Console.WriteLine(e.Message);
        //        }
        //        finally
        //        {
        //            if (rdr != null)
        //            {
        //                rdr.Close();
        //            }
        //        }

        //    }
        //    else
        //    {
        //        throw new Exception("Database connection not open");
        //    }

        //    return sg;
        //}

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }
    }
}
