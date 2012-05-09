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
