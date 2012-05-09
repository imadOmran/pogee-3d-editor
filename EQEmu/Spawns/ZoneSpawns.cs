using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public abstract class ZoneSpawns : Database.ManageDatabase
    {
        protected string _zone;
        protected int _version = 0;
        private ObservableCollection<Spawn2> _spawns = new ObservableCollection<Spawn2>();

        public ZoneSpawns(QueryConfig config) : base(config)
        {
        }

        public abstract Spawn2 GetNewSpawn();

        public Spawn2 GetNearestSpawn(Point3D p, double threshhold = 2.0)
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

        public string GetSQL()
        {
            return GetQuery(Spawns);
        }

        [XmlIgnore]
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

                //we aren't really requested a new spawn ... just a dummy reference for the id                
                Spawn2 copy = new Spawn2(_queryConfig);
                copy.Id = spawn.Id;
                copy.Zone = Zone;
                copy.Version = Version;
                NeedsDeleted.Add(copy);

                spawn.Id = i;
                i += 1;
                NeedsInserted.Add(spawn);
            }
        }

        public void AddSpawn(Spawn2 spawn)
        {
            AddObject(spawn);
        }

        public void RemoveSpawn(Spawn2 spawn)
        {
            RemoveObject(spawn);
        }

        protected override void AddObject(IDatabaseObject obj)
        {
            var spawn = obj as Spawn2;
            if (!Spawns.Contains(spawn))
            {
                base.AddObject(spawn);
                Spawns.Add(spawn);
            }
        }

        protected override void RemoveObject(IDatabaseObject obj)
        {
            var spawn = obj as Spawn2;
            if (Spawns.Contains(spawn))
            {
                base.RemoveObject(spawn);
                Spawns.Remove(spawn);
            }
        }

        protected override void ClearObjects()
        {
            base.ClearObjects();
            Spawns.Clear();
        }

        /// <summary>
        /// Provide a directory path to save to
        /// </summary>
        /// <param name="file"></param>
        public override void SaveXML(string dir)
        {
            using (var fs = new FileStream(dir + "\\" + this.Zone + "." + this.Version + ".spawn2.xml", FileMode.Create))
            {
                var ary = _spawns.ToArray();
                var x = new XmlSerializer(ary.GetType());
                x.Serialize(fs, ary);
            }
        }

        public override void LoadXML(string file)
        {
            //base.LoadXML(file);
            var filename = System.IO.Path.GetFileName(file);
            int period1 = filename.IndexOf('.', 0);
            int period2 = filename.IndexOf('.', period1 + 1);

            Zone = filename.Substring(0, period1);
            Version = int.Parse(filename.Substring(period1 + 1, period2 - period1 - 1));

            Spawn2[] spawns;
            using (var fs = new FileStream(file, FileMode.Open))
            {
                var x = new XmlSerializer(_spawns.ToArray().GetType());
                var obj = x.Deserialize(fs);
                spawns = obj as Spawn2[];
            }


            if (spawns != null)
            {
                ClearObjects();
                Created();
                foreach (var sp in spawns)
                {
                    AddSpawn(sp);
                    sp.Created();
                    sp.InitConfig(_queryConfig);
                }                
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

        [XmlIgnore]
        public ObservableCollection<Spawn2> Spawns
        {
            get
            {
                return _spawns;
            }
        }

        public virtual string Zone
        {
            get { return _zone; }
            set
            {
                _zone = value;
            }
        }

        public virtual int Version
        {
            get { return _version; }
            set
            {
                _version = value;
            }
        }
    }
}
