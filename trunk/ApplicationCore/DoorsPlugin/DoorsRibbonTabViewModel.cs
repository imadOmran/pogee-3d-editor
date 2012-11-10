using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Input;

using Microsoft.Practices.Unity;
using Microsoft.Win32;

using ApplicationCore;
using ApplicationCore.ViewModels.Editors;

using EQEmu.Doors;
using EQEmu.Files.WLD.Fragments;

namespace DoorsPlugin
{
    public class DoorsRibbonTabViewModel : DoorsViewModelBase
    {
        private DelegateCommand _openCommand;
        private DelegateCommand _removeDoorCommand;
        private DelegateCommand _setMeshCommand;

        public DoorsRibbonTabViewModel([Dependency("DoorsDataService")] DoorsDataService _service)
            : base(_service)
        {
            _service.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(service_PropertyChanged);

            OpenCommand = new DelegateCommand(
                x =>
                {
                    var fd = new OpenFileDialog();
                    fd.Filter = "S3D Files | *_obj.s3d";
                    if((bool)fd.ShowDialog())
                    {
                        DoorService.OpenModels(fd.FileName);
                    }
                },
                x =>
                {
                    return true;
                });

            RemoveDoorCommand = new DelegateCommand(
                x =>
                {
                    DoorService.DoorManager.RemoveDoor(SelectedDoor);
                    SelectedDoor = null;
                },
                x =>
                {
                    return SelectedDoor != null && DoorService != null && DoorService.DoorManager != null;
                });

            SetMeshCommand = new DelegateCommand(
                x =>
                {
                    string rem = "_DMSPRITEDEF";
                    SelectedDoor.Name = SelectedMesh.FragmentName.Replace(rem, "");
                    NotifyPropertyChanged("SelectedDoor");
                },
                x =>
                {
                    return SelectedDoor != null && SelectedMesh != null;
                });
        }

        public DelegateCommand SetMeshCommand
        {
            get { return _setMeshCommand; }
            set
            {
                _setMeshCommand = value;
                NotifyPropertyChanged("SetMeshCommand");
            }
        }

        public DelegateCommand RemoveDoorCommand
        {
            get { return _removeDoorCommand; }
            set
            {
                _removeDoorCommand = value;
                NotifyPropertyChanged("RemoveDoorCommand");
            }
        }

        public DelegateCommand OpenCommand
        {
            get { return _openCommand; }
            set
            {
                _openCommand = value;
                NotifyPropertyChanged("OpenCommand");
            }
        }

        public IEnumerable<Door> Doors
        {
            get
            {
                if (DoorService != null && DoorService.DoorManager != null)
                {
                    return DoorService.DoorManager.Doors;
                }
                else return null;
            }
        }

        public override Door SelectedDoor
        {
            get
            {
                return base.SelectedDoor;
            }
            set
            {
                base.SelectedDoor = value;
                if (DoorService != null)
                {
                    DoorService.ShowDoorsSelected(new List<Door>() { value });
                }
                RemoveDoorCommand.RaiseCanExecuteChanged();
                SetMeshCommand.RaiseCanExecuteChanged();
            }
        }

        [OptionalDependency("WorldTransform")]
        public Transform3D Transform3D
        {
            get;
            set;
        }

        void service_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "WLDObject":
                    NotifyPropertyChanged("MeshModels");
                    break;
                case "Zone":
                    NotifyPropertyChanged("Doors");
                    NotifyPropertyChanged("Zone");                    
                    break;
                case "Version":
                    Version = DoorService.Version;
                    NotifyPropertyChanged("Version");
                    break;
                default:
                    break;
            }
        }

        public IEnumerable<Mesh> MeshModels
        {
            get
            {
                if (DoorService != null && DoorService.WLDObject != null)
                {
                    return DoorService.WLDObject.ZoneMeshes;
                }
                else return null;
            }
        }

        private Mesh _selectedMesh = null;
        public Mesh SelectedMesh
        {
            get { return _selectedMesh; }
            set
            {
                _selectedMesh = value;                
                NotifyPropertyChanged("SelectedMesh");
                SetMeshCommand.RaiseCanExecuteChanged();
            }
        }


        public override void User3DClickAt(object sender, World3DClickEventArgs e)
        {
            if (e.ActiveRibbonControl as IDoorsControl == null) return;
            if (DoorService == null || DoorService.DoorManager == null) return;

            base.User3DClickAt(sender, e);
        }

        protected override void OnLeftMouseClick(object sender, World3DClickEventArgs e)
        {
            base.OnLeftMouseClick(sender, e);
            Point3D p = new Point3D(e.PointInWorld.X, e.PointInWorld.Y, e.PointInWorld.Z);
            if (Transform3D != null)
            {
                Transform3D.TryTransform(p, out p);
            }

            Door door = null;

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                door = DoorService.DoorManager.DoorFactory();
                door.UnlockObject();
                door.X = (float)p.X;
                door.Y = (float)p.Y;
                door.Z = (float)p.Z;
                door.Created();

                DoorService.DoorManager.AddDoor(door);
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && SelectedDoor != null)
            {
                SelectedDoor.LookAt(p);
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftAlt) && SelectedDoor != null)
            {
                SelectedDoor.X = (float)p.X;
                SelectedDoor.Y = (float)p.Y;
                SelectedDoor.Z = (float)p.Z;
                return;
            }

            door = DoorService.GetClosestDoor(p);
            if (door != null)
            {
                SelectedDoor = door;
            }
        }
    }
}
