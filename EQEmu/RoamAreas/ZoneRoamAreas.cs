using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.RoamAreas
{
    public abstract class ZoneRoamAreas : Database.ManageDatabase
    {
        private string _zone = "";
        private ObservableCollection<RoamArea> _roamAreas = new ObservableCollection<RoamArea>();

        public ZoneRoamAreas(string zone,QueryConfig config)
            : base(config)
        {
            _zone = zone;
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

        public void AddArea(RoamArea area)
        {
            if (RoamAreas.Contains(area)) return;

            if (RoamAreas.FirstOrDefault(x => x.Id == area.Id) != null || area.Zone != this.Zone)
            {
                throw new Exception("Roam Area ID/Zone conflict");
            }

            AddObject(area);
            RoamAreas.Add(area);
        }

        public RoamArea NewArea()
        {
            int newId = GetNextRoamAreaID();
            var area = new RoamArea(newId, _queryConfig);
            area.Zone = this.Zone;
            area.Created();
            return area;
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

        public override void SaveXML(string dir)
        {
            var ddir = System.IO.Path.GetDirectoryName(dir);

            using (var fs = new FileStream(ddir + "\\" + this.Zone + ".roamareas.xml", FileMode.Create))
            {
                var ary = _roamAreas.ToArray();
                var x = new XmlSerializer(ary.GetType());
                x.Serialize(fs, ary);
            }

            using (var fs = new FileStream(ddir + "\\" + this.Zone + ".roamentry.xml", FileMode.Create))
            {
                List<RoamAreaEntry> entries = new List<RoamAreaEntry>();
                foreach (var area in _roamAreas)
                {
                    entries.AddRange(area.Vertices);
                }
                var ary = entries.ToArray();
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

            _zone = filename.Substring(0, period1);

            //deserialize the areas first
            RoamArea[] areas;
            using (var fs = new FileStream(file, FileMode.Open))
            {
                var x = new XmlSerializer(_roamAreas.ToArray().GetType());
                var obj = x.Deserialize(fs);
                areas = obj as RoamArea[];                
            }

            //load each area into the zone collection
            if (areas != null)
            {
                ClearObjects();
                _roamAreas.Clear();
                Created();
                foreach (var area in areas)
                {
                    AddArea(area);
                    area.InitConfig(_queryConfig);
                    area.Created();
                }
            }
            
            //then load the zone area entries and map them accordingly
            RoamAreaEntry[] entries;
            var entryFile = dir + "\\" + _zone + ".roamentry.xml";
            if (RoamAreas.Count > 0 && System.IO.File.Exists(entryFile))
            {
                using (var fs = new FileStream(entryFile, FileMode.Open))
                {
                    var x = new XmlSerializer(RoamAreas.First().Vertices.ToArray().GetType());
                    var obj = x.Deserialize(fs);
                    entries = obj as RoamAreaEntry[];
                }
                //ALL entries are in array, now to determine which area they need to go into
                foreach (var entry in entries)
                {
                    var entryArea = RoamAreas.FirstOrDefault(x => x.Id == entry.RoamAreaId);
                    if (entryArea != null)
                    {
                        entryArea.AddEntry(entry);
                        entry.InitConfig(_queryConfig);
                        entry.Created();
                    }
                }
            }
        }

        public string Zone
        {
            get { return _zone; }
        }

        public ObservableCollection<RoamArea> RoamAreas
        {
            get
            {
                return _roamAreas;
            }
        }

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }
    }
}
