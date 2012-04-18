using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Media3D;
using System.Threading.Tasks;

using Microsoft.Practices.Unity;

using ApplicationCore;
using ApplicationCore.DataServices;

using EQEmu.Files;
using EQEmu.Files.WLD;
using EQEmu.Files.S3D;

namespace S3DPlugin
{
    [AutoRegister]
    public class S3DDataService : DataServiceBase, IModel3DProvider
    {
        public S3DDataService()
            : base()
        {
            // this is bad.... this should not be used like a viewmodel... rewrite is in order
            _dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }

        private System.Windows.Threading.Dispatcher _dispatcher = null;
        private EQEmuDisplay3D.WldDisplay3D _display3d = null;
        private WLD _wld = null;
        
        public WLD WLDObject
        {
            get { return _wld; }
            private set
            {
                _wld = value;

                if (_display3d != null)
                {
                    _display3d.Dispose();
                }

                _display3d = new EQEmuDisplay3D.WldDisplay3D(_wld);

                if(_viewClipping != null)
                {
                    _display3d.Clipping = _viewClipping;
                }

                Model3D = new ModelVisual3D()
                {
                    Content = _display3d.Model,          
                    //Transform = Transform3D                   
                };

                NotifyPropertyChanged("WLDObject");
            }
        }

        public void OpenFile(string file)
        {
            if (File.Exists(file))
            {
                Task.Factory.StartNew(() =>
                    {
                        var s3d = S3D.Load(file);
                        var filename = System.IO.Path.GetFileName(file);
                        int period = filename.IndexOf('.', 0);
                        var zone = filename.Substring(0, period);

                        var archiveFile = s3d.Files.FirstOrDefault(x => x.Name.Contains(zone + ".wld"));
                        if (archiveFile == null) return;

                        WLD wld = null;
                        using (var ms = new MemoryStream(archiveFile.Bytes))
                        {
                            wld = WLD.Load(ms);                            
                        }

                        if (wld != null)
                        {
                            _dispatcher.Invoke((Action)(() =>
                                {
                                    wld.Files = s3d;
                                    WLDObject = wld;
                                }));
                        }
                    });
            }
        }

        private ModelVisual3D _modelVisual = null;
        public ModelVisual3D Model3D
        {
            get
            {
                return _modelVisual;
            }
            private set
            {
                _modelVisual = value;
                NotifyPropertyChanged("Model3D");
                OnModelChanged();
            }
        }

        public event Model3DChangedHandler ModelChanged;
        protected void OnModelChanged()
        {
            if (ModelChanged != null)
            {
                ModelChanged(this, new EventArgs());
            }
        }
        
        private Transform3D _transform3d = null;
        [Dependency("WorldTransform")]
        public Transform3D Transform3D
        {
            get { return _transform3d; }
            set
            {
                _transform3d = value;
            }
        }


        private EQEmuDisplay3D.ViewClipping _viewClipping = null;
        [Dependency]
        public EQEmuDisplay3D.ViewClipping ViewClipping
        {
            get { return _viewClipping; }
            set
            {
                _viewClipping = value;
                NotifyPropertyChanged("ViewClipping");
            }
        }
    }
}
