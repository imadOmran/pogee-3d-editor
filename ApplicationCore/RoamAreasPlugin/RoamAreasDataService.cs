using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.IO;

using MySql.Data.MySqlClient;

using Microsoft.Practices.Unity;

using HelixToolkit;
using HelixToolkit.Wpf;

using ApplicationCore;
using ApplicationCore.DataServices;

namespace RoamAreasPlugin
{
    [AutoRegister]
    public class RoamAreasDataService : DataServiceBase, IModel3DProvider
    {
        private readonly MySqlConnection _connection = null;
        private EQEmuDisplay3D.RoamAreasDisplay3D _display3d = null;
        private EQEmu.RoamAreas.ZoneRoamAreas _zoneAreas = null;

        public RoamAreasDataService([Dependency] MySqlConnection connection)
        {
            _connection = connection;
        }

        private string _zone;
        public string Zone
        {
            get { return _zone; }
            set
            {
                if (_connection != null)
                {
                    ZoneAreas = new EQEmu.RoamAreas.ZoneRoamAreas(_connection, value, TypeQueryConfig);                    
                    _zone = value;
                }
                else
                {
                    _zone = "";
                }
                NotifyPropertyChanged("Zone");
            }
        }

        public EQEmu.RoamAreas.ZoneRoamAreas ZoneAreas
        {
            get { return _zoneAreas; }
            set
            {
                _zoneAreas = value;
                _display3d = new EQEmuDisplay3D.RoamAreasDisplay3D(value);

                if (_viewClipping != null)
                {
                    _display3d.Clipping = _viewClipping;
                }

                Model3D = new ModelVisual3D()
                {
                    Content = _display3d.Model,
                    Transform = Transform3D
                };

                NotifyPropertyChanged("ZoneAreas");
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

        protected void OnModelChanged()
        {
            var e = ModelChanged;
            if (e != null)
            {
                e(this, new EventArgs());
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

        public event Model3DChangedHandler ModelChanged;

        private EQEmu.RoamAreas.RoamArea _selectedArea = null;
        public EQEmu.RoamAreas.RoamArea SelectedArea
        {
            get
            {
                return _selectedArea;
            }

            set
            {
                _selectedArea = value;
                if (_selectedArea == null) return;

                _modelVisual.Children.Remove(_cursor);
                if (_display3d != null)
                {
                    _display3d.ShowArea(value);

                    //hack for update...
                    if (_selectedVertex != null && SelectedArea != null)
                    {
                        _cursor.Center = new Point3D(_selectedVertex.X, _selectedVertex.Y, SelectedArea.MaxZ);
                        _modelVisual.Children.Add(_cursor);
                    }
                }

                NotifyPropertyChanged("SelectedArea");
            }
        }

        private EQEmu.RoamAreas.RoamAreaEntry _selectedVertex = null;
        public EQEmu.RoamAreas.RoamAreaEntry SelectedVertex
        {
            get
            {
                return _selectedVertex;
            }
            set
            {
                _modelVisual.Children.Remove(_cursor);
                _selectedVertex = value;

                if(_selectedVertex != null )
                {
                    _cursor.Center = new Point3D(value.X,value.Y,SelectedArea.MaxZ);
                    _modelVisual.Children.Add(_cursor);
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
    }
}
