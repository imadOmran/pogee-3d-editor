using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.ViewModels.Editors;
using ApplicationCore.DataServices;

namespace RoamAreasPlugin
{
    abstract public class RoamAreasViewModelBase : EditorViewModelBase, IRoamAreasViewModel
    {
        abstract public override void User3DClickAt(object sender, World3DClickEventArgs e);

        private readonly RoamAreasDataService _service;

        public RoamAreasViewModelBase(RoamAreasDataService service)
        {
            _service = service;
            _service.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_service_PropertyChanged);
        }

        void  _service_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ZoneAreas":             
                    NotifyPropertyChanged("Areas");
                    break;
                case "SelectedArea":
                    NotifyPropertyChanged("SelectedArea");
                    break;
                default:
                    break;
            }
        }

        public override IDataService Service
        {
            get { return _service; }
        }

        public RoamAreasDataService RoamAreasDataService
        {
            get { return _service; }
        }

        public EQEmu.RoamAreas.RoamArea SelectedArea
        {
            get
            {
                return RoamAreasDataService.SelectedArea;
            }
            set
            {
                RoamAreasDataService.SelectedArea = value;
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<EQEmu.RoamAreas.RoamArea> Areas
        {
            get
            {
                if (RoamAreasDataService != null && RoamAreasDataService.ZoneAreas != null)
                {
                    return RoamAreasDataService.ZoneAreas.RoamAreas;
                }
                else return null;
            }
        }

        public EQEmu.RoamAreas.RoamArea CreateNewArea()
        {
            if (RoamAreasDataService != null && RoamAreasDataService.ZoneAreas != null)
            {
                var area = RoamAreasDataService.ZoneAreas.NewArea();
                area.MaxZ = this.DefaultMaxZ;
                area.MinZ = this.DefaultMinZ;

                RoamAreasDataService.ZoneAreas.AddArea(area);

                //TODO FIX
                RoamAreasDataService.SelectedArea = area;
                return area;
            }

            return null;
        }

        public string Zone
        {
            get
            {
                if (_service != null)
                {
                    return _service.Zone;
                }
                return "";
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

        private float _defaultMaxZ = 0.0f;
        public float DefaultMaxZ
        {
            get { return _defaultMaxZ; }
            set
            {
                _defaultMaxZ = value;
                NotifyPropertyChanged("DefaultMaxZ");
            }
        }

        private float _defaultMinZ = 0.0f;
        public float DefaultMinZ
        {
            get { return _defaultMinZ; }
            set
            {
                _defaultMinZ = value;
                NotifyPropertyChanged("DefaultMinZ");
            }
        }
    }
}
