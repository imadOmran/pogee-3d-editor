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

            //this is dependant on the FilterId property - per config.xml...I don't like this
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

        public IEnumerable<SpawnGroup> LookupByZone(string zone)
        {
            ClearCache();

            var query = Queries.ExtensionQueries.FirstOrDefault( x => x.Name == "GetByZone" );
            if (query != null)
            {
                string sql = String.Format(query.SelectQuery, new string[]{ zone } );
                var results = Database.QueryHelper.RunQuery(_connection, sql);
                foreach (var row in results)
                {
                    FilterById = (int)row["spawngroupid"];
                }
                return _spawnGroups;
            }
            else return null;
        }

        public void PackCachedId(int start, int end)
        {
            if (start == end || start > end)
            {
                throw new ArgumentOutOfRangeException("Invalid range");
            }

            int range = end - start;
            if( SpawnGroups.Count() > range)
            {
                throw new ArgumentOutOfRangeException("Range specified not large enough");
            }

            //spawngroups are associated with 0-n spawn2 entries so these will need updated            

            IEnumerable<SpawnGroup> sorted = SpawnGroups.OrderBy(x => x.Id);
            int i = start;
            NeedsInserted.Clear();
            foreach (var spawn in sorted)
            {
                //if we are going to potentially re-insert them all somewhere we might as well delete them
                //the update query generates the delete queries first so this works
                //create a spawn that keeps track of the identifier so we can delete it
                var copy = new SpawnGroup(spawn.Id, _connection, _queryConfig);                
                NeedsDeleted.Add(copy);

                spawn.UnlockObject();
                spawn.Id = i;
                spawn.Created();

                i += 1;
                NeedsInserted.Add(spawn);
            }
        }

        public string GetSQL()
        {
            return GetQuery(SpawnGroups);
        }
        
        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }
    }
}
