using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using HelixToolkit;
using HelixToolkit.Wpf;

using EQEmu.Zone;

namespace EQEmuDisplay3D
{
    public class ZonePointsDisplay3D
    {
        private Dictionary<ZonePoint, Model3DCollection> _mapping = new Dictionary<ZonePoint, Model3DCollection>();
        private readonly ZonePoints _zonePoints;

        [Flags]
        public enum DisplayFlags
        {
            None,
            GreenAura,
            DarkGrayAura
        }

        public ZonePointsDisplay3D(ZonePoints zonePoints)
        {
            _zonePoints = zonePoints;

            //initial hookup 
            //points added/removed will need to attach/unattach handlers
            foreach (var pt in _zonePoints.Points)
            {
                pt.ObjectDirtied += new EQEmu.Database.ObjectDirtiedHandler(pt_ObjectDirtied);
            }

            _zonePoints.Points.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Points_CollectionChanged);
            ShowAllPoints();
        }

        void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<ZonePoint>())
                    {
                        ShowPoint(item);
                        item.ObjectDirtied += new EQEmu.Database.ObjectDirtiedHandler(pt_ObjectDirtied);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<ZonePoint>())
                    {
                        HidePoint(item);
                        item.ObjectDirtied -= pt_ObjectDirtied;
                    }
                    break;
                default:
                    break;
            }           
        }

        void pt_ObjectDirtied(object sender, EventArgs args)
        {
            var zp = sender as ZonePoint;
            if (zp != null)
            {
                ShowPoint(zp);
            }
        }

        public void ShowAllPoints()
        {
            foreach (var pt in _zonePoints.Points)
            {
                ShowPoint(pt);
            }
        }

        public void ShowPoint(ZonePoint pt, DisplayFlags flags=DisplayFlags.None)
        {
            if (pt == null) return;
            CreatePoint(pt,flags);
        }

        private void HidePoint(ZonePoint pt)
        {
            Model3DGroup group = Model as Model3DGroup;
            Model3DCollection collection = new Model3DCollection();

            if (_mapping.ContainsKey(pt))
            {
                foreach (Model3D model in _mapping[pt])
                {
                    group.Children.Remove(model);
                }
            }       
        }

        private void CreatePoint(ZonePoint zp,DisplayFlags flags)
        {
            Model3DGroup group = Model as Model3DGroup;
            Model3DCollection collection = new Model3DCollection();

            if (_mapping.ContainsKey(zp))
            {
                foreach (Model3D model in _mapping[zp])
                {
                    group.Children.Remove(model);
                }
            }

            double px, py, pz;
            px = zp.X == 999999 ? 0 : zp.X;
            py = zp.Y == 999999 ? 0 : zp.Y;
            pz = zp.Z == 999999 ? 0 : zp.Z;

            Point3D p = new Point3D(px,py,pz);

            if (!Clipping.DrawPoint(p)) return;
            
            MeshBuilder builder = new MeshBuilder();
            builder.AddBox(p, 20, 20, 2);
            
            //connect box to destination            
            px = zp.TargetX == 999999 ? px : zp.TargetX;
            py = zp.TargetY == 999999 ? py : zp.TargetY;
            pz = zp.TargetZ == 999999 ? pz : zp.TargetZ;

            GeometryModel3D box;
            box = new GeometryModel3D(builder.ToMesh(), Materials.Red);
            collection.Add(box);

            builder = new MeshBuilder();
            Point3D destP = new Point3D(px, py, pz);
            builder.AddArrow(p, destP, 0.5);
            builder.AddBox(destP, 20, 20, 2);

            if (zp.X == 999999 || zp.Y == 999999 || zp.Z == 999999 ||
                zp.TargetX == 999999 || zp.TargetY == 999999 || zp.TargetZ == 999999)
            {
                box = new GeometryModel3D(builder.ToMesh(), Materials.Gold);
            }
            else
            {
                box = new GeometryModel3D(builder.ToMesh(), Materials.White);
            }

            collection.Add(box);

            if (flags != DisplayFlags.None)
            {
                builder = new MeshBuilder();

                if (flags.HasFlag(DisplayFlags.DarkGrayAura))
                {
                    builder.AddBox(p, 25,25, 1);
                    builder.AddBox(destP, 25, 25, 1);
                    box = new GeometryModel3D(builder.ToMesh(), Materials.DarkGray);
                    collection.Add(box);
                }
                else if (flags.HasFlag(DisplayFlags.GreenAura))
                {
                    builder.AddBox(p, 25, 25, 1);
                    builder.AddBox(destP, 25, 25, 1);
                    box = new GeometryModel3D(builder.ToMesh(), Materials.Green);
                    collection.Add(box);
                }
            }

            _mapping[zp] = collection;


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
