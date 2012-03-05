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
        }

        void _service_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ZoneGrids":
                    NotifyPropertyChanged("Grids");
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

        public EQEmu.Grids.Waypoint SelectedWaypoint 
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
                }
            }
        }

        private IEnumerable<EQEmu.Grids.Waypoint> _selectedWaypoints = null;
        public IEnumerable<EQEmu.Grids.Waypoint> SelectedWaypoints
        {
            get { return _selectedWaypoints; }
            set
            {
                _selectedWaypoints = value;
                NotifyPropertyChanged("SelectedWaypoints");
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
    }
}
