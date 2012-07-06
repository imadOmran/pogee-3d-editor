using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

using MySql.Data.MySqlClient;

using Microsoft.Practices.Unity;
using Microsoft.Win32;

using ApplicationCore;

using EQEmu.Spawns;
using SpawnsPlugin;
using SpawnGroupPlugin;

namespace SpawnExtractorPlugin
{
    public delegate void FileLoadingHandler(object sender, FileLoadingEventArgs e);

    public class FileLoadingEventArgs
    {
        public enum LoadingState
        {
            PreLoad,
            Loaded,
            Error
        }

        public FileLoadingEventArgs(string file, LoadingState state)
        {
            FileName = file;
            State = state;
        }

        public string FileName
        {
            get;
            private set;
        }

        public LoadingState State
        {
            get;
            private set;
        }

        public string Error
        {
            get;
            set;
        }            
    }

    public class SpawnExtractorTabViewModel : ApplicationCore.ViewModels.ViewModelBase, ApplicationCore.ViewModels.Editors.IEditorViewModel
    {
        private readonly MySql.Data.MySqlClient.MySqlConnection _connection;
        private readonly EQEmu.Database.QueryConfig _config;
        private readonly NpcPropertyTemplateManager _templates;

        private DelegateCommand _removeCommand;
        private DelegateCommand _applyTemplateCommand;
        private DelegateCommand _openFileCommand;
        private DelegateCommand _loadFileCommand;

        private NpcAggregator _npcs;
        private SpawnGroupAggregator _spawngroups;
        private ZoneSpawns _spawns;

        private string _fileSelected;

        public event FileLoadingHandler FileSelectionChanged;

