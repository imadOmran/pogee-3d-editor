using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using ApplicationCore.ViewModels.Editors;
using ApplicationCore.DataServices;

namespace GroundSpawnsPlugin
{
    public abstract class GroundSpawnsViewModelBase : EditorViewModelBase, IGroundSpawnsViewModel
    {
        public GroundSpawnsViewModelBase(GroundSpawnsDataService service)
        {
            _service = service;
            _service.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_service_PropertyChanged);

            ZAdjustment = 2.0;
        }

        void _service_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //switch (e.PropertyName)
            //{
            //    case "ZoneGroundSpawns":
            //        NotifyPropertyChanged("Grids");
            //        break;
            //    case "SelectedGrid":
            //        NotifyPropertyChanged("SelectedGrid");
            //        break;
            //    case "SelectedWaypoint":
            //        NotifyPropertyChanged("SelectedWaypoint");
            //        break;
            //    default:
            //        break;
            //}
        }

        abstract public override void User3DClickAt(object sender, World3DClickEventArgs e);

        public override IDataService Service
        {
            get { return _service; }
        }

        private readonly GroundSpawnsDataService _service;
        public GroundSpawnsDataService GroundSpawnsService
        {
            get { return _service; }
        }
        
        public double ZAdjustment
        {
            get;
            set;
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


        public EQEmu.GroundSpawns.GroundSpawn SelectedGroundSpawn
        {
            get
            {
                if (Service != null)
                {
                    return GroundSpawnsService.SelectedGroundSpawn;
                }
                else return null;
            }
            set
            {
                if (Service != null)
                {
                    GroundSpawnsService.SelectedGroundSpawn = value;
                }
                NotifyPropertyChanged("SelectedGroundSpawn");
            }
        }

        public IEnumerable<EQEmu.GroundSpawns.GroundSpawn> SelectedGroundSpawns
        {
            get
            {
                if (Service != null)
                {
                    return GroundSpawnsService.SelectedGroundSpawns;
                }
                else return null;
            }

            set
            {
                if (Service != null)
                {
                    GroundSpawnsService.SelectedGroundSpawns = value;
                    NotifyPropertyChanged("SelectedGroundSpawns");
                }               
            }
        }
    }
}
