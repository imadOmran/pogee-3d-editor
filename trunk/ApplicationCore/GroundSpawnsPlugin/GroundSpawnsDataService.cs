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

namespace GroundSpawnsPlugin
{
        [AutoRegister]
        public class GroundSpawnsDataService : DataServiceBase, IModel3DProvider
        {
            private readonly MySqlConnection _connection = null;
            private EQEmu.GroundSpawns.ZoneGroundSpawns _zoneSpawns;
            private EQEmuDisplay3D.GroundSpawnDisplay3D _groundSpawns3d;

            public EQEmu.GroundSpawns.ZoneGroundSpawns ZoneGroundSpawns
            {
                get { return _zoneSpawns; }
                set
                {
                    _zoneSpawns = value;

                    _groundSpawns3d = new EQEmuDisplay3D.GroundSpawnDisplay3D(value);

                    if (_viewClipping != null)
                    {
                        _groundSpawns3d.Clipping = _viewClipping;
                    }

                    Model3D = new ModelVisual3D()
                    {
                        Content = _groundSpawns3d.Model,
                        Transform = Transform3D
                    };

                    NotifyPropertyChanged("ZoneGroundSpawns");
                }
            }            

            public GroundSpawnsDataService(MySqlConnection connection)
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
                        ZoneGroundSpawns = new EQEmu.GroundSpawns.ZoneGroundSpawns(_connection, value, TypeQueryConfig);

                        _selectedSpawn = null;
                        _selectedGroundSpawns = null;

