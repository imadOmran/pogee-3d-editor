using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

using MySql.Data.MySqlClient;

using ApplicationCore;

using EQEmu.Spawns;
using EQEmu.Database;

namespace SpawnGroupPlugin
{
    public class SpawnGroupEditTabViewModel : ApplicationCore.ViewModels.ViewModelBase
    {
        private readonly SpawnGroupAggregator _manager;
        private readonly NpcAggregator _npcFinder;
        private Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        private string _npcFilterString;
        private string _zoneFilter;
        private int _packStart = 0;
        private int _packEnd = 0;
        private int _packProgress = 0;

        private SpawnEntry _selectedEntry = null;
        private SpawnGroup _selectedSpawnGroup = null;
        private BackgroundWorker _packWork;

        private DelegateCommand _packCommand;
        private DelegateCommand _clearCacheCommand;
        private DelegateCommand _adjustChanceTotalCommand;
        private DelegateCommand _prevIdCommand;
        private DelegateCommand _nextIdCommand;
        private DelegateCommand _createNewCommand;
        private DelegateCommand _removeSpawnGroupCommand;
        private DelegateCommand _removeSelectedCommand;
        private DelegateCommand _viewQueryCommand;

        public SpawnGroupEditTabViewModel(MySqlConnection connection,QueryConfig config)
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                _manager = new SpawnGroupAggregatorDatabase(connection, config);
                _npcFinder = new NpcAggregatorDatabase(connection, config);
            }
            else
            {
                _manager = new SpawnGroupAggregatorLocal(config);
                _npcFinder = new NpcAggregatorLocal(config);
            }
            _manager.DataLoaded += new SpawnGroupDataLoadedHandler(_manager_DataLoaded);            
            InitCommands();
        }

        void _manager_DataLoaded(object sender, SpawnGroupDataLoadedEventArgs e)
        {
            if (_manager.SpawnGroups.Count() > 0)
            {
                SelectedSpawnGroup = _manager.SpawnGroups.ElementAt(0);
            }
            RefreshCommands();
        }

        private void InitCommands()
        {
            RemoveSelectedEntryCommand = new DelegateCommand(
                x =>
                {
                    var entry = x as SpawnEntry;
                    if (entry != null && SelectedSpawnGroup != null)
                    {
                        SelectedSpawnGroup.RemoveEntry(entry);
                    }
                },
                x =>
                {
                    if (SelectedSpawnGroup != null && ((x as SpawnEntry) != null)) return true;
                    else return false;
                });

            ViewQueryCommand = new DelegateCommand(
                x =>
                {
                    var window = new TextWindow(_manager.GetSQL());
                    window.Show();
                },
                x =>
                {
                    return true;
                });

            CreateNewCommand = new DelegateCommand(
                x =>
                {
                    var sg = _manager.CreateSpawnGroup();
                    _manager.AddSpawnGroup(sg);
                    if (sg != null)
                    {
                        SelectedSpawnGroup = sg;
                    }
                    NotifyPropertyChanged("SpawnGroups");
                },
                x =>
                {
                    return true;
                });

            NextIdCommand = new DelegateCommand(
                x =>
                {
                    var sorted = _manager.SpawnGroups.OrderBy(g => { return g.Id; });

                    if (SelectedSpawnGroup.Id == sorted.Last().Id)
                    {
                        SelectedSpawnGroup = sorted.First();
                    }
                    else
                    {
                        SelectedSpawnGroup = sorted.First(g => { return g.Id > SelectedSpawnGroup.Id; });
                    }
                },
                x =>
                {
                    if (_manager.SpawnGroups.Count() > 1 && SelectedSpawnGroup != null)
                    {
                        return true;
                    }
                    else return false;
                });

            PreviousIdCommand = new DelegateCommand(
                x =>
                {
                    var sorted = _manager.SpawnGroups.OrderBy(g => { return g.Id; });

                    if (SelectedSpawnGroup.Id == sorted.First().Id)
                    {
                        SelectedSpawnGroup = sorted.Last();
                    }
                    else
                    {
                        SelectedSpawnGroup = sorted.Last(g => { return g.Id < SelectedSpawnGroup.Id; });
                    }
                },
                x =>
                {
                    if (_manager.SpawnGroups.Count() > 1 && SelectedSpawnGroup != null)
                    {
                        return true;
                    }
                    else return false;
                });

            RemoveSpawnGroupCommand = new DelegateCommand(
                x =>
                {
                    _manager.RemoveSpawnGroup(SelectedSpawnGroup);

                    if (_manager.SpawnGroups.Count() > 0)
                    {
                        SelectedSpawnGroup = _manager.SpawnGroups.ElementAt(0);
                    }
                    else SelectedSpawnGroup = null;
                    NotifyPropertyChanged("SpawnGroups");
                },
                x =>
                {
                    return SelectedSpawnGroup != null;
                });

            AdjustChanceTotalCommand = new DelegateCommand(
                x =>
                {
                    var entries = x as IEnumerable<object>;

                    if (entries != null && entries.Count() > 0)
                    {
                        var total = SelectedSpawnGroup.ChanceTotal;
                        var unitsLeft = 100 - total;

                        while (unitsLeft > 1)
                        {
                            foreach (SpawnEntry entry in entries)
                            {
                                entry.Chance += 1;
                                unitsLeft--;
                                if (unitsLeft == 0) break;
                            }
                        }
                    }
                    else return;
                },
                x =>
                {
                    return SelectedSpawnGroup != null && SelectedSpawnGroup.Entries.Count > 0;
                });

            ClearCacheCommand = new DelegateCommand(
                x =>
                {
                    _manager.ClearCache();
                    SelectedSpawnGroup = null;
                    ClearCacheCommand.RaiseCanExecuteChanged();
                    NotifyPropertyChanged("SpawnGroups");
                },
                x =>
                {
                    return _manager.SpawnGroups.Count() > 0;
                });

            PackCommand = new DelegateCommand(
                x =>
                {
                    PackIds();
                },
                x =>
                {
                    if (_packWork != null && _packWork.IsBusy) return false;
                    return (PackEnd > PackStart && PackEnd - PackStart > _manager.SpawnGroups.Count());
                });
        }

        public void RefreshCommands()
        {
            _adjustChanceTotalCommand.RaiseCanExecuteChanged();
            _clearCacheCommand.RaiseCanExecuteChanged();
            _createNewCommand.RaiseCanExecuteChanged();
            _nextIdCommand.RaiseCanExecuteChanged();
            _packCommand.RaiseCanExecuteChanged();
            _prevIdCommand.RaiseCanExecuteChanged();
            _removeSelectedCommand.RaiseCanExecuteChanged();
            _removeSpawnGroupCommand.RaiseCanExecuteChanged();
            _viewQueryCommand.RaiseCanExecuteChanged();
        }

        public void SaveAsXml(string zone, string dir)
        {
            _manager.SaveXML(zone, dir);
        }

        public void LoadXml(string file)
        {
            _manager.LoadXML(file);
        }

        public void LoadXmlNpcs(string file)
        {
            _npcFinder.LoadXML(file);
        }

        public void SaveXmlNpcs(string zone, string dir)
        {
            _npcFinder.SaveXML(zone, dir);
        }

        public ICollection<Npc> NPCFilter
        {
            get { return _npcFinder.NPCs; }
        }        
        
        public string NPCFilterString
        {
            get { return _npcFilterString; }
            set
            {
                _npcFilterString = value;
                _npcFinder.Lookup(value);
                NotifyPropertyChanged("NPCFilterString");
            }
        }

        public void ClearNpcCache()
        {
            _npcFinder.ClearCache();
        }

        public IEnumerable<SpawnGroup> SpawnGroups
        {
            get
            {
                if (_manager != null)
                {
                    return _manager.SpawnGroups;
                }
                else return null;
            }
        }        
        
        public SpawnGroup SelectedSpawnGroup
        {
            get { return _selectedSpawnGroup; }
            set
            {
                if (_selectedSpawnGroup != null)
                {
                    _selectedSpawnGroup.SpawnChanceTotalChanged -= _selectedSpawnGroup_SpawnChanceTotalChanged;
                }

                _selectedSpawnGroup = value;
                if (_selectedSpawnGroup != null)
                {
                    _selectedSpawnGroup.SpawnChanceTotalChanged += new SpawnChanceTotalChangedHandler(_selectedSpawnGroup_SpawnChanceTotalChanged);
                }

                RefreshSelection();

                NextIdCommand.RaiseCanExecuteChanged();
                PreviousIdCommand.RaiseCanExecuteChanged();
                RemoveSpawnGroupCommand.RaiseCanExecuteChanged();
                AdjustChanceTotalCommand.RaiseCanExecuteChanged();
                ClearCacheCommand.RaiseCanExecuteChanged();
            }
        }

        public ICollection<SpawnEntry> SpawnEntries
        {
            get 
            {
                if (_selectedSpawnGroup != null)
                {
                    return _selectedSpawnGroup.Entries;
                }
                else return null;
            }
        }

        void _selectedSpawnGroup_SpawnChanceTotalChanged(SpawnGroup sender, EventArgs e)
        {
            NotifyPropertyChanged("ChanceTotal");
        }

        public int FilterID
        {
            get { return _manager.FilterById; }
            set
            {
                _manager.FilterById = value;
                SelectedSpawnGroup = _manager.SpawnGroups
                    .Where(x => { return x.Id == value; }).FirstOrDefault();
                NotifyPropertyChanged("FilterID");
            }
        }
        
        public string ZoneFilter
        {
            get { return _zoneFilter; }
            set
            {
                _zoneFilter = value;

                var sggr = _manager as SpawnGroupAggregatorDatabase;
                _npcFinder.LookupByZone(value);
                _manager.LookupByZone(value);
                SelectedSpawnGroup = _manager.SpawnGroups.FirstOrDefault();
                NotifyPropertyChanged("ZoneFilter");
                NotifyPropertyChanged("SpawnGroups");
            }
        }
                
        public int PackStart 
        {
            get { return _packStart; } 
            set 
            {
                _packStart = value;
                NotifyPropertyChanged("PackStart");
                PackCommand.RaiseCanExecuteChanged();
            }
        }
                
        public int PackEnd
        {
            get { return _packEnd; }
            set
            {
                _packEnd = value;
                NotifyPropertyChanged("PackEnd");
                PackCommand.RaiseCanExecuteChanged();
            }
        }

        public int ChanceTotal
        {
            get
            {
                if (SelectedSpawnGroup != null)
                {
                    return SelectedSpawnGroup.ChanceTotal;
                }
                else return 0;
            }
        }

        public void RefreshSelection()
        {
            NotifyPropertyChanged("SelectedSpawnGroup");
            NotifyPropertyChanged("SpawnEntries");
            NotifyPropertyChanged("ChanceTotal");
        }

        public void PackIds()
        {
            _packWork = new BackgroundWorker();
            _packWork.WorkerReportsProgress = true;
            
            _packWork.DoWork += (s, e) =>
                {
                    _manager.PackCachedId(PackStart, PackEnd, _packWork);
                };
            _packWork.ProgressChanged += new ProgressChangedEventHandler(_packWork_ProgressChanged);
            _packWork.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_packWork_RunWorkerCompleted);
            _packWork.RunWorkerAsync();   
        }

        void _packWork_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            PackProgress = 0;
        }

        void _packWork_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PackProgress = e.ProgressPercentage;
            PackCommand.RaiseCanExecuteChanged();
        }
                
        public int PackProgress
        {
            get { return _packProgress; }
            set
            {
                _packProgress = value;
                NotifyPropertyChanged("PackProgress");
            }
        }        
        
        public SpawnEntry SelectedEntry
        {
            get { return _selectedEntry; }
            set
            {
                _selectedEntry = value;
                NotifyPropertyChanged("SelectedEntry");
                RemoveSelectedEntryCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand RemoveSpawnGroupCommand
        {
            get { return _removeSpawnGroupCommand; }
            set
            {
                _removeSpawnGroupCommand = value;
                NotifyPropertyChanged("RemoveSpawnGroupCommand");
            }
        }

        
        public DelegateCommand RemoveSelectedEntryCommand
        {
            get { return _removeSelectedCommand; }
            private set
            {
                _removeSelectedCommand = value;
                NotifyPropertyChanged("RemoveSelectedCommand");
            }
        }
                
        public DelegateCommand ViewQueryCommand
        {
            get { return _viewQueryCommand; }
            private set
            {
                _viewQueryCommand = value;
                NotifyPropertyChanged("ViewQueryCommand");
            }
        }
        
        public DelegateCommand CreateNewCommand
        {
            get { return _createNewCommand; }
            set
            {
                _createNewCommand = value;
                NotifyPropertyChanged("CreateNewCommand");
            }
        }
                
        public DelegateCommand NextIdCommand
        {
            get { return _nextIdCommand; }
            set
            {
                _nextIdCommand = value;
                NotifyPropertyChanged("NextIdCommand");
            }
        }
                
        public DelegateCommand PreviousIdCommand
        {
            get { return _prevIdCommand; }
            set
            {
                _prevIdCommand = value;
                NotifyPropertyChanged("NextIdCommand");
            }
        }
                
        public DelegateCommand AdjustChanceTotalCommand
        {
            get { return _adjustChanceTotalCommand; }
            set
            {
                _adjustChanceTotalCommand = value;
                NotifyPropertyChanged("AdjustChanceTotalCommand");
            }
        }
        
        public DelegateCommand ClearCacheCommand
        {
            get { return _clearCacheCommand; }
            set
            {
                _clearCacheCommand = value;
                NotifyPropertyChanged("ClearCacheCommand");
            }
        }
                
        public DelegateCommand PackCommand
        {
            get { return _packCommand; }
            set
            {
                _packCommand = value;
                NotifyPropertyChanged("PackCommand");
            }
        }
    }
}
