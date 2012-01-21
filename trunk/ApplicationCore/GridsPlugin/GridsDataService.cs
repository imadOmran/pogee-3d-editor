using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using Microsoft.Practices.Unity;

using MySql.Data.MySqlClient;

using HelixToolkit;
using HelixToolkit.Wpf;

using ApplicationCore;
using ApplicationCore.DataServices;

namespace GridsPlugin
{
    [AutoRegister]
    public class GridsDataService : DataServiceBase, IModel3DProvider
    {
        private readonly MySqlConnection _connection = null;

        public GridsDataService(MySqlConnection connection)
            : base()
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
                    ZoneGrids = new EQEmu.Grids.ZoneGrids(_connection, value,TypeQueryConfig);
                    _zone = value;
                }
                else
                {
                    _zone = "";
                }                
                NotifyPropertyChanged("Zone");
            }
        }

        private EQEmuDisplay3D.GridsDisplay3D _grids3d = null;
        private EQEmu.Grids.ZoneGrids _zoneGrids = null;
        public EQEmu.Grids.ZoneGrids ZoneGrids
        {
            get { return _zoneGrids; }
            set
            {
                _zoneGrids = value;

                _grids3d = new EQEmuDisplay3D.GridsDisplay3D(value);

                if (_viewClipping != null)
                {
                    _grids3d.Clipping = _viewClipping;
                }

                Model3D = new ModelVisual3D()
                {
                    Content = _grids3d.Model,
                    Transform = Transform3D
                };

                NotifyPropertyChanged("ZoneGrids");
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
        protected void OnModelChanged()
        {
            if (ModelChanged != null)
            {
                ModelChanged(this, new EventArgs());
            }
        }

        private EQEmu.Grids.Grid _selectedGrid = null;
        public EQEmu.Grids.Grid SelectedGrid
        {
            get { return _selectedGrid; }
            set
            {
                _selectedGrid = value;

                if (_grids3d != null)
                {
                    _grids3d.ShowGrid(value);
                    SelectedWaypoint = null;
                }

                NotifyPropertyChanged("SelectedGrid");
            }
        }

        private BoxVisual3D _cursor = new BoxVisual3D()
        {
            Width = 5,
            Length = 5,
            Height = 0.1,
            Material = Materials.Green
        };

        private EQEmu.Grids.Waypoint _selectedWaypoint = null;
        public EQEmu.Grids.Waypoint SelectedWaypoint
        {
            get { return _selectedWaypoint; }
            set
            {
                _modelVisual.Children.Remove(_cursor);
                _selectedWaypoint = value;

                if (_selectedWaypoint != null)
                {
                    _cursor.Center = new Point3D(_selectedWaypoint.X, _selectedWaypoint.Y, _selectedWaypoint.Z);
                    _modelVisual.Children.Add(_cursor);
                }
                NotifyPropertyChanged("SelectedWaypoint");
            }
        }

    }
}
