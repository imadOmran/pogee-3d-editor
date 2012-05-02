using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using ApplicationCore;
using ApplicationCore.ViewModels.Editors;
using ApplicationCore.DataServices;

namespace GridsPlugin
{
    public abstract class GridsViewModelBase : EditorViewModelBase, IGridsViewModel
    {
        private DelegateCommand _removeZeroPauseWaypointsCommand;
        private DelegateCommand _selectGridCommand;
        private DelegateCommand _selectWaypointCommand;
        private DelegateCommand _removeAllButOneCommand;
        private DelegateCommand _removeWaypointsCommand;


        public GridsViewModelBase(GridsDataService service)
        {
            _service = service;
            _service.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_service_PropertyChanged);

            ZAdjustment = 2.0;

            DefineCommands();
        }

        private void DefineCommands()
        {
            RemoveZeroPauseWaypointsCommand = new DelegateCommand(
             x =>
             {
                 SelectedGrid.RemoveNonPauseNodes();
             },
             x =>
             {
                 return SelectedGrid != null && SelectedGrid.Waypoints.Count(wp => wp.PauseTime == 0) > 0;
             });

            SelectGridCommand = new DelegateCommand(
                x =>
                {
                    NotifyPropertyChanged("SelectedGrid");
                },
                x =>
                {
                    return SelectedGrid != null;
                });

            SelectWaypointCommand = new DelegateCommand(
                x =>
                {
                    NotifyPropertyChanged("SelectedWaypoint");
                },
                x =>
                {
                    return SelectedWaypoint != null;
                });

            RemoveAllButOneCommand = new DelegateCommand(
                x =>
                {                    
                    foreach (var wp in SelectedWaypoints)
                    {
                        if (wp == SelectedWaypoints.First()) continue;
                        SelectedGrid.RemoveWaypoint(wp);
                    }
                },
                x =>
                {
                    return SelectedGrid != null && SelectedWaypoints != null && SelectedWaypoints.Count() > 1;
                });

            RemoveWaypointsCommand = new DelegateCommand(
                x =>
                {
                    if (SelectedWaypoints != null && SelectedWaypoints.Count() > 1)
                    {
                        SelectedGrid.RemoveWaypoints(SelectedWaypoints);
                    }
                    else
                    {
                        SelectedGrid.RemoveWaypoint(SelectedWaypoint);
                    }
                },
                x =>
                {
                    return SelectedGrid != null && SelectedWaypoint != null;
                });
        }

        void _service_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ZoneGrids":
                    NotifyPropertyChanged("Grids");
                    break;
                case "Zone":
                    NotifyPropertyChanged("Zone");
                    NotifyPropertyChanged("ZoneIdNumber");
                    break;
                case "SelectedGrid":
                    NotifyPropertyChanged("SelectedGrid");
                    break;
                case "SelectedWaypoint":
                    NotifyPropertyChanged("SelectedWaypoint");
                    break;
                default:
                    break;
            }
        }

        abstract public override void User3DClickAt(object sender, World3DClickEventArgs e);

        public override IDataService Service
        {
            get { return _service; }
        }

        private readonly GridsDataService _service;
        public GridsDataService GridsService
        {
            get { return _service; }
        }

        public ObservableCollection<EQEmu.Grids.Grid> Grids
        {
            get
            {
                if (_service != null && _service.ZoneGrids != null)
                {
                    return _service.ZoneGrids.Grids;
                }
                else return null;
            }
        }

        public double ZAdjustment
        {
            get;
            set;
        }

        public int ZoneIdNumber
        {
            get 
            {
                if (_service != null && _service.ZoneGrids != null)
                {
                    return _service.ZoneGrids.ZoneId;
                }
                else return 0;
            }
            set
            {
                if (_service != null && _service.ZoneGrids != null)
                {
                    _service.ZoneGrids.ZoneId = value;
                    NotifyPropertyChanged("ZoneIdNumber");
                }
            }
        }

        public EQEmu.Grids.Grid SelectedGrid 
        {
            get
            {
                if (_service != null)
                {
                    return _service.SelectedGrid;
                }
                else return null;
            }
            set
            {
                if (_service != null)
                {
                    _service.SelectedGrid = value;
                    RemoveZeroPauseWaypointsCommand.RaiseCanExecuteChanged();
                    SelectGridCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public virtual EQEmu.Grids.Waypoint SelectedWaypoint 
        {
            get
            {
                if (_service != null)
                {
                    return _service.SelectedWaypoint;
                }
                else return null;
            }
            set
            {
                if (_service != null)
                {
                    _service.SelectedWaypoint = value;                    
                    NotifyPropertyChanged("SelectedWaypoint");
                    SelectWaypointCommand.RaiseCanExecuteChanged();
                    RemoveWaypointsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private IEnumerable<EQEmu.Grids.Waypoint> _selectedWaypoints = null;
        public virtual IEnumerable<EQEmu.Grids.Waypoint> SelectedWaypoints
        {
            get { return _selectedWaypoints; }
            set
            {
                _selectedWaypoints = value;
                NotifyPropertyChanged("SelectedWaypoints");
                RemoveAllButOneCommand.RaiseCanExecuteChanged();
                RemoveWaypointsCommand.RaiseCanExecuteChanged();
            }
        }
        
        public string Zone
        {
            get
            {
                if (_service != null)
                {
                    return _service.Zone;
                }
                else return "";
            }
            set
            {
                if (_service != null)
                {
                    _service.Zone = value;
                }
                NotifyPropertyChanged("Zone");
                NotifyPropertyChanged("ZoneIdNumber");
            }
        }

        public DelegateCommand RemoveZeroPauseWaypointsCommand
        {
            get { return _removeZeroPauseWaypointsCommand; }
            set
            {
                _removeZeroPauseWaypointsCommand = value;
                NotifyPropertyChanged("RemoveZeroPauseWaypointsCommand");
            }
        }

        public DelegateCommand SelectGridCommand
        {
            get { return _selectGridCommand; }
            set
            {
                _selectGridCommand = value;
                NotifyPropertyChanged("SelectGridCommand");
            }
        }

        public DelegateCommand SelectWaypointCommand
        {
            get { return _selectWaypointCommand; }
            set
            {
                _selectWaypointCommand = value;
                NotifyPropertyChanged("SelectWaypointCommand");
            }
        }

        public DelegateCommand RemoveAllButOneCommand
        {
            get { return _removeAllButOneCommand; }
            set
            {
                _removeAllButOneCommand = value;
                NotifyPropertyChanged("RemoveAllButOneCommand");
            }
        }

        public DelegateCommand RemoveWaypointsCommand
        {
            get { return _removeWaypointsCommand; }
            set
            {
                _removeWaypointsCommand = value;
                NotifyPropertyChanged("RemoveWaypointsCommand");
            }
        }
    }
}
