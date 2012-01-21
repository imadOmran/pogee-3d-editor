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
    public class Spawn2Display3D : IDisplay3D
    {
        private Dictionary<EQEmu.Spawns.Spawn2, Model3DCollection> _mapping = new Dictionary<EQEmu.Spawns.Spawn2, Model3DCollection>();
        private readonly EQEmu.Spawns.ZoneSpawns _zoneSpawns;

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
                if (_clipping != null)
                {
                    _clipping.OnClippingChanged -= clipping_OnClippingChanged;
                }
                
                _clipping = value;
                _clipping.OnClippingChanged += new ViewClipping.ClippingChangedHandler(clipping_OnClippingChanged);
                ShowAllSpawns();
            }
        }

        void clipping_OnClippingChanged(object sender, EventArgs e)
        {
            ShowAllSpawns();
        }

        public void ShowAllSpawns()
        {
            foreach (EQEmu.Spawns.Spawn2 spawn in _zoneSpawns.Spawns)
            {
                ShowSpawn(spawn);
            }
        }

        public Spawn2Display3D( EQEmu.Spawns.ZoneSpawns zoneSpawns)
        {
            _zoneSpawns = zoneSpawns;
            _zoneSpawns.Spawns.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Spawns_CollectionChanged);
            ShowAllSpawns();
        }

        void Spawns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (EQEmu.Spawns.Spawn2 spawn in e.OldItems)
                    {
                        RemoveSpawn(spawn);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (EQEmu.Spawns.Spawn2 spawn in e.NewItems)
                    {
                        ShowSpawn(spawn);
                    }
                    break;
            }
        }

        public void ShowSpawn(EQEmu.Spawns.Spawn2 spawn)
        {
            if (spawn == null)
            {
                //UpdateAll();
                return;
            }
            else
            {
                //Model3DGroup group = Model as Model3DGroup;
                //group.Children.Clear();
                CreateSpawn(spawn);
            }
        }

        public void RemoveSpawn(EQEmu.Spawns.Spawn2 spawn)
        {
            Model3DGroup group = Model as Model3DGroup;            

            foreach (Model3D model in _mapping[spawn])
            {
                group.Children.Remove(model);
            }

            _mapping[spawn] = null;
        }

        private void CreateSpawn(EQEmu.Spawns.Spawn2 spawn)
        {
            Model3DGroup group = Model as Model3DGroup;


            Model3DCollection collection = new Model3DCollection();

            if (_mapping.ContainsKey(spawn))
            {
                foreach (Model3D model in _mapping[spawn])
                {
                    group.Children.Remove(model);
                }
            }

            //try
            //{
            //    if (_mapping[spawn] != null)
            //    {
            //        foreach (Model3D model in _mapping[spawn])
            //        {
            //            group.Children.Remove(model);
            //        }
            //    }
            //}
            //catch (KeyNotFoundException)
            //{
            //    //nothing needs to be done
            //}

            MeshBuilder builder = new MeshBuilder();
            Point3D p = new Point3D(spawn.X,spawn.Y,spawn.Z);

            if( !Clipping.DrawPoint(p) ) return;

            builder.AddBox(p,2,2,2);

            Transform3D headingRotate = new RotateTransform3D()
            {
                CenterX = p.X,
                CenterY = p.Y,
                CenterZ = p.Z,
                Rotation = new AxisAngleRotation3D(
                    new Vector3D(0,0,-1),spawn.HeadingDegrees)
            };

            GeometryModel3D box;
            Material mat;

            if(spawn.RoamAreaId > 0)
            {
                mat = Materials.Red;
            }
            else if ( spawn.GridId > 0 )
            {
                mat = Materials.Yellow;
            }
            else
            {
                mat = Materials.White;
            }
            
            box = new GeometryModel3D(builder.ToMesh(), mat);
            box.Transform = headingRotate;
            collection.Add(box);

            builder = new MeshBuilder();
            float radius = 3.0f;
            double hx = spawn.X + Math.Cos((spawn.HeadingDegrees - 90) / 180 * Math.PI) * radius;
            double hy = spawn.Y + Math.Sin((spawn.HeadingDegrees + 90) / 180 * Math.PI) * radius;
            
            builder.AddArrow(new Point3D(spawn.X, spawn.Y, spawn.Z), new Point3D(hx, hy, spawn.Z), 0.5, 1);            
            collection.Add(new GeometryModel3D(builder.ToMesh(), mat));

            _mapping[spawn] = collection;
            foreach (Model3D model in collection)
            {
                group.Children.Add(model);
            }
        }
    }
}
