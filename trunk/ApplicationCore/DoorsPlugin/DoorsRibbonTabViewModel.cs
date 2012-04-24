using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Practices.Unity;
using Microsoft.Win32;

using ApplicationCore;
using ApplicationCore.ViewModels.Editors;

namespace DoorsPlugin
{
    public class DoorsRibbonTabViewModel : DoorsViewModelBase
    {
        private DelegateCommand _openCommand;

        public DoorsRibbonTabViewModel([Dependency("DoorsDataService")] DoorsDataService _service)
            : base(_service)
        {
            _service.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(service_PropertyChanged);

            OpenCommand = new DelegateCommand(
                x =>
                {
                    var fd = new OpenFileDialog();
                    if((bool)fd.ShowDialog())
                    {
                        DoorService.OpenModels(fd.FileName);
                    }
                },
                x =>
                {
                    return true;
                });
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

        void service_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                default:
                    break;
            }
        }

        public override void User3DClickAt( object sender, World3DClickEventArgs e )
        {            
            //throw new NotImplementedException();
        }
    }
}
