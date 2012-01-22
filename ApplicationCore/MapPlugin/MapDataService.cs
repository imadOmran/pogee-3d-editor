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

namespace MapPlugin
{
    [AutoRegister]
    public class MapDataService : DataServiceBase, IModel3DProvider
    {
        public MapDataService() : base()
        {
            // this is bad.... this should not be used like a viewmodel... rewrite is in order
            _dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }

        private System.Windows.Threading.Dispatcher _dispatcher = null;

        private EQEmuDisplay3D.MapDisplay3D _map3d = null;
        private EQEmu.Map.Map _map = null;
        public EQEmu.Map.Map Map
        {
            get { return _map; }
            private set
            {
                _map = value;

                if (_map3d != null)
                {
                    _map3d.Dispose();
                }

                _map3d = new EQEmuDisplay3D.MapDisplay3D(_map);

                if(_viewClipping != null)
                {
                    _map3d.Clipping = _viewClipping;
                }

                Model3D = new ModelVisual3D()
                {
                    Content = _map3d.Model,          
                    Transform = Transform3D                   
                };

                NotifyPropertyChanged("Map");
            }
        }

        public void OpenFile(string file)
        {
            if (File.Exists(file))
            {
                Task.Factory.StartNew(() =>
                    {
                        var map = EQEmu.Map.Map.LoadFile(file);
                        _dispatcher.Invoke((Action)(() =>
                            {
                                Map = map;
                            }));
                    });

                ////delegate defining work to be done
                //Func<EQEmu.Map.Map> work = (Func<EQEmu.Map.Map>)( () =>
                //    {
                //        return EQEmu.Map.Map.LoadFile(file);
                //    });

                ////asynchronously invoke delegate on thread pool
                ////1st parameter is the EndInvoke callback
                //work.BeginInvoke((res) =>
                //{
                //    //get the return value from the delegate
                //    var value = work.EndInvoke(res);                    
                //    //marshal property notification onto the correct thread
                //    _dispatcher.Invoke((Action)(() =>
                //        {
                //            Map = value;
                //        }));
                //}, null);

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
