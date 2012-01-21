using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.IO;

using Microsoft.Practices.Unity;

using HelixToolkit;
using HelixToolkit.Wpf;

using ApplicationCore;
using ApplicationCore.DataServices;

namespace LineOfSightAreaPlugin
{
    [AutoRegister]
    public class LineOfSightAreaDataService : DataServiceBase, IModel3DProvider
    {
        private EQEmuDisplay3D.LineOfSightAreaDisplay3D _los3d = null;
        private EQEmu.LineOfSightAreas.ZoneLineOfSightAreas _zoneAreas = null;

        public EQEmu.LineOfSightAreas.ZoneLineOfSightAreas ZoneAreas
        {
            get { return _zoneAreas; }
            set
            {
                _zoneAreas = value;
                _los3d = new EQEmuDisplay3D.LineOfSightAreaDisplay3D(value);

                if (_viewClipping != null)
                {
                    _los3d.Clipping = _viewClipping;
                }

                Model3D = new ModelVisual3D()
                {
                    Content = _los3d.Model,
                    Transform = Transform3D
                };

                NotifyPropertyChanged("ZoneAreas");
            }
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

        public event Model3DChangedHandler ModelChanged;
        protected void OnModelChanged()
        {
            if (ModelChanged != null)
            {
                ModelChanged(this, new EventArgs());
            }
        }

        private EQEmu.LineOfSightAreas.LineOfSightArea _selectedArea = null;
        public EQEmu.LineOfSightAreas.LineOfSightArea SelectedArea
        {
            get
            {
                return _selectedArea;
            }

            set
            {
                _selectedArea = value;
                if (_los3d != null)
                {
                    _los3d.ShowArea(value);
                }

                NotifyPropertyChanged("SelectedArea");
            }
        }

        private Point3D _selectedVertex = new Point3D();
        public Point3D SelectedVertex
        {
            get
            {
                return _selectedVertex;
            }
            set
            {
                _modelVisual.Children.Remove( _cursor );
                _selectedVertex = value;

                if ( SelectedArea != null ) {
                    Point3D p = new Point3D();
                    if ( SelectedArea.GetClosestVertex( value,out p ) ) {
                        _cursor.Center = new Point3D( p.X, p.Y, p.Z );
                        _modelVisual.Children.Add( _cursor );
                    }
                }
            }
        }

        private BoxVisual3D _cursor = new BoxVisual3D()
        {
            Width = 10,
            Length = 10,
            Height = 0.5,
            Material = Materials.Red
        };

        public void OpenFile(string file)
        {
            if (File.Exists(file))
            {
                ZoneAreas = EQEmu.LineOfSightAreas.ZoneLineOfSightAreas.LoadFileBinary(file);                
            }
        }

        public void NewFile()
        {
            ZoneAreas = new EQEmu.LineOfSightAreas.ZoneLineOfSightAreas();            
        }

        public void SaveFile(string file)
        {
            if (ZoneAreas != null)
            {
                ZoneAreas.SaveFileBinary(file);
            }
            /*
            if (Pathing != null)
            {
                if (!file.Contains(".xml"))
                {
                    Pathing.SaveAsBinary(file);
                }
            }
            */
        }
        
    }
}
