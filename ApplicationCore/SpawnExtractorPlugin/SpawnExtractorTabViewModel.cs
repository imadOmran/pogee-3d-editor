using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

using MySql.Data.MySqlClient;

using Microsoft.Practices.Unity;

using ApplicationCore;

using EQEmu.Spawns;
using SpawnsPlugin;

namespace SpawnExtractorPlugin
{
    public class SpawnExtractorTabViewModel : ApplicationCore.ViewModels.ViewModelBase, ApplicationCore.ViewModels.Editors.IEditorViewModel
    {
        private readonly MySql.Data.MySqlClient.MySqlConnection _connection;
        private readonly EQEmu.Database.QueryConfig _config;
        private readonly NPCPropertyTemplateManager _templates;

        private DelegateCommand _removeCommand;
        private DelegateCommand _applyTemplateCommand;

        private NPCAggregator _npcs;
        private SpawnGroupAggregator _spawngroups;
        private ZoneSpawns _spawns;

        public SpawnExtractorTabViewModel(MySqlConnection connection,EQEmu.Database.QueryConfig config,NPCPropertyTemplateManager templates)
        {
            _connection = connection;
            _config = config;
            _templates = templates;

            _npcs = new NPCAggregator(connection, config);
            _npcs.Created();
            _spawngroups = new SpawnGroupAggregator(connection, config);
            _spawngroups.Created();
            Zone = "";
            ZoneVersion = 0;
            _startId = 0;

            RemoveCommand = new DelegateCommand(
                x =>
                {
                    //remove all entries where this npc exists
                    foreach(var sg in _spawngroups.SpawnGroups.ToArray())
                    {
                        var entries = sg.Entries.Where(y => y.NpcID == SelectedNPC.Id).ToArray();
                        foreach (var entry in entries)
                        {
                            sg.RemoveEntry(entry);
                        }

                        //if no more entries remove the spawn2 that contains this spawngroup
                        if (sg.Entries.Count == 0)
                        {
                            _spawngroups.RemoveSpawnGroup(sg);

                            var spawns = _spawns.Spawns.Where(y => y.SpawnGroupId == sg.Id).ToArray();
                            foreach (var sp in spawns)
                            {
                                _spawns.RemoveSpawn(sp);
                            }

                        }
                    }
                    _npcs.RemoveNPC(SelectedNPC);
                },
                x =>
                {
                    return SelectedNPC != null;
                });

            ApplyTemplateCommand = new DelegateCommand(
                x =>
                {
                    var npcs = x as IEnumerable<NPC>;
                    if (npcs == null) return;

                    SelectedTemplate.SetProperties(npcs);
                },
                x =>
                {
                    return SelectedTemplate != null;
                });

        }
        
        public IEnumerable<INPCPropertyTemplate> Templates
        {
            get { return _templates.Templates; }
        }

        private int _startId;
        public int StartId 
        {
            get
            {
                return _startId;
            }
            set
            {
                _startId = value;
                NotifyPropertyChanged("StartId");
            }
        }

        private INPCPropertyTemplate _selectedTemplate;
        public INPCPropertyTemplate SelectedTemplate
        {
            get { return _selectedTemplate; }
            set
            {
                _selectedTemplate = value;
                NotifyPropertyChanged("SelectedTemplate");
            }
        }
        
        private NPC _selectedNPC;
        public NPC SelectedNPC 
        {
            get
            {
                return _selectedNPC;
            }
            set
            {
                _selectedNPC = value;

                var serv = _service as SpawnsPlugin.SpawnDataService;
                if (serv != null)
                {
                    var groups = _spawngroups.SpawnGroups.Where(x => x.Entries.Where(y => y.NPC == value).Count() > 0);
                    var selSpawns = _spawns.Spawns.Where(x => groups.Where(y => y.Id == x.SpawnGroupId).Count() > 0);

                    if (selSpawns != null && selSpawns.Count() > 0)
                    {
                        serv.SelectedSpawn = selSpawns.ElementAt(0);
                        serv.SelectedSpawns = selSpawns;
                    }
                }

                NotifyPropertyChanged("SelectedNPC");
                RemoveCommand.RaiseCanExecuteChanged();
            }
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

            if (LoadSpawnEntries)
            {
                if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
                {
                    var zps = new ZoneSpawnsDatabase(_connection, _config);
                    _spawns = zps;
                    zps.Lookup(Zone, ZoneVersion);
                }
                else
                {
                    _spawns = new ZoneSpawnsLocal(_config);
                    _spawns.Zone = Zone;
                    _spawns.Version = ZoneVersion;
                }
            }

            var zoneNpcs = spawns.Where(x => x.IsNPC > 0 && x.PetOwnerID == 0 && x.IsMercenary == 0 );

            int id = _npcs.GetNextIdForZone(Zone);
            if (id == -1) id = 0;

            foreach (var sp in zoneNpcs)
            {
                if ( _npcs.NPCs.Count(x => x.Level == sp.Level && x.Name == sp.SpawnName) == 0)
                {
                    var npc = _npcs.CreateNPC();
                    NPC.SetNPCProperties(ref npc, sp);
                    npc.Version = ZoneVersion;
                    npc.Id = id;
                    _npcs.AddNPC(npc);
                    id += 1;
                }
            }

            Dictionary<string, SpawnGroup> spawngroupMap = new Dictionary<string, SpawnGroup>();
            if (LoadSpawnGroups)
            {
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
            }


            if (LoadSpawnEntries)
            {
                foreach (var npc in zoneNpcs)
                {
                    Spawn2 spawn = _spawns.GetNewSpawn();
                    spawn.Created();

                    if (LoadSpawnGroups)
                    {
                        if (spawngroupMap.ContainsKey(npc.SpawnName))
                        {
                            spawn.SpawnGroupRef = spawngroupMap[npc.SpawnName];
                        }
                    }

                    spawn.X = npc.XPos;
                    spawn.Y = npc.YPos;
                    spawn.Z = npc.ZPos;
                    spawn.Heading = npc.Heading;
                    _spawns.AddSpawn(spawn);
                }
            }

            var serv = _service as SpawnsPlugin.SpawnDataService;
            if(serv != null)
            {
                serv.ZoneSpawns = _spawns;
            }
        }

        public DelegateCommand RemoveCommand
        {
            get { return _removeCommand; }
            set
            {
                _removeCommand = value;
                NotifyPropertyChanged("RemoveCommand");
            }
        }

        public DelegateCommand ApplyTemplateCommand
        {
            get { return _applyTemplateCommand; }
            set
            {
                _applyTemplateCommand = value;
                NotifyPropertyChanged("ApplyTemplateCommand");
            }
        }

        public bool LoadSpawnGroups
        {
            get;
            set;
        }

        public bool LoadSpawnEntries
        {
            get;
            set;
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
            string str = _npcs.GetSQL();
            if (LoadSpawnGroups) str += _spawngroups.GetSQL();
            if (LoadSpawnEntries) str += _spawns.GetSQL();
            return str;
        }

        public IEnumerable<NPC> NPCs
        {
            get { return _npcs.NPCs; }
        }

        private ApplicationCore.DataServices.IDataService _service;
        [Dependency("SpawnDataService")]
        public ApplicationCore.DataServices.IDataService Service
        {
            get { return _service; }
            set
            {
                _service = value;
                NotifyPropertyChanged("Service");
            }
        }
    }
}
