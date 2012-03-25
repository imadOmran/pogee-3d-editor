using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

using MySql.Data.MySqlClient;

using EQEmu.Spawns;

namespace SpawnExtractorPlugin
{
    public class SpawnExtractorTabViewModel : ApplicationCore.ViewModels.ViewModelBase, ApplicationCore.ViewModels.Editors.IEditorViewModel
    {
        private readonly MySql.Data.MySqlClient.MySqlConnection _connection;
        private readonly EQEmu.Database.QueryConfig _config;

        private NPCAggregator _npcs;
        private SpawnGroupAggregator _spawngroups;
        private ZoneSpawns _spawns;

        public SpawnExtractorTabViewModel(MySqlConnection connection,EQEmu.Database.QueryConfig config)
        {
            _connection = connection;
            _config = config;

            _npcs = new NPCAggregator(connection, config);
            _npcs.Created();
            _spawngroups = new SpawnGroupAggregator(connection, config);
            _spawngroups.Created();
            Zone = "";
            ZoneVersion = 0;
        }

        public void User3DClickAt(object sender, ApplicationCore.ViewModels.Editors.World3DClickEventArgs e)
        {
        }

        public void OpenXML(string file)
        {            
            XmlSerializer serializer = new XmlSerializer(typeof(List<ZoneEntryStruct>));

            List<ZoneEntryStruct> spawns;

            try
            {
                using (var fs = new System.IO.FileStream(file, FileMode.Open))
                {
                    var obj = serializer.Deserialize(fs);
                    spawns = obj as List<ZoneEntryStruct>;
                }
            }
            catch (Exception)
            {
                throw;
            }

            if (spawns == null) throw new FileFormatException();

            _npcs.ClearCache();
            _spawngroups.ClearCache();
            _spawns = new ZoneSpawns(_connection,Zone, _config,ZoneVersion);

            var zoneNpcs = spawns.Where(x => x.IsNPC > 0 && x.PetOwnerID == 0 && x.IsMercenary == 0 && x.Race != 127);

            foreach (var sp in zoneNpcs)
            {
                if ( _npcs.NPCs.Count(x => x.Level == sp.Level && x.Name == sp.SpawnName) == 0)
                {
                    var npc = _npcs.CreateNPC();
                    NPC.SetNPCProperties(ref npc, sp);
                    npc.Version = ZoneVersion;
                    _npcs.AddNPC(npc);
                }
            }

            Dictionary<string, SpawnGroup> spawngroupMap = new Dictionary<string, SpawnGroup>();

            foreach (var npc in _npcs.NPCs.GroupBy(x => x.Name))
            {                
                var sg = _spawngroups.CreateSpawnGroup();
                foreach (var name in npc)
                {
                    sg.AddEntry(name).NPC = name;
                }                
                sg.Created();
                sg.BalanceChance();
                sg.Name = String.Format("{0}_{1}_v{2}", Zone, npc.ElementAt(0).Name, ZoneVersion);                
                spawngroupMap[npc.ElementAt(0).Name] = sg;
            }

            foreach (var npc in zoneNpcs)
            {
                var spawn = _spawns.GetNewSpawn();
                spawn.Created();                      
                if( spawngroupMap.ContainsKey(npc.SpawnName) )
                {
                    spawn.SpawnGroupRef = spawngroupMap[npc.SpawnName];
                }
                spawn.X = npc.XPos;
                spawn.Y = npc.YPos;
                spawn.Z = npc.ZPos;
                spawn.Heading = npc.Heading;                
                _spawns.AddSpawn(spawn);
            }
        }

        private string _zone;
        public string Zone
        {
            get { return _zone; }
            set
            {
                _zone = value;
                NotifyPropertyChanged("Zone");
            }
        }

        private int _zoneVersion;
        public int ZoneVersion
        {
            get { return _zoneVersion; }
            set
            {
                _zoneVersion = value;
                NotifyPropertyChanged("ZoneVersion");
            }
        }

        public string NPCQuery()
        {
            return _npcs.GetSQL() + _spawngroups.GetSQL() + _spawns.GetSQL();
        }

        public IEnumerable<NPC> NPCs
        {
            get { return _npcs.NPCs; }
        }

        public ApplicationCore.DataServices.IDataService Service
        {
            get { throw new NotImplementedException(); }
        }
    }
}
