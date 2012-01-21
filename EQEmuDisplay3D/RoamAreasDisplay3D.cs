using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using HelixToolkit;
using HelixToolkit.Wpf;

using EQEmu.RoamAreas;

namespace EQEmuDisplay3D
{
    public class RoamAreasDisplay3D : IDisplay3D
    {
        private Dictionary<RoamArea, Model3DCollection> _mapping = new Dictionary<RoamArea, Model3DCollection>();
        private readonly ZoneRoamAreas _zoneAreas;

        public RoamAreasDisplay3D(ZoneRoamAreas zoneRoamAreas)
        {
            _zoneAreas = zoneRoamAreas;
            foreach (var area in zoneRoamAreas.RoamAreas)
            {
                area.Vertices.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Vertices_CollectionChanged);
            }

            _zoneAreas.RoamAreas.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(RoamAreas_CollectionChanged);

            if (_zoneAreas.RoamAreas.Count > 0)
            {
                ShowArea(_zoneAreas.RoamAreas.First());
            }
        }

        void RoamAreas_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (RoamArea area in e.NewItems)
                    {
                        area.Vertices.CollectionChanged += Vertices_CollectionChanged;
                    }
                    return;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (RoamArea area in e.OldItems)
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
                    return;
            }
        }

        private RoamArea _activeArea = null;

        public void ShowArea(RoamArea area)
        {
            Model3DGroup group = Model as Model3DGroup;
            group.Children.Clear();

            if (area != null)
            {
                CreateArea(area);
                _activeArea = area;
            }
        }

        private void CreateArea(RoamArea area)
        {
            Model3DGroup group = Model as Model3DGroup;

            Model3DCollection collection = new Model3DCollection();
            if (_mapping.ContainsKey(area))
            {
                foreach (Model3D model in _mapping[area])
                {
                    group.Children.Remove(model);
                }
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

                Point3D next = new Point3D();
                if (vertex == area.Vertices.Last())
                {
                    next.X = area.Vertices[0].X;
                    next.Y = area.Vertices[0].Y;
                }
                else
                {
                    next.X = area.Vertices.ElementAt(area.Vertices.IndexOf(vertex) + 1).X;
                    next.Y = area.Vertices.ElementAt(area.Vertices.IndexOf(vertex) + 1).Y;
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
        public System.Windows.Media.Media3D.Model3D Model
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
