using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.RoamAreas
{
    public class ZoneRoamAreas : Database.ManageDatabase
    {
        private readonly MySqlConnection _connection = null;
        private readonly string _zone = "";

        private ObservableCollection<RoamArea> _roamAreas = new ObservableCollection<RoamArea>();
        public ObservableCollection<RoamArea> RoamAreas
        {
            get
            {
                return _roamAreas;
            }
        }

        public string Zone
        {
            get { return _zone; }
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

        public int GetNextRoamAreaID()
        {
            int number = 1;

            if (RoamAreas.Count > 0)
            {
                number = RoamAreas.Max(x => x.Id) + 1;
            }

            return number;
        }

        public RoamArea NewArea()
        {
            int newId = GetNextRoamAreaID();
            var area = new RoamArea(newId, _queryConfig);
            area.Zone = this.Zone;
            area.Created();            
            return area;
        }

        public void AddArea(RoamArea area)
        {
            if (RoamAreas.FirstOrDefault(x => x.Id == area.Id) != null || area.Zone != this.Zone)
            {
                throw new Exception("Roam Area ID/Zone conflict");
            }

            RoamAreas.Add(area);

            // object was finalized, any changes now are recorded
            if (CreatedObj)
            {
                NeedsInserted.Add(area);
            }
        }

        public ZoneRoamAreas(MySqlConnection connection, string zone, QueryConfig config)
            : base(config)
        {
            _connection = connection;
            _zone = zone;

            if (_connection == null)
            {
                throw new NullReferenceException();
            }

            var sql = String.Format(SelectString, SelectArgValues);
            var results = Database.QueryHelper.RunQuery(_connection, sql);

            foreach (var row in results)
            {
                var ra = new RoamArea(_queryConfig);
                ra.SetProperties(Queries, row);
                RoamAreas.Add(ra);
            }

            foreach (var area in RoamAreas)
            {
                sql = String.Format(area.SelectString, area.SelectArgValues);
                results = Database.QueryHelper.RunQuery(_connection, sql);
                foreach (var row in results)
                {
                    var entry = new RoamAreaEntry(_queryConfig);
                    entry.SetProperties(area.Queries, row);
                    area.Vertices.Add(entry);
                    entry.Created();
                }

                area.Created();
            }

            this.Created();
        }

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }
    }
}
