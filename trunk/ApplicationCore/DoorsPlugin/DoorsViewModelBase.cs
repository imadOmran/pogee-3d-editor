using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Practices.Unity;

using ApplicationCore;
using ApplicationCore.ViewModels.Editors;
using ApplicationCore.DataServices;

using EQEmu.Doors;

namespace DoorsPlugin
{
    abstract public class DoorsViewModelBase : EditorViewModelBase, IDoorsViewModel
    {
        public DoorsViewModelBase(DoorsDataService service)
        {
            _service = service;
        }

        public string Zone
        {
            get 
            {
                if (_service != null)
                {
                    return _service.Zone;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (_service != null)
                {
                    _service.LoadZone(value);
                    NotifyPropertyChanged("Zone");
                }
            }
        }
        
        public abstract override void User3DClickAt( object sender, World3DClickEventArgs e );

        public override IDataService Service { get { return _service; } }

        private Door _selectedDoor;
        public virtual Door SelectedDoor
        {
            get { return _selectedDoor; }
            set
            {
                _selectedDoor = value;
                NotifyPropertyChanged("SelectedDoor");
            }
        }

        private readonly DoorsDataService _service = null;
        public DoorsDataService DoorService
        {
            get { return _service; }
        }
    }
}
