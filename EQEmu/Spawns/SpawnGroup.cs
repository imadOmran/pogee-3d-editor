using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using MySql.Data.MySqlClient;

namespace EQEmu.Spawns
{
    public class SpawnEntry : Database.DatabaseObject
    {
        private int _spawnGroupId;
        private int _npcId;
        private short _chance;

        public int SpawnGroupID
        {
            get { return _spawnGroupId; }
            set
            {
                _spawnGroupId = value;
                Dirtied();
            }
        }

        public int NpcID
        {
            get { return _npcId; }
            set
            {
                _npcId = value;
                Dirtied();
            }
        }

        public short Chance
        {
            get { return _chance; }
            set
            {
                _chance = value;
                Dirtied();
            }
        }

        public string NpcName
        { 
            get; 
            set; 
        }

        public short NpcLevel
        {
            get;
            set;
        }

        public SpawnEntry(Database.QueryConfig config) : base(config)
        {

        }
    }

    public class SpawnGroup : Database.ManageDatabase
    {
        #region Private Fields
        private int _id;
        private string _name;
        private short _spawnLimit;

        private int _roamDelay;
        private float _roamDistance;
        private float _minRoamX;
        private float _maxRoamX;
        private float _minRoamY;
        private float _maxRoamY;
        #endregion

        #region Properties

        public int Id
        {
            get { return _id; }
            set
            {
                if (CreatedObj) throw new Exception("Cannot modify ID field");
                _id = value;
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        public short SpawnLimit
        {
            get { return _spawnLimit; }
            set
            {
                _spawnLimit = value;
                Dirtied();
            }
        }

        public float RoamingDistance
        {
            get { return _roamDistance; }
            set
            {
                _roamDistance = value;
                Dirtied();
            }
        }

        public float MinRoamingX
        {
            get { return _minRoamX; }
            set
            {
                _minRoamX = value;
                Dirtied();
            }
        }

        public float MaxRoamingX
        {
            get { return _maxRoamX; }
            set
            {
                _maxRoamX = value;
                Dirtied();
            }
        }

        public float MinRoamingY
        {
            get { return _minRoamY; }
            set
            {
                _minRoamY = value;
                Dirtied();
            }
        }

        public float MaxRoamingY
        {
            get { return _maxRoamY; }
            set
            {
                _maxRoamY = value;
                Dirtied();
            }
        }

        public int RoamingDelay
        {
            get { return _roamDelay; }
            set
            {
                _roamDelay = value;
                Dirtied();
            }
        }

        public int ChanceTotal
        {
            get { return Entries.Sum(x => { return x.Chance; }); }
        }

        #endregion

        private MySqlConnection _connection;
        private ObservableCollection<SpawnEntry> _entries = new ObservableCollection<SpawnEntry>();
        public ObservableCollection<SpawnEntry> Entries
        {
            get { return _entries; }
        }

        public SpawnEntry CreateEntry()
        {
            var entry =  new SpawnEntry(_queryConfig);            
            return entry;
        }

        public void GetEntries()
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

                while (rdr.Read())
                {
                    var entry = new SpawnEntry(_queryConfig);                    

                    foreach (var item in _queries.SelectQueryFields)
                    {
                        if (fields.Contains(item.Column))
                        {
                            SetProperty(entry, item, rdr);
                        }
                    }

                    if (_entries.Where(x => x.NpcID == entry.NpcID).FirstOrDefault() == null)
                    {
                        AddEntry(entry);
                        entry.Created();
                    }
                }
                rdr.Close();

            }
            catch (Exception e)
            {
                //TODO log this
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

        public SpawnGroup(int id, MySqlConnection connection, Database.QueryConfig queryConfig)
            : base(queryConfig)
        {
            _id = id;
            _connection = connection;
        }

        public SpawnGroup(MySqlConnection connection, Database.QueryConfig queryConfig)
            : base(queryConfig)
        {
            _connection = connection;
        }

        public void AddEntry(SpawnEntry entry)
        {
            var count = _entries.Count(
                x =>
                {
                    return x.SpawnGroupID == entry.SpawnGroupID && x.NpcID == entry.NpcID;
                });
            if( count == 0 )
            {
                NeedsInserted.Add(entry);
                _entries.Add(entry);
                entry.ObjectDirtied += new Database.ObjectDirtiedHandler(entry_ObjectDirtied);
            }
            OnSpawnChanceTotalChanged();
        }

        private void entry_ObjectDirtied(object sender, EventArgs args)
        {
            SpawnEntry entry = sender as SpawnEntry;
            if (entry != null)
            {
                OnSpawnChanceTotalChanged();
            }
        }

        public void RemoveEntry(SpawnEntry entry)
        {
            if (NeedsInserted.Contains(entry))
            {
                NeedsInserted.Remove(entry);
            }
            else
            {
                NeedsDeleted.Add(entry);
            }

            Entries.Remove(entry);
            entry.ObjectDirtied -= entry_ObjectDirtied;
            OnSpawnChanceTotalChanged();
        }

        public void RemoveAllEntries()
        {
            foreach (var entry in Entries)
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
            Entries.Clear();
            OnSpawnChanceTotalChanged();
        }

        public override string UpdateString
        {
            get
            {
                //string sql = String.Format("SET @RoamAreaID = {0};", Id) + Environment.NewLine;
                string sql = "";
                if (Dirty)
                {
                    sql += base.UpdateString;
                }
                sql += GetQuery(Entries);
                return sql;
            }
        }

        public override string DeleteString
        {
            get
            {
                string sql = base.DeleteString;
                sql += GetQuery(Entries);
                return sql;
            }
        }

        public override string InsertString
        {
            get
            {
                string sql = base.InsertString;
                sql += GetQuery(Entries);
                return sql;
            }
        }

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { return Entries.Where(x => x.Dirty).ToList<Database.IDatabaseObject>(); }
        }

        public event SpawnChanceTotalChangedHandler SpawnChanceTotalChanged;

        private void OnSpawnChanceTotalChanged()
        {
            var e = SpawnChanceTotalChanged;
            if (e != null)
            {
                e(this, new EventArgs());
            }
        }
    }

    public delegate void SpawnChanceTotalChangedHandler(SpawnGroup sender,EventArgs e);
}
