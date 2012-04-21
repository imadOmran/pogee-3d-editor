using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Practices.Unity;
using Microsoft.Win32;

using ApplicationCore;
using ApplicationCore.ViewModels.Editors;

using EQEmu.Files.WLD;
using EQEmu.Files.WLD.Fragments;

namespace S3DPlugin
{
    public class S3DRibbonTabViewModel : S3DViewModelBase
    {
        private DelegateCommand _renderAllCommand;

        public S3DRibbonTabViewModel([Dependency("S3DDataService")] S3DDataService _service)
            : base(_service)
        {
            _service.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(service_PropertyChanged);

            RenderAllCommand = new DelegateCommand(
                x =>
                {
                    S3DService.RenderAll();
                },
                x =>
                {
                    return S3DService != null && S3DService.WLDObject != null;
                });
        }

        void service_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "WLDObject":
                case "WLDCollection":
                    NotifyPropertyChanged("Triangles");
                    NotifyPropertyChanged("ZoneMeshes");
                    NotifyPropertyChanged("WLDFiles");
                    RenderAllCommand.RaiseCanExecuteChanged();
                    break;
                case "Status":
                    LoadStatus = S3DService.Status;
                    NotifyPropertyChanged("LoadStatus");
                    break;
                default:
                    break;
            }
        }

        public LoadStatus LoadStatus
        {
            get;
            set;
        }

        public DelegateCommand RenderAllCommand
        {
            get { return _renderAllCommand; }
            set
            {
                _renderAllCommand = value;
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

        private Mesh _selectedMesh = null;
        public Mesh SelectedMesh
        {
            get { return _selectedMesh; }
            set
            {
                _selectedMesh = value;
                S3DService.RenderMesh(value);
                NotifyPropertyChanged("SelectedMesh");
            }
        }

        public IEnumerable<Mesh> ZoneMeshes
        {
            get
            {
                if (S3DService != null && S3DService.WLDObject != null)
                {
                    return S3DService.WLDObject.ZoneMeshes;
                }
                else return null;
            }
        }
                
        public void OpenFile()
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "S3D Files (.s3d)|*.s3d|All Files (*.*)|*.*";
            if ((bool)fd.ShowDialog())
            {
                var result = System.Windows.MessageBox.Show("Load objects?", "Load objects?", System.Windows.MessageBoxButton.YesNo);
                bool useObjects = result == System.Windows.MessageBoxResult.Yes ? true : false;                
                S3DService.OpenFile(fd.FileName,useObjects);                
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
