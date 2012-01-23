using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media.Media3D;

using HelixToolkit;
using HelixToolkit.Wpf;

using EQEmu.Grids;

namespace EQEmuDisplay3D
{
    public class GridsDisplay3D : IDisplay3D
    {
        private Dictionary<EQEmu.Grids.Grid, Model3DCollection> _mapping = new Dictionary<EQEmu.Grids.Grid, Model3DCollection>();
        private readonly EQEmu.Grids.ZoneGrids _zoneGrid;

        private EQEmu.Grids.Grid _activeGrid = null;

        public GridsDisplay3D(EQEmu.Grids.ZoneGrids zonegrid)
        {
            _zoneGrid = zonegrid;

            if (_zoneGrid.Grids.Count > 0)
            {
                ShowGrid(_zoneGrid.Grids.ElementAt(0));
            }

            foreach ( Grid grid in _zoneGrid.Grids ) {
                grid.Waypoints.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler( Waypoints_CollectionChanged );
            }
            _zoneGrid.Grids.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler( Grids_CollectionChanged );

            //UpdateAll();
        }

        void Grids_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            switch ( e.Action ) {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach ( EQEmu.Grids.Grid old in e.OldItems ) {
                        old.Waypoints.CollectionChanged -= Waypoints_CollectionChanged;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (EQEmu.Grids.Grid @new in e.NewItems.Cast<EQEmu.Grids.Grid>())
                    {
                        @new.Waypoints.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Waypoints_CollectionChanged);
                    }
                    break;
                default:
                    break;
            }
        }

        void Waypoints_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            switch ( e.Action ) {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    ShowGrid( _activeGrid );
                    break;
                default:
                    break;
            }
        }
        
        public void UpdateAll()
        {
            foreach (Grid grid in _zoneGrid.Grids)
            {
                CreateGrid(grid);
            }
        }

        public void ShowGrid(Grid grid)
        {
            if (grid == null)
            {
                //UpdateAll();
                return;
            }
            else
            {
                Model3DGroup group = Model as Model3DGroup;
                group.Children.Clear();
                CreateGrid(grid);
                _activeGrid = grid;
            }
        }

        private void CreateGrid(Grid grid)
        {
            Model3DGroup group = Model as Model3DGroup;
            

            Model3DCollection collection = new Model3DCollection();

            if (_mapping.ContainsKey(grid))
            {
                foreach (Model3D model in _mapping[grid])
                {
                    group.Children.Remove(model);
                }
            }

            foreach (Waypoint wp in grid.Waypoints)
            {
                MeshBuilder builder = new MeshBuilder();
                Point3D p = new Point3D(wp.X,wp.Y,wp.Z);

                if (Clipping != null && !Clipping.DrawPoint(p)) continue;
                builder.AddBox(p, 2, 2, 2);

                Transform3D headingRotate = new RotateTransform3D()
                {
                    CenterX = wp.X,
                    CenterY = wp.Y,
                    CenterZ = wp.Z,
                    Rotation = new AxisAngleRotation3D(
                        new Vector3D(0, 0, -1), wp.HeadingDegrees)
                };

                GeometryModel3D box;
                Material mat;
                
                if (wp.PauseTime > 0)
                {
                    mat = Materials.Red;
                }
                else
                {
                    mat = Materials.DarkGray;
                }

                box = new GeometryModel3D(builder.ToMesh(),mat);
                box.Transform = headingRotate;
                collection.Add(box);
                                
                builder = new MeshBuilder();
                float radius = 3.0f;
                double hx = wp.X + Math.Cos( (wp.HeadingDegrees-90) / 180 * Math.PI ) * radius;
                double hy = wp.Y + Math.Sin( (wp.HeadingDegrees + 90) / 180 * Math.PI ) * radius;

                builder.AddArrow(new Point3D(wp.X, wp.Y, wp.Z),new Point3D(hx, hy, wp.Z), 0.5,1);
                collection.Add(new GeometryModel3D(builder.ToMesh(), mat));
                
                //box = new GeometryModel3D(builder.ToMesh(), mat);
                //collection.Add(box);

                if(wp.Name != null && !String.IsNullOrWhiteSpace(wp.Name) )
                {
                    GeometryModel3D text = TextCreator.CreateTextLabelModel3D(wp.Name, BrushHelper.CreateGrayBrush(50), true, 2, new Point3D(p.X, p.Y, p.Z + 5), new Vector3D(1, 0, 0), new Vector3D(0, 0, 1));
                    text.Transform = new ScaleTransform3D(new Vector3D(-1, 1, 1), new Point3D(p.X, p.Y, p.Z));
                    collection.Add(text);
                }


                //GeometryModel3D text = TextCreator.CreateTextLabelModel3D(wp.GridId.ToString() + "G:" + wp.Number.ToString() + "N:" + wp.PauseTime.ToString() + "P", BrushHelper.CreateGrayBrush(5), true, 2,
                  //                                                          new Point3D(p.X, p.Y, p.Z + 5), new Vector3D(1, 0, 0), new Vector3D(0, 0, 1));
                //text.Transform = new ScaleTransform3D(new Vector3D(-1, 1, 1), new Point3D(p.X, p.Y, p.Z));
                //collection.Add(text);

                builder = new MeshBuilder();

                if (grid.WanderType == Grid.WanderTypes.Patrol || grid.WanderType == Grid.WanderTypes.Circular)
                {
                    IEnumerable<Waypoint> nextWaypointQuery = grid.Waypoints.Where(
                        x => x.Number > wp.Number).OrderBy(y => y.Number);
                    if (nextWaypointQuery.Count() > 0)
                    {
                        Waypoint nextWaypoint = nextWaypointQuery.ElementAt(0);
                        builder.AddArrow(p, new Point3D(nextWaypoint.X, nextWaypoint.Y, nextWaypoint.Z), 0.5);

                        collection.Add(
                            new GeometryModel3D(builder.ToMesh(), Materials.White));
                    }
                }

                //collection.Add( new GeometryModel3D(builder.ToMesh(), Materials.
            }

            //collection.Add(new GeometryModel3D(builder.ToMesh(), Materials.White));

            _mapping[grid] = collection;
            foreach (Model3D model in collection)
            {
                group.Children.Add(model);
            }
        }

        private Model3D _model = new Model3DGroup();
        public Model3D Model
        {
            get { return _model; }
        }

        private ViewClipping _clipping = new ViewClipping();
        public ViewClipping Clipping
        {
            get
            {
                return _clipping;
            }
            set
            {
                _clipping = value;
            }
        }
    }
}
