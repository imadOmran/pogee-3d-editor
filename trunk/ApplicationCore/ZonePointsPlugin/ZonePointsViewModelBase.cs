using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore;
using ApplicationCore.ViewModels.Editors;
using ApplicationCore.DataServices;

namespace ZonePointsPlugin
{
    public class ZonePointsViewModelBase : EditorViewModelBase, IZonePointsViewModel
    {
        private DelegateCommand _addZonePointCommand;
        private DelegateCommand _removeZonePointCommand;

        public ZonePointsViewModelBase(ZonePointsDataService service)
        {
            _service = service;

            AddZonePointCommand = new DelegateCommand(
                x =>
                {
                    SelectedZonePoint = ZonePointsService.ZonePoints.Create();
                    SelectedZonePoint.TargetZoneId = TargetZoneId;
                },
                x =>
                {
                    return ZonePointsService != null && ZonePointsService.ZonePoints != null;
                });

            RemoveZonePointCommand = new DelegateCommand(
                x =>
                {
                    ZonePointsService.ZonePoints.Remove(SelectedZonePoint);
                },
                x =>
                {
                    return ZonePointsService != null && ZonePointsService.ZonePoints != null && SelectedZonePoint != null;
                });

        }

        public DelegateCommand AddZonePointCommand
        {
            get { return _addZonePointCommand; }
            set
            {
                _addZonePointCommand = value;
                NotifyPropertyChanged("AddZonePointCommand");
            }
        }

        public DelegateCommand RemoveZonePointCommand
        {
            get { return _removeZonePointCommand; }
            set
            {
                _removeZonePointCommand = value;
                NotifyPropertyChanged("RemoveZonePointCommand");
            }
        }

        private EQEmu.Zone.ZonePoint _selectedZonePoint = null;
        public EQEmu.Zone.ZonePoint SelectedZonePoint
        {
            get { return _selectedZonePoint; }
            set
            {
                _selectedZonePoint = value;

                if (_service != null)
                {
                    ZonePointsService.SelectedZonePoint = value;
                }
                RefreshCommands();
                NotifyPropertyChanged("SelectedZonePoint");
            }
        }

        public double ZAdjustment
        {
            get;
            set;
        }

        public int TargetZoneId
        {
            get;
            set;
        }

        private IEnumerable<EQEmu.Zone.ZonePoint> _selectedZonePoints;
        public IEnumerable<EQEmu.Zone.ZonePoint> SelectedZonePoints
        {
            get { return _selectedZonePoints; }
            set
            {
                _selectedZonePoints = value;
                NotifyPropertyChanged("SelectedZonePoints");
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
                NotifyPropertyChanged("ZonePoints");
                RefreshCommands();
            }
        }

        private void RefreshCommands()
        {
            AddZonePointCommand.RaiseCanExecuteChanged();
            RemoveZonePointCommand.RaiseCanExecuteChanged();
        }

        public override void User3DClickAt(object sender, World3DClickEventArgs e)
        {
            throw new NotImplementedException();
        }

        private readonly ZonePointsDataService _service;
        public ZonePointsDataService ZonePointsService
        {
            get { return _service; }
        }

        public override IDataService Service
        {
            get { return _service; }
        }

        public ICollection<EQEmu.Zone.ZonePoint> ZonePoints
        {
            get
            {
                if (_service != null && _service.ZonePoints != null)
                {
                    return _service.ZonePoints.Points;
                }
                else return null;
            }
        }
    }
}
