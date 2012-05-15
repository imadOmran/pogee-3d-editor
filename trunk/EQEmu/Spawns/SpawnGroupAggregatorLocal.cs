using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public class SpawnGroupAggregatorLocal : SpawnGroupAggregator
    {
        private Dictionary<string, List<SpawnGroup>> _zoneGroups = new Dictionary<string, List<SpawnGroup>>();

        public SpawnGroupAggregatorLocal(QueryConfig config)
            : base(config)
        {
            Created();
            DataLoaded += new SpawnGroupDataLoadedHandler(SpawnGroupAggregatorLocal_DataLoaded);
        }

        void SpawnGroupAggregatorLocal_DataLoaded(object sender, SpawnGroupDataLoadedEventArgs e)
        {
            var items = new List<SpawnGroup>();
            items.AddRange(e.ItemsLoaded);
            _zoneGroups[e.ZoneName] = items;
        }

        public override SpawnGroup CreateSpawnGroup()
        {
            int id = 1;
            var sg = new SpawnGroupLocal(_queryConfig);

            if (SpawnGroups.Count() > 0)
            {
                id = SpawnGroups.Max(x => x.Id) + 1;
            }

            sg.Id = id;
            sg.Created();
            //AddSpawnGroup(sg);

            return sg;
        }

        /// <summary>
        /// Note that this will only look up spawn group's by data that was loaded in initially as spawngroup's are not innately associated with a zone
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public override IEnumerable<SpawnGroup> LookupByZone(string zone)
        {
            if (_zoneGroups.ContainsKey(zone))
            {
                return _zoneGroups[zone];
            }
            else return new List<SpawnGroup>();
        }
    }
}
