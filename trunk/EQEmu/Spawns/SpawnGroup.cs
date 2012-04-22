﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

using MySql.Data.MySqlClient;

namespace EQEmu.Spawns
{
    public class SpawnEntry : Database.DatabaseObject
    {
        private int _spawnGroupId;
        private int _npcId;
        private short _chance;
        private NPC _npc;

        public int SpawnGroupID
        {
            get { return _spawnGroupId; }
            set
            {
                _spawnGroupId = value;
                Dirtied();
            }
        }

        [Browsable(false)]
        public NPC NPC
        {
            get { return _npc; }
            set
            {
                _npc = value;
                NpcID = value.Id;
            }
        }

        public int NpcID
        {
            get 
            {
                if (_npc == null)
                {
                    return _npcId;
                }
                else
                {
                    return _npc.Id;
                }
            }
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

                //the entries will need updated as well if the Id changes
                //if the ID is changing - which is the key field:
                //a new one might as well be created and remove the old one

                foreach (var entry in Entries.ToArray())
                {
                    var newEntry = CreateEntry();
                    newEntry.SpawnGroupID = value;
                    newEntry.NpcID = entry.NpcID;
                    newEntry.NpcLevel = entry.NpcLevel;
                    newEntry.NpcName = entry.NpcName;

                    AddEntry(newEntry);
                    RemoveEntry(entry);
                }
                Dirtied();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                Dirtied();
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
            entry.SpawnGroupID = Id;
            return entry;
        }

        public IEnumerable<Spawn2> GetLinkedSpawn2()
        {
            List<Spawn2> spawns = new List<Spawn2>();

            var query = Queries.ExtensionQueries.FirstOrDefault(x => x.Name == "GetSpawn2");
            if (query != null)
            {
                string sql = String.Format(query.SelectQuery, new string[] { Id.ToString() });
                var results = Database.QueryHelper.RunQuery(_connection, sql);
                foreach (var row in results)
                {
                    var spawn = new Spawn2(_queryConfig);
                    spawn.SetProperties(query, row);
                    spawns.Add(spawn);
                }                
            }
            return spawns;
        }

        public void GetEntries()
        {
            var sql = String.Format(SelectString,SelectArgValues);
            var results = Database.QueryHelper.RunQuery(_connection, sql);

            foreach (var row in results)
            {
                var entry = new SpawnEntry(_queryConfig);
                entry.SetProperties(Queries, row);

                if (_entries.Where(x => x.NpcID == entry.NpcID).FirstOrDefault() == null)
                {
                    _entries.Add(entry);
                    entry.ObjectDirtied += new Database.ObjectDirtiedHandler(entry_ObjectDirtied);
                    entry.Created();
                    OnSpawnChanceTotalChanged();
                }
            }     
        }

        public SpawnGroup() : base (null)
        {

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

        public SpawnEntry AddEntry(NPC npc)
        {
            var entry = CreateEntry();
            entry.NpcID = npc.Id;
            entry.Created();
            AddEntry(entry);
            return entry;
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
                try
                {
                    _entries.Add(entry);
                }
                catch (NotSupportedException)
                {
                    //GUI updates will fail if on a different thread...
                    //the item will still be added this exception is preventing the GUI from updating /
                    //in the case of a wpf control being bound to this collection
                };
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

        public void BalanceChance()
        {
            var chance = 100;
            while (chance > 0)
            {
                foreach (var entry in Entries)
                {
                    entry.Chance += 1;
                    chance -= 1;
                    if (chance == 0) break;
                }
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

            try
            {
                Entries.Remove(entry);
            }
            catch (NotSupportedException) { 
                //GUI updates will fail if on a different thread...
                //a dependency here on the windows dispatcher is not wanted yet...
            };

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