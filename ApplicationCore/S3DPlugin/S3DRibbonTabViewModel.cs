using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Practices.Unity;
using Microsoft.Win32;

using ApplicationCore;
using ApplicationCore.ViewModels.Editors;

namespace S3DPlugin
{
    public class S3DRibbonTabViewModel : S3DViewModelBase
    {
        public S3DRibbonTabViewModel([Dependency("S3DDataService")] S3DDataService _service)
            : base(_service)
        {
            _service.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(service_PropertyChanged);
        }

        void service_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "WLDObject":
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
                if (S3DService != null && S3DService.WLDObject != null)
                {
                    int count = 0;

                    foreach (var z in S3DService.WLDObject.ZoneMeshes)
                    {
                        count += z.Polygons.Count();
                    }
                    return count;
                }
                else return 0;
            }
        }

        public void OpenFile()
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "S3D Files (.s3d)|*.s3d|All Files (*.*)|*.*";
            if ((bool)fd.ShowDialog())
            {
                S3DService.OpenFile(fd.FileName);                
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
    }
}
