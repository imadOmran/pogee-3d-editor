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

        public SpawnGroupEditTabViewModel(SpawnGroupAggregator manager)
        {
            if (manager == null) throw new NullReferenceException("null parameter");
            _manager = manager;
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
                    SelectedSpawnGroup = null;
                },
                x =>
                {
                    return SelectedSpawnGroup != null;
                });
        }
        
        private SpawnGroup _selectedSpawnGroup = null;
        public SpawnGroup SelectedSpawnGroup
        {
            get { return _selectedSpawnGroup; }
            set
            {
                _selectedSpawnGroup = value;
                NotifyPropertyChanged("SelectedSpawnGroup");
                NextIdCommand.RaiseCanExecuteChanged();
                PreviousIdCommand.RaiseCanExecuteChanged();
                RemoveSpawnGroupCommand.RaiseCanExecuteChanged();
            }
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
    }
}