        public SpawnExtractorTabViewModel(MySqlConnection connection,EQEmu.Database.QueryConfig config,NpcPropertyTemplateManager templates)
        {
            _connection = connection;
            _config = config;
            _templates = templates;
            _startId = 0;
            Zone = "";
            ZoneVersion = 0;            

            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                _npcs = new NpcAggregatorDatabase(connection, config);
            }
            else
            {
                _npcs = new NpcAggregatorLocal(config);
            }            
            _npcs.Created();

            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                _spawngroups = new SpawnGroupAggregatorDatabase(connection, config);
            }
            else
            {
                _spawngroups = new SpawnGroupAggregatorLocal(config);
            }
            _spawngroups.Created();
        }
        
        public IEnumerable<INpcPropertyTemplate> Templates
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

        private INpcPropertyTemplate _selectedTemplate;
        public INpcPropertyTemplate SelectedTemplate
        {
            get { return _selectedTemplate; }
            set
            {
                _selectedTemplate = value;
                ApplyTemplateCommand.RaiseCanExecuteChanged();
                NotifyPropertyChanged("SelectedTemplate");
            }
        }
        
        private Npc _selectedNPC;
        public Npc SelectedNPC 
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
                    var groups = _spawngroups.SpawnGroups.Where(x => x.Entries.Where(y => y.NPC == value || y.NpcID == value.Id ).Count() > 0);
                    var selSpawns = _spawns.Spawns.Where(x => groups.Where(y => y.Id == x.SpawnGroupId).Count() > 0);

                    if (selSpawns != null && selSpawns.Count() > 0)
                    {
                        serv.SelectedSpawn = selSpawns.ElementAt(0);
                        serv.SelectedSpawns = selSpawns;
                    }
                }

                if (_selectedNpcs == null) _selectedNpcs = new List<Npc>() { value };

                NotifyPropertyChanged("SelectedNPC");
                RemoveCommand.RaiseCanExecuteChanged();
                ApplyTemplateCommand.RaiseCanExecuteChanged();
            }
        }

        private IEnumerable<Npc> _selectedNpcs;
        public IEnumerable<Npc> SelectedNpcs
        {
            get { return _selectedNpcs; }
            set
            {
                _selectedNpcs = value;
                if (_selectedNpcs.Count() > 0)
                {
                    SelectedNPC = _selectedNpcs.First();
                }
                else { SelectedNPC = null; }

                NotifyPropertyChanged("SelectedNpcs");
            }
        }

        public void User3DClickAt(object sender, ApplicationCore.ViewModels.Editors.World3DClickEventArgs e)
        {
        }

        public void SaveXML(string dir)
        {
            if (_spawns != null)
            {
                _spawns.SaveXML(dir);
            }

            if (_spawngroups != null)
            {
                _spawngroups.SaveXML(Zone,dir);
            }

            if (_npcs != null)
            {
                _npcs.SaveXML(Zone, dir);
            }
        }

        public void OpenXML(string file,string zone,int version)
        {            
            XmlSerializer serializer = new XmlSerializer(typeof(List<ZoneEntryStruct>));

            Zone = zone;
            ZoneVersion = version;

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
                    Npc.SetNPCProperties(ref npc, sp);
                    npc.Version = ZoneVersion;
                    npc.Id = id;
                    _npcs.AddNPC(npc);
                    id += 1;
                }
            }

            Dictionary<string, SpawnGroup> spawngroupMap = new Dictionary<string, SpawnGroup>();
            if (LoadSpawnGroups)
            {
                //all added spawngroups need to be flagged for insertion
                _spawngroups.Created();

                foreach (var npc in _npcs.NPCs.GroupBy(x => x.Name))
                {
                    var sg = _spawngroups.CreateSpawnGroup();
                    foreach (var name in npc)
                    {
                        sg.AddEntry(name);
                    }
                    sg.Created();
                    _spawngroups.AddSpawnGroup(sg);
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

                var serv = _service as SpawnsPlugin.SpawnDataService;
                if (serv != null)
                {
                    serv.ZoneSpawns = _spawns;
                }
            }

            OnFileLoaded(FileSelected, FileLoadingEventArgs.LoadingState.Loaded);
        }

        public DelegateCommand RemoveCommand
        {
            get 
            {
                if (_removeCommand == null)
                {
                    _removeCommand = new DelegateCommand(
                        x =>
                        {
                            //remove all entries where this npc exists
                            foreach (var sg in _spawngroups.SpawnGroups.ToArray())
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
                }
                return _removeCommand; 
            }
            set
            {
                _removeCommand = value;
                NotifyPropertyChanged("RemoveCommand");
            }
        }

        public DelegateCommand ApplyTemplateCommand
        {
            get 
            {
                if (_applyTemplateCommand == null)
                {
                    _applyTemplateCommand = new DelegateCommand(
                    x =>
                    {
                        if (_selectedNpcs == null) return;
                        SelectedTemplate.SetProperties(_selectedNpcs);
                        NotifyPropertyChanged("Items");
                    },
                    x =>
                    {
                        return SelectedTemplate != null && SelectedNPC != null;
                    });
                }
                return _applyTemplateCommand; 
            }
        }

        public DelegateCommand OpenFileCommand
        {
            get
            {
                if (_openFileCommand == null)
                {
                    _openFileCommand = new DelegateCommand(
                        x =>
                        {
                            OpenFileDialog fd = new OpenFileDialog();
                            if (fd.ShowDialog() == true)
                            {
                                FileSelected = fd.FileName;                                
                            }
                        },
                        y =>
                        {
                            return true;
                        });
                }
                return _openFileCommand;
            }
        }

        public DelegateCommand LoadFileCommand
        {
            get
            {
                if (_loadFileCommand == null)
                {
                    _loadFileCommand = new DelegateCommand(
                        x =>
                        {
                            try
                            {
                                OpenXML(FileSelected, Zone, ZoneVersion);
                            }
                            catch (System.IO.FileFormatException ex)
                            {
                                OnFileLoaded(FileSelected, FileLoadingEventArgs.LoadingState.Error, "Data format:" + ex.Message);
                            }
                            catch (Exception ex)
                            {
                                OnFileLoaded(FileSelected, FileLoadingEventArgs.LoadingState.Error, ex.Message);
                            }
                        },
                        y =>
                        {
                            return _fileSelected != null;
                        });
                }
                return _loadFileCommand;
            }
        }

        public string FileSelected
        {
            get { return _fileSelected; }
            set
            {
                _fileSelected = value;
                NotifyPropertyChanged("FileSelected");
                OnFileLoaded(value, FileLoadingEventArgs.LoadingState.PreLoad);
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

        public IEnumerable<Npc> NPCs
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

        private SpawnGroupEditTabViewModel _spawnGroupViewModel;
        [Dependency]
        public SpawnGroupEditTabViewModel SpawnGroupViewModel
        {
            get { return _spawnGroupViewModel; }
            set
            {
                _spawnGroupViewModel = value;

                if (value != null && _spawnGroupViewModel.Aggregator != null)
                {
                    _spawngroups = _spawnGroupViewModel.Aggregator;
                }

                NotifyPropertyChanged("SpawnGroupViewModel");
            }
        }

        private void OnFileLoaded(string file, FileLoadingEventArgs.LoadingState state,string message=null)
        {
            var e = FileSelectionChanged;
            if (e != null)
            {
                e(this, new FileLoadingEventArgs(file, state) { Error = message });
                LoadFileCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
