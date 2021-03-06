﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Practices.Unity;
using Microsoft.Win32;

using ApplicationCore;
using ApplicationCore.ViewModels.Editors;

namespace MapPlugin
{
    public class MapRibbonTabViewModel : MapViewModelBase
    {
        public MapRibbonTabViewModel([Dependency("MapDataService")] MapDataService _service) : base(_service)
        {
            _service.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(service_PropertyChanged);
        }

        void service_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Map":
                    CloseCommand.RaiseCanExecuteChanged();                    
                    NotifyPropertyChanged("Triangles");
                    break;
                default:
                    break;
            }
        }

        public int Triangles
        {
            get
            {
                if (MapService != null && MapService.Map != null)
                {
                    return this.MapService.Map.Triangles.Count;
                }
                else return 0;
            }
        }

        public void OpenFile()
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Map Files (.map)|*.map|All Files (*.*)|*.*";
            if ((bool)fd.ShowDialog())
            {
                MapService.OpenFile(fd.FileName);
            }
        }

        public override void User3DClickAt( object sender, World3DClickEventArgs e )
        {            
            //throw new NotImplementedException();
        }
        
        public override bool CanExecuteOpenCommand(object arg)
        {
            return true;
        }

        public override void ExecuteOpenCommand(object arg)
        {
            OpenFile();
            return;
        }

        public override bool CanExecuteCloseCommand(object arg)
        {
            if (MapService != null && MapService.Map != null) return true;
            else return false;            
        }

        public override void ExecuteCloseCommand(object arg)
        {
            MapService.CloseMap();
        }
    }
}
