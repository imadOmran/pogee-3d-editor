using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore;

using EQEmu.Spawns;

namespace SpawnGroupPlugin
{
    public class SpawnGroupEditTabViewModel : ApplicationCore.ViewModels.ViewModelBase
    {
        private readonly SpawnGroupAggregator _manager;
        private readonly NPCAggregator _npcFinder;

        public SpawnGroupEditTabViewModel(SpawnGroupAggregator manager, NPCAggregator finder)
        {
            if (manager == null) throw new NullReferenceException("null parameter");
            _manager = manager;
            _npcFinder = finder;

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
                    if (sg != null)
                    {
                        SelectedSpawnGroup = sg;
                    }
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
                        if (unitsLeft <= 100)
                        {
                            if (unitsLeft <= entries.Count()) return;
                            var chancePerEntry = unitsLeft / entries.Count();
                            foreach(SpawnEntry entry in entries)
                            {
                                if (entry.Chance < (short)chancePerEntry)
                                {
                                    entry.Chance = (short)chancePerEntry;
                                }
                                unitsLeft -= chancePerEntry;

                                if (entries.Last() == entry && unitsLeft > 0)
                                {
                                    entry.Chance += (short)unitsLeft;
                                }
                            }
                        }
                       
                    }
                    else return;
                },
                x =>
                {
                    return SelectedSpawnGroup != null && SelectedSpawnGroup.Entries.Count > 0;
                });
        }

        public ICollection<NPC> NPCFilter
        {
            get { return _npcFinder.NPCs; }
        }
        
        private string _npcFilterString;
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
        
        private SpawnGroup _selectedSpawnGroup = null;
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

                NotifyPropertyChanged("SelectedSpawnGroup");
                NotifyPropertyChanged("ChanceTotal");
                NextIdCommand.RaiseCanExecuteChanged();
                PreviousIdCommand.RaiseCanExecuteChanged();
                RemoveSpawnGroupCommand.RaiseCanExecuteChanged();
                AdjustChanceTotalCommand.RaiseCanExecuteChanged();
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
            NotifyPropertyChanged("ChanceTotal");
        }
        
        private SpawnEntry _selectedEntry = null;
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

        private DelegateCommand _removeSpawnGroupCommand;
        public DelegateCommand RemoveSpawnGroupCommand
        {
            get { return _removeSpawnGroupCommand; }
            set
            {
                _removeSpawnGroupCommand = value;
                NotifyPropertyChanged("RemoveSpawnGroupCommand");
            }
        }

        private DelegateCommand _removeSelectedCommand;
        public DelegateCommand RemoveSelectedEntryCommand
        {
            get { return _removeSelectedCommand; }
            private set
            {
                _removeSelectedCommand = value;
                NotifyPropertyChanged("RemoveSelectedCommand");
            }
        }

        private DelegateCommand _viewQueryCommand;
        public DelegateCommand ViewQueryCommand
        {
            get { return _viewQueryCommand; }
            private set
            {
                _viewQueryCommand = value;
                NotifyPropertyChanged("ViewQueryCommand");
            }
        }

        private DelegateCommand _createNewCommand;
        public DelegateCommand CreateNewCommand
        {
            get { return _createNewCommand; }
            set
            {
                _createNewCommand = value;
                NotifyPropertyChanged("CreateNewCommand");
            }
        }

        private DelegateCommand _nextIdCommand;
        public DelegateCommand NextIdCommand
        {
            get { return _nextIdCommand; }
            set
            {
                _nextIdCommand = value;
                NotifyPropertyChanged("NextIdCommand");
            }
        }

        private DelegateCommand _prevIdCommand;
        public DelegateCommand PreviousIdCommand
        {
            get { return _prevIdCommand; }
            set
            {
                _prevIdCommand = value;
                NotifyPropertyChanged("NextIdCommand");
            }
        }

        private DelegateCommand _adjustChanceTotalCommand;
        public DelegateCommand AdjustChanceTotalCommand
        {
            get { return _adjustChanceTotalCommand; }
            set
            {
                _adjustChanceTotalCommand = value;
                NotifyPropertyChanged("AdjustChanceTotalCommand");
            }
        }
    }
}