                        _zone = value;
                    }
                    else
                    {
                        _zone = "";
                    }
                    NotifyPropertyChanged("Zone");
                }
            }

            public EQEmu.GroundSpawns.GroundSpawn GetClosestGroundSpawn(Point3D p)
            {
                EQEmu.GroundSpawns.GroundSpawn g = null;

                if (ZoneGroundSpawns != null)
                {
                    g = ZoneGroundSpawns.GetClosestGroundSpawn(p);
                }

                return g;
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

            private EQEmu.GroundSpawns.GroundSpawn _selectedSpawn;
            public EQEmu.GroundSpawns.GroundSpawn SelectedGroundSpawn
            {
                get { return _selectedSpawn; }
                set
                {
                    //when selectedspawn is changed - unsubscribe to position changed events from the previously selected spawn
                    if (_selectedSpawn != value && _selectedSpawn != null)
                    {
                        _selectedSpawn.PositionChanged -= _selectedSpawn_PositionChanged;
                        _groundSpawns3d.ShowSpawn(_selectedSpawn);
                    }

                    _selectedSpawn = value;

                    //subscribe to position changed events of the newly selected spawn
                    if (_selectedSpawn != null)
                    {
                        _selectedSpawn.PositionChanged += new EQEmu.GroundSpawns.GroundSpawn.PositionChangedHandler(_selectedSpawn_PositionChanged);
                    }

                    //force update on cursor graphic
                    _groundSpawns3d.ShowSpawn(_selectedSpawn, EQEmuDisplay3D.GroundSpawnDisplay3D.DisplayFlags.GreenAura);
                    //RepositionCursorOn(_selectedSpawn);

                    NotifyPropertyChanged("SelectedGroundSpawn");
                }
            }

            private IEnumerable<EQEmu.GroundSpawns.GroundSpawn> _selectedGroundSpawns = null;
            public IEnumerable<EQEmu.GroundSpawns.GroundSpawn> SelectedGroundSpawns
            {
                get { return _selectedGroundSpawns; }
                set
                {
                    if (_selectedGroundSpawns != null)
                    {
                        if (value == null)
                        {
                            //remove selection indicators from all previously selected items
                            foreach (var s in _selectedGroundSpawns.Where( x => { return x != _selectedSpawn; } ))
                            {
                                _groundSpawns3d.ShowSpawn(s);
                                s.PositionChanged -= _selectedSpawn_PositionChanged;
                            }
                        }
                        else
                        {
                            //remove selection indicators from items not in the new selections
                            foreach(var s in _selectedGroundSpawns.Where( x => { return !value.Contains(x) && x != _selectedSpawn; } ) )
                            {
                                _groundSpawns3d.ShowSpawn(s);
                                s.PositionChanged -= _selectedSpawn_PositionChanged;
                            }
                        }
                    }

                    _selectedGroundSpawns = value;
                    if (_selectedGroundSpawns != null && _selectedGroundSpawns.Count() > 0)
                    {
                        SelectedGroundSpawn = _selectedGroundSpawns.ElementAt(0);
                        for (int i = 1; i < _selectedGroundSpawns.Count(); i++)
                        {
                            _groundSpawns3d.ShowSpawn(_selectedGroundSpawns.ElementAt(i), EQEmuDisplay3D.GroundSpawnDisplay3D.DisplayFlags.DarkGrayAura);
                            _selectedGroundSpawns.ElementAt(i).PositionChanged += new EQEmu.GroundSpawns.GroundSpawn.PositionChangedHandler(_selectedSpawn_PositionChanged);
                        }
                    }                    
                    NotifyPropertyChanged("SelectedGroundSpawns");
                }
            }

            void _selectedSpawn_PositionChanged(object sender, EQEmu.GroundSpawns.PositionChangedEventArgs args)
            {
                //when the spawn's position changes the cursor position needs to be updated
                var spawn = sender as EQEmu.GroundSpawns.GroundSpawn;
                if (spawn != null)
                {
                    if (spawn == _selectedSpawn)
                    {
                        _groundSpawns3d.ShowSpawn(_selectedSpawn, EQEmuDisplay3D.GroundSpawnDisplay3D.DisplayFlags.GreenAura);
                    }
                    else
                    {
                        _groundSpawns3d.ShowSpawn(spawn, EQEmuDisplay3D.GroundSpawnDisplay3D.DisplayFlags.DarkGrayAura);
                    }
                }
            }

            //private void RepositionSelectionCursors()
            //{
            //    foreach (var item in _cursors)
            //    {
            //        _modelVisual.Children.Remove(item);
            //    }
            //    if (_selectedSpawn != null)
            //    {
            //        _cursors.Add(new BoxVisual3D()
            //        {
            //            Width = 6 + _selectedSpawn.MaxX - _selectedSpawn.MinX,
            //            Length = 6 + _selectedSpawn.MaxY - _selectedSpawn.MinY,
            //            Height = 1,
            //            Material = Materials.Green
            //        });

            //        if (_selectedGroundSpawns != null)
            //        {
            //            foreach (var spawn in _selectedGroundSpawns.Where(x => { return x != _selectedSpawn; }))
            //            {
            //                _cursors.Add(new BoxVisual3D()
            //                {
            //                    Width = 6 + spawn.MaxX - spawn.MinX,
            //                    Length = 6 + spawn.MaxY - spawn.MinY,
            //                    Height = 1,
            //                    Material = Materials.Blue
            //                });
            //            }
            //        }
            //    }

            //    foreach (var cursor in _cursors)
            //    {
            //        _modelVisual.Children.Add(cursor);
            //    }
            //}

            //private void PositionMultipleSelectionIndicators()
            //{
            //    foreach (var cursor in _cursors)
            //    {
            //        _modelVisual.Children.Remove(cursor);
            //    }
            //    _cursors.Clear();

            //    if (_selectedGroundSpawns != null)
            //    {
            //        foreach (var sp in _selectedGroundSpawns.Where(x => { return x != SelectedGroundSpawn; }))
            //        {
            //            _cursors.Add(new BoxVisual3D()
            //            {
            //                Width = 6 + sp.MaxX - sp.MinX,
            //                Length = 6 + sp.MaxY - sp.MinY,
            //                Height = 1,
            //                Material = Materials.DarkGray,
            //                Center = sp.MidPoint

            //            });
            //        }
            //    }

            //    //foreach (var cursor in _cursors)
            //    //{
            //    //    _modelVisual.Children.Add(cursor);
            //    //}
            //}

            //private void RepositionCursorOn(EQEmu.GroundSpawns.GroundSpawn spawn)
            //{
            //    _modelVisual.Children.Remove(_cursor);

            //    if (spawn != null)
            //    {
            //        _cursor.Width = 6 + spawn.MaxX - spawn.MinX;
            //        _cursor.Length = 6 + spawn.MaxY - spawn.MinY;
            //        _cursor.Height = 1;
            //        _cursor.Material = Materials.Green;
            //        _cursor.Center = spawn.MidPoint;

            //        _modelVisual.Children.Add(_cursor);
            //    }
            //}

            protected void OnModelChanged()
            {
                if (ModelChanged != null)
                {
                    ModelChanged(this, new EventArgs() );
                }
            }
            public event Model3DChangedHandler ModelChanged;

            //private BoxVisual3D _cursor = new BoxVisual3D();
            //private List<BoxVisual3D> _cursors = new List<BoxVisual3D>();
        }
}
