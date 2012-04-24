using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;

using HelixToolkit;
using HelixToolkit.Wpf;

using EQEmu.Doors;
using EQEmu.Files.WLD;
using EQEmu.Files.WLD.Fragments;

namespace EQEmuDisplay3D
{
    public class PolygonDoorRender
    {
        private IEnumerable<EQEmu.Files.WLD.Polygon> _polys = null;
        private Door _door;

        public PolygonDoorRender(IEnumerable<EQEmu.Files.WLD.Polygon> polys, Door door)
        {
            _polys = polys;
            _door = door;
        }

        public IEnumerable<EQEmu.Files.WLD.Polygon> Polygons
        {
            get { return _polys; }
        }

        public List<Transform3D> GetTransforms()
        {
            var transforms = new List<Transform3D>();

            RotateTransform3D heading = new RotateTransform3D();
            heading.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), Door.Heading / 512 * 360);

            TranslateTransform3D translate = new TranslateTransform3D();
            translate.OffsetX = Door.X;
            translate.OffsetY = Door.Y;
            translate.OffsetZ = Door.Z;

            transforms.Add(heading);
            transforms.Add(translate);

            return transforms;
        }

        public Door Door
        {
            get { return _door; }
        }
    }

    public class DoorsDisplay3D : IDisplay3D, IDisposable
    {
        private readonly Model3D _group = new Model3DGroup();
        private ViewClipping _clipping = null;
        private DoorManager _manager;
        private WLD _objWld = null;

        public DoorsDisplay3D(DoorManager manager)
        {
            _manager = manager;
            UpdateAll();
        }

        public WLD ObjectsWLD
        {
            get { return _objWld; }
            set
            {
                _objWld = value;
                UpdateAll();
            }
        }

        public Model3D Model
        {
            get { return _group; }
        }

        private IEnumerable<PolygonDoorRender> _doorObjects = null;

        private void BuildDoors()
        {   
            string rem = "_DMSPRITEDEF";
            if (_objWld != null)
            {
                var doorobjs = new List<PolygonDoorRender>();
                foreach (var door in _manager.Doors)
                {
                    var mesh = _objWld.ZoneMeshes.FirstOrDefault(x => x.FragmentName.Replace(rem, "") == door.Name);                    
                    if (mesh != null)
                    {
                        doorobjs.Add(new PolygonDoorRender(mesh.Polygons, door));
                    }
                }
                _doorObjects = doorobjs;
            }
        }

        public void UpdateAll()
        {            
            Model3DGroup group = Model as Model3DGroup;
            Material mat = Materials.Red;

            BuildDoors();

            group.Children.Clear();
                        
            if (_doorObjects != null)
            {
                foreach (var dr in _doorObjects)
                {
                    var builder = new MeshBuilder();
                    var transforms = dr.GetTransforms();
                    if (dr.Polygons.ElementAt(0).Image != null)
                    {
                        var img = dr.Polygons.ElementAt(0).Image;
                        var brush = new System.Windows.Media.ImageBrush(img);
                        brush.ViewportUnits = System.Windows.Media.BrushMappingMode.Absolute;
                        brush.TileMode = System.Windows.Media.TileMode.Tile;
                        mat = HelixToolkit.Wpf.MaterialHelper.CreateMaterial(brush);
                    }
                    else
                    {
                        mat = Materials.LightGray;
                    }

                    foreach (var poly in dr.Polygons)
                    {
                        Point3D p1 = new Point3D(poly.V1.X, poly.V1.Y, poly.V1.Z);
                        Point3D p2 = new Point3D(poly.V2.X, poly.V2.Y, poly.V2.Z);
                        Point3D p3 = new Point3D(poly.V3.X, poly.V3.Y, poly.V3.Z);

                        foreach (var transform in transforms)
                        {
                            p1 = transform.Transform(p1);
                            p2 = transform.Transform(p2);
                            p3 = transform.Transform(p3);
                        }

                        if (!Clipping.DrawPoint(p1) || !Clipping.DrawPoint(p2) || !Clipping.DrawPoint(p3))
                        {
                            continue;
                        }

                        var t1 = new System.Windows.Point(poly.V1.U, 1 - poly.V1.V);
                        var t2 = new System.Windows.Point(poly.V2.U, 1 - poly.V2.V);
                        var t3 = new System.Windows.Point(poly.V3.U, 1 - poly.V3.V);
                        builder.AddTriangle(p3, p2, p1, t3, t2, t1);
                    }
                    group.Children.Add(new GeometryModel3D(builder.ToMesh(), mat));
                }
            }
            else
            {
                var builder = new MeshBuilder();
                foreach (var door in _manager.Doors)
                {
                    builder.AddBox(new Point3D(door.X, door.Y, door.Z), 5, 5, 5);
                }
                group.Children.Add(new GeometryModel3D(builder.ToMesh(), mat));
            }            
        }

        public ViewClipping Clipping
        {
            get { return _clipping; }
            set
            {
                if (_clipping != null)
                {
                    _clipping.OnClippingChanged -= clipping_OnClippingChanged;
                }
                _clipping = value;
                _clipping.OnClippingChanged += new ViewClipping.ClippingChangedHandler(clipping_OnClippingChanged);
                UpdateAll();
            }
        }

        void clipping_OnClippingChanged(object sender, EventArgs e)
        {
            UpdateAll();
        }

        public void Dispose()
        {
            if (_clipping != null)
            {
                _clipping.OnClippingChanged -= clipping_OnClippingChanged;
            }
        }
    }
}
