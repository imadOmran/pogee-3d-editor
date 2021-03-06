﻿using System;
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

namespace ZonePointsPlugin
{
    [AutoRegister]
    public class ZonePointsDataService : DataServiceBase, IModel3DProvider
    {
        private readonly MySqlConnection _connection = null;
        private string _zone;
        private EQEmu.Zone.ZonePoints _zonePoints;
        private EQEmuDisplay3D.ZonePointsDisplay3D _zp3d;

        public ZonePointsDataService(MySqlConnection conn)
        {
            _connection = conn;
        }

        private EQEmu.Zone.ZonePoint _selectedZonePoint = null;
        public EQEmu.Zone.ZonePoint SelectedZonePoint
        {
            get { return _selectedZonePoint; }
            set
            {
                if (_zp3d != null && value != null)
                {
                    if (_selectedZonePoint != null)
                    {
                        //display the previously selected point without a selection indicator
                        _zp3d.ShowPoint(_selectedZonePoint);
                    }
                    _zp3d.ShowPoint(value, EQEmuDisplay3D.ZonePointsDisplay3D.DisplayFlags.GreenAura);
                }
                _selectedZonePoint = value;
            }
        }

        public EQEmu.Zone.ZonePoints ZonePoints
        {
            get { return _zonePoints; }
            set
            {
                if (_zonePoints != null)
                {
                    _zonePoints.DataLoaded -= _zonePoints_DataLoaded;
                }

                _zonePoints = value;
                _zonePoints.DataLoaded += new EQEmu.Zone.ZonePointDataLoadedHandler(_zonePoints_DataLoaded);

                _zp3d = new EQEmuDisplay3D.ZonePointsDisplay3D(value);                

                if (_viewClipping != null)
                {
                    _zp3d.Clipping = _viewClipping;
                }

                Model3D = new ModelVisual3D()
                {
                    Content = _zp3d.Model,
                    Transform = Transform3D
                };

                NotifyPropertyChanged("ZonePoints");
            }
        }

        void _zonePoints_DataLoaded(object sender, EQEmu.Zone.ZonePointDataLoadedEventArgs e)
        {
            _zone = e.ZoneName;
            NotifyPropertyChanged("Zone");
        }

        public string Zone
        {
            get { return _zone; }
            set
            {
                _zone = value;
                if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
                {
                    ZonePoints = new EQEmu.Zone.ZonePointsDatabase(value, _connection, TypeQueryConfig);
                }
                else
                {
                    ZonePoints = new EQEmu.Zone.ZonePointsLocal(value, TypeQueryConfig);
                }
                NotifyPropertyChanged("Zone");
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

        [Dependency("WorldTransform")]
        public Transform3D Transform3D
        {
            get;
            set;
        }

        private EQEmuDisplay3D.ViewClipping _viewClipping;
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

        public event Model3DChangedHandler ModelChanged;
        protected void OnModelChanged()
        {
            if (ModelChanged != null)
            {
                ModelChanged(this, new EventArgs());
            }
        }
    }
}
