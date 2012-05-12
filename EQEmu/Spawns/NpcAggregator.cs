using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public abstract class NpcAggregator : Database.ManageDatabase
    {
        private ObservableCollection<Npc> _npcs = new ObservableCollection<Npc>();
        private List<Npc> _cache = new List<Npc>();

        private static string _defaultFilePrefix = "global";

        public NpcAggregator(Database.QueryConfig config)
            : base(config)
        {
        }

        public ObservableCollection<Npc> NPCs
        {
            get { return _npcs; }
        }

        public IEnumerable<Npc> CachedNpcs
        {
            get { return _cache; }
        }

        //this is kind of a pointless property... only used for reflection property setting in query
        public string FilterName
        {
            get;
            set;
        }

        public void ClearCache()
        {
            NPCs.Clear();
            _cache.Clear();
            ClearObjects();
        }

        public void AddNPC(Npc npc)
        {
            AddObject(npc);
            _npcs.Add(npc);

            if (_cache.FirstOrDefault(x => x.Id == npc.Id) != null) return;
            _cache.Add(npc);
        }

        public void RemoveNPC(Npc npc)
        {
            RemoveObject(npc);
            _npcs.Remove(npc);
            _cache.Remove(npc);
        }

        public Npc CreateNPC()
        {
            return new Npc(_queryConfig);
        }

        public string GetSQL()
        {
            return GetQuery(_cache);
        }

        public abstract void Lookup(string name);
        public abstract int GetNextIdForZone(string zone);
        public abstract void LookupByZone(string zone);

        public override void SaveXML(string dir)
        {
            using (var fs = new FileStream(dir + "\\" + _zone + ".npctypes.xml", FileMode.Create))
            {
                var ary = _cache.ToArray();
                var x = new XmlSerializer(ary.GetType());
                x.Serialize(fs, ary);
            }            
        }

        private string _zone = _defaultFilePrefix;
        public void SaveXML(string zone, string dir)
        {
            if (zone == "" || zone == null) _zone = _defaultFilePrefix;
            else _zone = zone;
            SaveXML(dir);
        }

        public override void LoadXML(string file)
        {
            var filename = System.IO.Path.GetFileName(file);
            int period1 = filename.IndexOf('.', 0);
            int period2 = filename.IndexOf('.', period1 + 1);

            string zone = filename.Substring(0, period1);

            Npc[] npcs;
            using (var fs = new FileStream(file, FileMode.Open))
            {
                var x = new XmlSerializer(_cache.ToArray().GetType());
                var obj = x.Deserialize(fs);
                npcs = obj as Npc[];
            }


            if (npcs != null)
            {
                ClearObjects();
                Created();
                foreach (var npc in npcs)
                {
                    //AddNPC(npc);
                    _cache.Add(npc);
                    npc.Created();
                    npc.InitConfig(_queryConfig);
                }
            }           
        }

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }
    }
}
