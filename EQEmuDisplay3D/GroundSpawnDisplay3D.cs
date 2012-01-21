using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using HelixToolkit;
using HelixToolkit.Wpf;

using EQEmu.GroundSpawns;

namespace EQEmuDisplay3D
{
    public class GroundSpawnDisplay3D : IDisplay3D
    {
        private Dictionary<GroundSpawn, Model3DCollection> _mapping = new Dictionary<GroundSpawn, Model3DCollection>();
        private readonly ZoneGroundSpawns _zoneSpawns;

        [Flags]
        public enum DisplayFlags
        {
            None,
            GreenAura,
            DarkGrayAura
        }

        public GroundSpawnDisplay3D(ZoneGroundSpawns zoneSpawns)
        {
            _zoneSpawns = zoneSpawns;

            foreach (var spawn in _zoneSpawns.GroundSpawns)
            {
                spawn.PositionChanged += new GroundSpawn.PositionChangedHandler(item_PositionChanged);
            }

            _zoneSpawns.GroundSpawns.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(GroundSpawns_CollectionChanged);
            ShowAllSpawns();
        }

        void GroundSpawns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<GroundSpawn>())
                    {
                        ShowSpawn(item);
                        item.PositionChanged += new GroundSpawn.PositionChangedHandler(item_PositionChanged);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<GroundSpawn>())
                    {
                        HideSpawn(item);
                        item.PositionChanged -= item_PositionChanged;
                    }
                    break;
                default:
                    break;
            }
        }

        void item_PositionChanged(object sender, PositionChangedEventArgs args)
        {
            var spawn = sender as GroundSpawn;
            if (spawn == null) return;
            ShowSpawn(spawn);
        }

        public void ShowAllSpawns()
        {
            foreach (var spawn in _zoneSpawns.GroundSpawns)
            {
                ShowSpawn(spawn);
            }
        }

        public void ShowSpawn(GroundSpawn spawn, DisplayFlags flags=DisplayFlags.None)
        {
            if (spawn == null) return;
            CreateSpawn(spawn,flags);
        }

        private void HideSpawn(GroundSpawn spawn)
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
        }

        private void CreateSpawn(GroundSpawn spawn,DisplayFlags flags)
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
                        
            Point3D p = new Point3D( 
                (spawn.MaxX + spawn.MinX) / 2,
                (spawn.MaxY + spawn.MinY) / 2,
                spawn.MaxZ);

            if (!Clipping.DrawPoint(p)) return;

            var xlen = spawn.MaxX - spawn.MinX;
            var ylen = spawn.MaxY - spawn.MinY;

            xlen = xlen <= 0 ? 4 : xlen;
            ylen = ylen <= 0 ? 4 : ylen;

            MeshBuilder builder = new MeshBuilder();
            builder.AddBox(p, ylen, xlen, 2);
            
            GeometryModel3D box = new GeometryModel3D(builder.ToMesh(), Materials.Gold);            

            collection.Add(box);            

            if ( flags != DisplayFlags.None )
            {
                var scale = 1.25;
                builder = new MeshBuilder();

                if (flags.HasFlag(DisplayFlags.DarkGrayAura))
                {
                    builder.AddBox(p, ylen * scale, xlen * scale, 1);
                    box = new GeometryModel3D(builder.ToMesh(), Materials.DarkGray);
                    collection.Add(box);
                }
                else if (flags.HasFlag(DisplayFlags.GreenAura))
                {
                    builder.AddBox(p, ylen * scale, xlen * scale, 1);
                    box = new GeometryModel3D(builder.ToMesh(), Materials.Green);
                    collection.Add(box);
                }
            }

            _mapping[spawn] = collection;


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
                if (_clipping != null)
                {
                    _clipping.OnClippingChanged -= clipping_OnClippingChanged;
                }

                _clipping = value;
                _clipping.OnClippingChanged += new ViewClipping.ClippingChangedHandler(clipping_OnClippingChanged);
            }
        }

        void clipping_OnClippingChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}
