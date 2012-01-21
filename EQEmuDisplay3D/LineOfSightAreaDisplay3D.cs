using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using HelixToolkit;
using HelixToolkit.Wpf;

using EQEmu.LineOfSightAreas;

namespace EQEmuDisplay3D
{
    public class LineOfSightAreaDisplay3D : IDisplay3D
    {
        private Dictionary<LineOfSightArea, Model3DCollection> _mapping = new Dictionary<LineOfSightArea, Model3DCollection>();
        private readonly ZoneLineOfSightAreas _zoneAreas;

        private LineOfSightArea _activeArea = null;

        public LineOfSightAreaDisplay3D(ZoneLineOfSightAreas zoneAreas)
        {
            _zoneAreas = zoneAreas;

            foreach (var area in _zoneAreas.Areas)
            {
                area.Vertices.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Vertices_CollectionChanged);
            }
            _zoneAreas.Areas.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Areas_CollectionChanged);

            if (_zoneAreas.Areas.Count > 0)
            {
                ShowArea(_zoneAreas.Areas.First());                
            }
        }

        void Areas_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (LineOfSightArea area in e.NewItems)
                    {
                        area.Vertices.CollectionChanged += Vertices_CollectionChanged;                       
                    }
                    return;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (LineOfSightArea area in e.OldItems)
                    {
                        area.Vertices.CollectionChanged -= Vertices_CollectionChanged;
                    }
                    return;
            }
        }

        void Vertices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:                
                    ShowArea(_activeArea);                    
                    break;
                default:
                    break;
            }
        }

        public void ShowArea(LineOfSightArea area)
        {
            Model3DGroup group = Model as Model3DGroup;
            group.Children.Clear();

            if (area != null)
            {
                CreateArea(area);
                _activeArea = area;
            }

            //if (area == null)
            //{
            //    //UpdateAll();
            //    return;
            //}
            //else
            //{
            //    Model3DGroup group = Model as Model3DGroup;
            //    group.Children.Clear();
            //    CreateArea(area);
            //    _activeArea = area;                
            //}
        }

        private void CreateArea(LineOfSightArea area)
        {
            Model3DGroup group = Model as Model3DGroup;

            Model3DCollection collection = new Model3DCollection();
            try
            {
                if (_mapping[area] != null)
                {
                    foreach (Model3D model in _mapping[area])
                    {
                        group.Children.Remove(model);
                    }
                }
            }
            catch (KeyNotFoundException)
            {
                //nothing needs to be done
            }

            foreach (var vertex in area.Vertices)
            {
                MeshBuilder builder = new MeshBuilder();
                Point3D p1 = new Point3D(vertex.X, vertex.Y, area.MaxZ);
                Point3D p2 = new Point3D(vertex.X, vertex.Y, area.MinZ);

                //if (Clipping != null && !Clipping.DrawPoint(p)) continue;

                builder.AddBox(p1, 4, 4, 4);
                builder.AddBox(p2, 4, 4, 4);
                builder.AddPipe(p1, p2, 1.5, 1.5, 50);

                GeometryModel3D box = new GeometryModel3D(builder.ToMesh(), Materials.Yellow);
                collection.Add(box);

                builder = new MeshBuilder();

                Point3D next;
                if (vertex == area.Vertices.Last())
                {
                    next = area.Vertices[0];
                }
                else
                {
                    next = area.Vertices.ElementAt(area.Vertices.IndexOf(vertex) + 1);
                }
                Point3D n1 = new Point3D(next.X, next.Y, area.MaxZ);
                Point3D n2 = new Point3D(next.X, next.Y, area.MinZ);

                //builder.AddPipe(p1, p2);
                builder.AddPipe(p1, n1, 0.5, 0.5, 50);
                builder.AddPipe(p2, n2, 0.5, 0.5, 50);
                collection.Add(new GeometryModel3D(builder.ToMesh(), Materials.Yellow));                

            }

            _mapping[area] = collection;
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
