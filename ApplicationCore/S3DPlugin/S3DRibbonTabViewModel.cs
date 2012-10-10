using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Practices.Unity;
using Microsoft.Win32;

using ApplicationCore;
using ApplicationCore.ViewModels.Editors;

using EQEmu.Files.S3D;
using EQEmu.Files.WLD;
using EQEmu.Files.WLD.Fragments;

namespace S3DPlugin
{
    public class S3DRibbonTabViewModel : S3DViewModelBase
    {
        private DelegateCommand _renderAllCommand;
        private DelegateCommand _saveSelectedFileCommand;        

        private ArchiveFile _selectedFile;

        private int _modelTextureNumber = 0;
        private int _modelHeadNumber = 0;

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
                    NotifyPropertyChanged("Models");
                    NotifyPropertyChanged("WLDFiles");
                    NotifyPropertyChanged("Files");
                    RenderAllCommand.RaiseCanExecuteChanged();
                    break;
                case "Status":
                    LoadStatus = S3DService.Status;
                    NotifyPropertyChanged("Files");
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

        public int TextureNumber
        {
            get { return _modelTextureNumber; }
            set
            {
                _modelTextureNumber = value;
                if (SelectedModel != null) SelectedModel = SelectedModel;
                NotifyPropertyChanged("TextureNumber");
            }
        }

        public int HeadNumber
        {
            get { return _modelHeadNumber; }
            set
            {
                _modelHeadNumber = value;
                if (SelectedModel != null) SelectedModel = SelectedModel;
                NotifyPropertyChanged("HeadNumber");
            }
        }

        public DelegateCommand RenderAllCommand
        {
            get { return _renderAllCommand; }
            set
            {
                _renderAllCommand = value;
            }
        }

        public DelegateCommand SaveSelectedFileCommand
        {
            get
            {
                if (_saveSelectedFileCommand == null)
                {
                    _saveSelectedFileCommand = new DelegateCommand(
                        x =>
                        {
                            var sd = new SaveFileDialog();
                            sd.FileName = _selectedFile.Name;
                            if ((bool)sd.ShowDialog())
                            {
                                using (var fs = new FileStream(sd.FileName, FileMode.Create))
                                {
                                    fs.Write(_selectedFile.Bytes, 0, _selectedFile.Bytes.Count());
                                    fs.Close();
                                }
                            }
                        },
                        y =>
                        {
                            return SelectedFile != null;
                        });

                }
                return _saveSelectedFileCommand;
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

        public ArchiveFile SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                _selectedFile = value;              
                NotifyPropertyChanged("SelectedFile");
                SaveSelectedFileCommand.RaiseCanExecuteChanged();
            }
        }

        public IEnumerable<ArchiveFile> Files
        {
            get
            {
                if (S3DService != null && S3DService.S3DArchive != null)
                {
                    return S3DService.S3DArchive.Files;
                }
                else return null;
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

        private ModelReference _selectedModel = null;
        public ModelReference SelectedModel
        {
            get { return _selectedModel; }
            set
            {
                _selectedModel = value;

                if (_selectedModel != null)
                {
                    S3DService.RenderModel(value, _modelTextureNumber, _modelHeadNumber);
                }

                NotifyPropertyChanged("SelectedModel");
            }
        }

        public IEnumerable<ModelReference> Models
        {
            get
            {
                if (S3DService != null && S3DService.WLDObject != null)
                {
                    return S3DService.WLDObject.Models;
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
                bool useObjects = false;
                var fname = Path.GetFileName(fd.FileName);
                if (!fname.Contains('_'))
                {
                    var result = System.Windows.MessageBox.Show("Load objects?", "Load objects?", System.Windows.MessageBoxButton.YesNo);
                    useObjects = result == System.Windows.MessageBoxResult.Yes ? true : false;
                }

                bool useNewTextures = true;
                var result2 = System.Windows.MessageBox.Show("Does this archive use hi-res files? If textures are flipped choose the opposite selection", "Texture Format", System.Windows.MessageBoxButton.YesNo);
                useNewTextures = result2 == System.Windows.MessageBoxResult.Yes ? true : false;

                S3DService.OpenFile(fd.FileName, useObjects, useNewTextures);
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
