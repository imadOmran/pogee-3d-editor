using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using ApplicationCore.ViewModels.Editors;
using ApplicationCore.DataServices;

namespace SpawnsPlugin
{
    public abstract class SpawnsViewModelBase : EditorViewModelBase, ISpawnsViewModel
    {
        public SpawnsViewModelBase(SpawnDataService service)
        {
            _service = service;
            _service.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_service_PropertyChanged);
        }

        void _service_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedSpawn":
                    NotifyPropertyChanged("SelectedSpawn");
                    break;
                case "SelectedSpawns":
                    NotifyPropertyChanged("SelectedSpawns");
                    break;
                case "ZoneSpawns":
                    NotifyPropertyChanged("Spawns");
                    break;
                case "Zone":
                    NotifyPropertyChanged("Zone");
                    break;
                case "Version":
                    NotifyPropertyChanged("Version");                   
                    break;
                default:
                    break;
            }
        }
        
        public override IDataService Service
        {
            get { return _service; }
        }

        private readonly SpawnDataService _service = null;
        public SpawnDataService SpawnsService
        {
            get { return _service; }
        }

        public ObservableCollection<EQEmu.Spawns.Spawn2> Spawns
        {
            get
            {
                if (_service != null && _service.ZoneSpawns != null)
                {
                    return _service.ZoneSpawns.Spawns;
                }
                else return null;
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
                return "";
            }
            set
            {
                if (_service != null)
                {
                    _service.Zone = value;
                }
                NotifyPropertyChanged("Zone");
                NotifyPropertyChanged("ServiceOnline");
            }
        }

        private bool _allZoneVersions = false;
        public bool AllZoneVersions
        {
            get { return _allZoneVersions; }
            set
            {
                _allZoneVersions = value;
                if (value)
                {
                    Version = -1;
                }
                else
                {
                    Version = 0;
                }
            }
        }

        public bool ServiceOnline
        {
            get { return _service != null && _service.ZoneSpawns != null; }
        }

        public int Version
        {
            get
            {
                if (_service != null)
                {
                    return _service.Version;
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                if (_service != null )
                {
                    _service.Version = value;
                    NotifyPropertyChanged("Version");
                }
            }
        }

        public bool SaveToFile(string file)
        {
            if (SpawnsService != null && SpawnsService.ZoneSpawns != null)
            {
                SpawnsService.ZoneSpawns.SaveXML(file);
                return true;
            }
            return false;
        }

        public bool LoadFile(string file)
        {
            if (SpawnsService != null)
            {
                var zone = SpawnsService.CreateNewZone(Zone, Version, true);
                zone.LoadXML(file);
                return true;
            }
            return false;
        }

        abstract public EQEmu.Spawns.Spawn2 SelectedSpawn { get; set; }
        abstract public void CreateNewSpawn(System.Windows.Media.Media3D.Point3D p);
        abstract public IEnumerable<EQEmu.Spawns.Spawn2> SelectedSpawns { get; set; }
    }
}
