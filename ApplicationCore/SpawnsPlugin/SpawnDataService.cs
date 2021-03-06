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
using GridsPlugin;

namespace SpawnsPlugin
{
    [AutoRegister]
    public class SpawnDataService : DataServiceBase, IModel3DProvider
    {
        private readonly MySqlConnection _connection = null;
        public SpawnDataService([Dependency] MySqlConnection connection)
            : base()
        {
            _connection = connection;
        }

        private int _version = 0;
        public int Version
        {
            get { return _version; }
            set
            {
                _version = value;
            }
        }

        private string _zone;
        public string Zone
        {
            get { return _zone; }
            set
            {
                if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
                {
                    var zps = new EQEmu.Spawns.ZoneSpawnsDatabase(_connection, TypeQueryConfig);
                    ZoneSpawns = zps;
                    zps.Lookup(value, _version);
                }
                else
                {
                    ZoneSpawns = new EQEmu.Spawns.ZoneSpawnsLocal(TypeQueryConfig);
                    ZoneSpawns.Zone = value;
                    ZoneSpawns.Version = _version;
                }

                _zone = value;
                NotifyPropertyChanged("Zone");
            }
        }

        public EQEmu.Spawns.ZoneSpawns CreateNewZone(string zone,int version,bool local = false)
        {
            if (local)
            {
                ZoneSpawns = new EQEmu.Spawns.ZoneSpawnsLocal(TypeQueryConfig);
                ZoneSpawns.Zone = zone;
                ZoneSpawns.Version = _version;
            }
            else
            {
                var zps = new EQEmu.Spawns.ZoneSpawnsDatabase(_connection, TypeQueryConfig);
                ZoneSpawns = zps;
                zps.Lookup(zone, _version);
            }

            _zone = zone;           
            NotifyPropertyChanged("Zone");

            return ZoneSpawns;
        }

        private EQEmuDisplay3D.Spawn2Display3D _spawn3d = null;
        private EQEmu.Spawns.ZoneSpawns _zoneSpawns = null;
        public EQEmu.Spawns.ZoneSpawns ZoneSpawns
        {
            get { return _zoneSpawns; }
            set
            {
                if (_zoneSpawns != null)
                {
                    _zoneSpawns.DataLoaded -= _zoneSpawns_DataLoaded;
                }

                _zoneSpawns = value;
                _zoneSpawns.DataLoaded += new EQEmu.Spawns.SpawnsDataLoadedHandler(_zoneSpawns_DataLoaded);


                if (_spawn3d != null)
                {
                    _spawn3d.Dispose();
                }

                _spawn3d = new EQEmuDisplay3D.Spawn2Display3D(_zoneSpawns);

                if (_viewClipping != null)
                {
                    _spawn3d.Clipping = _viewClipping;
                }

                Model3D = new ModelVisual3D()
                {
                    Content = _spawn3d.Model,
                    Transform = Transform3D
                };

                NotifyPropertyChanged("ZoneSpawns");
            }
        }

        void _zoneSpawns_DataLoaded(object sender, EQEmu.Spawns.SpawnsLoadedEventArgs e)
        {
            _zone = e.ZoneName;
            _version = e.Version;

            NotifyPropertyChanged("Zone");
            NotifyPropertyChanged("Version");
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

        //private BoxVisual3D _cursor = new BoxVisual3D()
        //{
        //    Width = 5,
        //    Length = 5,
        //    Height = 0.1,
        //    Material = Materials.Green
        //};

        private IEnumerable<EQEmu.Spawns.Spawn2> _selectedSpawns = null;
        public IEnumerable<EQEmu.Spawns.Spawn2> SelectedSpawns
        {
            get { return _selectedSpawns; }
            set
            {
                if (_selectedSpawns != null)
                {
                    foreach (var s in _selectedSpawns.Where( x => { return x != _selectedSpawn; }) )
                    {
                        _spawn3d.ShowSpawn(s, EQEmuDisplay3D.Spawn2Display3D.DisplayFlags.None);
                    }
                }

                _selectedSpawns = value;

                if (_selectedSpawns != null)
                {
                    foreach (var s in _selectedSpawns.Where(x => { return x != _selectedSpawn; }))
                    {
                        _spawn3d.ShowSpawn(s, EQEmuDisplay3D.Spawn2Display3D.DisplayFlags.Blue);
                    }
                }

                NotifyPropertyChanged("SelectedSpawns");
            }
        }

        private EQEmu.Spawns.Spawn2 _selectedSpawn = null;
        public EQEmu.Spawns.Spawn2 SelectedSpawn
        {
            get { return _selectedSpawn; }
            set
            {
                //_modelVisual.Children.Remove(_cursor);

                if (_selectedSpawn != null && _selectedSpawn != value)
                {
                    _spawn3d.ShowSpawn(_selectedSpawn, EQEmuDisplay3D.Spawn2Display3D.DisplayFlags.None);
                }

                _selectedSpawn = value;
                if (_selectedSpawn != null)
                {
                    //_cursor.Center = new Point3D(_selectedSpawn.X, _selectedSpawn.Y, _selectedSpawn.Z);
                    //_modelVisual.Children.Add(_cursor);

                    // TODO visual should update automatically
                    _spawn3d.ShowSpawn(_selectedSpawn, EQEmuDisplay3D.Spawn2Display3D.DisplayFlags.Green);


                    //bring this spawns grid into view
                    if (GridsService != null && GridsService.ZoneGrids != null)
                    {
                        IEnumerable<EQEmu.Grids.Grid> grid =
                            GridsService.ZoneGrids.Grids.Where(x => x.Id == _selectedSpawn.GridId).Distinct();
                        if (grid.Count() > 0)
                        {
                            GridsService.SelectedGrid = grid.ElementAt(0);
                        }
                    }
                }

                NotifyPropertyChanged("SelectedSpawn");
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

        [OptionalDependency( "GridsDataService" )]
        public GridsDataService GridsService
        {
            get;
            set;
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
