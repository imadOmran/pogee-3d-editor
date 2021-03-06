﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

using MySql.Data.MySqlClient;

namespace EQEmu.Spawns
{
    public abstract class SpawnGroupAggregator : Database.ManageDatabase
    {        
        private IEnumerable<Spawn2> _updateSpawn2 = null;
        protected ObservableCollection<SpawnGroup> _spawnGroups = new ObservableCollection<SpawnGroup>();
        private int _filterById;
        private static string _defaultFilePrefix = "global";

        public SpawnGroupAggregator(Database.QueryConfig queryConfig)
            : base(queryConfig)
        {
        }

        public static string DefaultFilePrefix
        {
            get { return _defaultFilePrefix; }
        }

        public int FilterById
        {
            get { return _filterById; }
            set
            {
                _filterById = value;
                Lookup(value);
            }
        }

        public IEnumerable<SpawnGroup> SpawnGroups
        {
            get { return _spawnGroups; }
        }

        public void RemoveSpawnGroup(SpawnGroup group)
        {
            if (!_spawnGroups.Contains(group)) return;

            RemoveObject(group);
            group.RemoveAllEntries();
            _spawnGroups.Remove(group);
        }

        public void AddSpawnGroup(SpawnGroup group)
        {
            if (_spawnGroups.Contains(group)) return;
            AddObject(group);
            _spawnGroups.Add(group);
        }

        public void ClearCache()
        {
            ClearObjects();
            _spawnGroups.Clear();
            _updateSpawn2 = null;
        }

        public abstract SpawnGroup CreateSpawnGroup();
        public abstract IEnumerable<SpawnGroup> LookupByZone(string zone);
        
        protected virtual SpawnGroup Lookup(int id)
        {
            SpawnGroup sg = _spawnGroups.Where(x => { return x.Id == id; }).FirstOrDefault();
            if (sg != null) return sg;
            return sg;
        }

        public virtual void PackCachedId(int start, int end,BackgroundWorker worker=null)
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

            IEnumerable<SpawnGroup> sorted = SpawnGroups.OrderBy(x => x.Id);
            int i = start;
            var updates = new List<Spawn2>();

            foreach (var spawn in sorted)
            {
                //if we are going to potentially re-insert them all somewhere we might as well delete them
                //the update query generates the delete queries first so this works
                //create a spawn that keeps track of the identifier so we can delete it
                var copy = CreateSpawnGroup();
                copy.Id = spawn.Id;
                NeedsDeleted.Add(copy);

                //spawngroups are associated with 0-n spawn2 entries so these will need updated
                
                foreach (var s2 in copy.GetLinkedSpawn2())
                {
                    //force it to be dirtied
                    s2.Created();
                    s2.SpawnGroupId = i;
                    updates.Add(s2);
                }                

                spawn.UnlockObject();
                spawn.Id = i;
                spawn.Created();

                i += 1;

                if (worker != null)
                {
                    double x = i - start;
                    double y = SpawnGroups.Count();

                    double percent = x / y * 100;
                    worker.ReportProgress((int)percent);
                }
            }

            _updateSpawn2 = updates;
        }

        public string GetSQL()
        {
            List<Database.IDatabaseObject> updates = new List<Database.IDatabaseObject>();

            if (_updateSpawn2 != null)
            {
                updates.AddRange(_updateSpawn2);
            }
            updates.AddRange(SpawnGroups);

            return GetQuery(updates);
        }

        private string _zone = "";

        public void SaveXML(string zone, string dir)
        {
            _zone = zone;
            if (zone == "" || zone == null) _zone = _defaultFilePrefix;            
            SaveXML(dir);
        }

        public override void SaveXML(string dir)
        {
            using (var fs = new FileStream(dir + "\\" + _zone + ".spawngroups.xml", FileMode.Create))
            {
                var ary = _spawnGroups.ToArray();
                var x = new XmlSerializer(ary.GetType());
                x.Serialize(fs, ary);
            }

            using (var fs = new FileStream(dir + "\\" + _zone + ".spawnentries.xml", FileMode.Create))
            {
                List<SpawnEntry> allEntries = new List<SpawnEntry>();
                foreach (var g in _spawnGroups)
                {
                    allEntries.AddRange(g.Entries);
                }

                var ary = allEntries.ToArray();
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

            string zone = filename.Substring(0, period1);

            SpawnGroup[] spawngroups;            
            using (var fs = new FileStream(file, FileMode.Open))
            {
                var x = new XmlSerializer( _spawnGroups.ToArray().GetType());
                var obj = x.Deserialize(fs);
                spawngroups = obj as SpawnGroup[];                
            }

            if (spawngroups != null)
            {
                ClearCache();
                Created();
                foreach (var sg in spawngroups)
                {                    
                    sg.InitConfig(_queryConfig);
                    sg.Created();
                    AddSpawnGroup(sg);
                }
            }

            SpawnEntry[] entries;
            file = dir + "\\" + zone + ".spawnentries.xml";

            if (_spawnGroups.Count > 0 && System.IO.File.Exists(file))
            {
                using(var fs = new FileStream(file, FileMode.Open) )
                {
                    var x = new XmlSerializer( _spawnGroups.ElementAt(0).Entries.ToArray().GetType() );
                    var obj = x.Deserialize(fs);
                    entries = obj as SpawnEntry[];
                }

                foreach (var entry in entries)
                {
                    var sg = SpawnGroups.FirstOrDefault(x => x.Id == entry.SpawnGroupID);
                    entry.InitConfig(_queryConfig);
                    entry.Created();
                    if (sg != null)
                    {
                        sg.AddEntry(entry);
                    }
                }
            }
            OnSpawnGroupDataLoaded(zone,spawngroups);
        }
        
        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }

        public event SpawnGroupDataLoadedHandler DataLoaded;
        private void OnSpawnGroupDataLoaded(string zone, IEnumerable<SpawnGroup> items)
        {
            var e = DataLoaded;
            if (e != null)
            {
                e(this, new SpawnGroupDataLoadedEventArgs(zone,items));
            }
        }
    }

    public delegate void SpawnGroupDataLoadedHandler(object sender, SpawnGroupDataLoadedEventArgs e);
    public class SpawnGroupDataLoadedEventArgs : EventArgs
    {
        public SpawnGroupDataLoadedEventArgs(string zonename,IEnumerable<SpawnGroup> items)
        {
            ZoneName = zonename;
            ItemsLoaded = items;
        }

        public IEnumerable<SpawnGroup> ItemsLoaded { get; private set; }
        public string ZoneName { get; private set; }
    }   
}
