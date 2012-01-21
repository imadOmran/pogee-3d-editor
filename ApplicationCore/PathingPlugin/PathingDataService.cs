using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Practices.Unity;

using HelixToolkit;
using HelixToolkit.Wpf;

using ApplicationCore;
using ApplicationCore.DataServices;

namespace PathingPlugin
{
    [AutoRegister]
    public class PathingDataService : DataServiceBase, IModel3DProvider
    {
        public PathingDataService()
            : base()
        {
            _dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }

        private System.Windows.Threading.Dispatcher _dispatcher;

        private EQEmuDisplay3D.PathDisplay3D _path3d = null;
        private EQEmu.Path.Path _path = null;
        public EQEmu.Path.Path Pathing
        {
            get { return _path; }
            set
            {
                _path = value;
                _path.NodeAdded += new EQEmu.Path.Path.NodeModifiedHandler( path_NodeAdded );
                _path.NodeRemoved += new EQEmu.Path.Path.NodeModifiedHandler(path_NodeRemoved);

                _path3d = new EQEmuDisplay3D.PathDisplay3D(_path);                

                if (_viewClipping != null)
                {
                    _path3d.Clipping = _viewClipping;
                }

                Model3D = new ModelVisual3D()
                {
                    Content = _path3d.Model,
                    Transform = Transform3D
                };

                NotifyPropertyChanged("Pathing");
            }
        }

        void path_NodeRemoved(object sender, EQEmu.Path.PathModifiedEventArgs e)
        {
            NotifyPropertyChanged("Pathing");
        }

        void path_NodeAdded( object sender, EQEmu.Path.PathModifiedEventArgs e )
        {
            NotifyPropertyChanged( "Pathing" );
        }

        public void OpenFile(string file)
        {
            if (File.Exists(file))
            {
                Task.Factory.StartNew(() =>
                {
                    var path = EQEmu.Path.Path.LoadFile(file);                    
                    _dispatcher.Invoke((Action)(() =>
                    {
                        Pathing = path;
                    }));
                });

                //Pathing = EQEmu.Path.Path.LoadFile(file);
            }
        }

        public void NewFile()
        {
            Pathing = new EQEmu.Path.Path();
        }

        public void SaveFile(string file)
        {
            if (Pathing != null)
            {
                if (!file.Contains(".xml"))
                {
                    Pathing.SaveAsBinary(file);
                }
            }
        }

        private ModelVisual3D _modelVisual = null;
        public ModelVisual3D Model3D
        {
            get { return _modelVisual; }
            private set
            {
                if (_modelVisual != null)
                {
                    _modelVisual.Children.Clear();
                }

                _modelVisual = value;
                NotifyPropertyChanged("Model3D");
                OnModelChanged();
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

        public event Model3DChangedHandler ModelChanged;
        protected void OnModelChanged()
        {
            if (ModelChanged != null)
            {
                ModelChanged(this, new EventArgs());
            }
        }

        private BoxVisual3D _cursor = new BoxVisual3D()
        {
            Width = 5,
            Length = 5,
            Height = 0.1,
            Material = Materials.Blue
        };

        private EQEmu.Path.Node _selectedNode = null;
        public EQEmu.Path.Node SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (_selectedNode != null)
                {
                    _selectedNode.PropertyChanged -= selectedNode_PropertyChanged;
                }

                _modelVisual.Children.Remove( _cursor );
                _selectedNode = value;

                if ( _selectedNode != null ) {
                    _cursor.Center = new Point3D( _selectedNode.X, _selectedNode.Y, _selectedNode.Z );

                    _selectedNode.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(selectedNode_PropertyChanged);

                    _modelVisual.Children.Add( _cursor );
                }

                NotifyPropertyChanged( "SelectedNode" );
            }
        }

        private void selectedNode_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _cursor.Center = new Point3D(_selectedNode.X, _selectedNode.Y, _selectedNode.Z);
            NotifyPropertyChanged("SelectedNode");
        }
        
        private EQEmuDisplay3D.ViewClipping _viewClipping = null;
        [Dependency]
        public EQEmuDisplay3D.ViewClipping ViewClipping
        {
            get
            {
                return _viewClipping;
            }
            set
            {
                _viewClipping = value;
                NotifyPropertyChanged("ViewClipping");
            }
        }
    }
}
