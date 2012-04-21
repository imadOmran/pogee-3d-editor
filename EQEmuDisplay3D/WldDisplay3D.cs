using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;

using HelixToolkit;
using HelixToolkit.Wpf;

using EQEmu.Files.WLD;
using EQEmu.Files.WLD.Fragments;

namespace EQEmuDisplay3D
{
    public class WldDisplay3D : IDisplay3D, IDisposable
    {
        private readonly Model3D _group = new Model3DGroup();
        private readonly IEnumerable<Mesh> _zoneMeshes;
        private ViewClipping _clipping = new ViewClipping();
        private WLD _wld = null;
        private WLD _placeables = null;
        private WLD _objects = null;

        public System.Windows.Media.Media3D.Model3D Model
        {
            get { return _group; }
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

        public WldDisplay3D(WLD wld,WLD placeables=null,WLD objectMesh=null)
        {
            _wld = wld;
            _placeables = placeables;
            _objects = objectMesh;

            _zoneMeshes = wld.ZoneMeshes;
        }

        public void UpdateAll()
        {
            RenderMesh(_zoneMeshes);
        }

        public void RenderMesh(IEnumerable<Mesh> meshes)
        {
            if (meshes == null) return;
            Model3DGroup group = Model as Model3DGroup;

            group.Children.Clear();
            Dictionary<BitmapImage, List<EQEmu.Files.WLD.Polygon>> polysbyTex = new Dictionary<BitmapImage, List<EQEmu.Files.WLD.Polygon>>();
            List<EQEmu.Files.WLD.Polygon> untexturedPolys = new List<EQEmu.Files.WLD.Polygon>();
            foreach (var mesh in meshes)
            {
                foreach (var p in mesh.Polygons)
                {
                    if (p.Image != null)
                    {
                        if (polysbyTex.ContainsKey(p.Image))
                        {
                            polysbyTex[p.Image].Add(p);
                        }
                        else
                        {
                            polysbyTex[p.Image] = new List<EQEmu.Files.WLD.Polygon>();
                            polysbyTex[p.Image].Add(p);
                        }
                    }
                    else
                    {
                        untexturedPolys.Add(p);
                    }
                }
            }

            Material mat = null;
            foreach (var polytex in polysbyTex)
            {
                MeshBuilder builder = new MeshBuilder();
                if (mat == null)
                {
                    if (polytex.Value.ElementAt(0).Image != null)
                    {
                        //mat = HelixToolkit.Wpf.MaterialHelper.CreateImageMaterial(polytex.Value.ElementAt(0).Image, 100.0);
                        var img = polytex.Value.ElementAt(0).Image;
                        var brush = new System.Windows.Media.ImageBrush(img);
                        brush.ViewportUnits = System.Windows.Media.BrushMappingMode.Absolute;
                        //brush.TileMode
                        brush.TileMode = System.Windows.Media.TileMode.Tile;
                        //brush.Stretch = System.Windows.Media.Stretch.Fill;
                        mat = HelixToolkit.Wpf.MaterialHelper.CreateMaterial(brush);
                    }
                    else
                    {
                        mat = Materials.LightGray;
                    }
                }
                foreach (var poly in polytex.Value)
                {
                    Point3D p1 = new Point3D(poly.V1.X, poly.V1.Y, poly.V1.Z);
                    Point3D p2 = new Point3D(poly.V2.X, poly.V2.Y, poly.V2.Z);
                    Point3D p3 = new Point3D(poly.V3.X, poly.V3.Y, poly.V3.Z);
                    var rotate = new RotateTransform3D();
                    rotate.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 90);
                    p1 = rotate.Transform(p1);
                    p2 = rotate.Transform(p2);
                    p3 = rotate.Transform(p3);

                    if (!Clipping.DrawPoint(p1) || !Clipping.DrawPoint(p2) || !Clipping.DrawPoint(p3))
                    {
                        continue;
                    }

                    //v coordinate - negate it to convert from opengl coordinates to directx
                    var t1 = new System.Windows.Point(poly.V1.U, 1 - poly.V1.V);
                    var t2 = new System.Windows.Point(poly.V2.U, 1 - poly.V2.V);
                    var t3 = new System.Windows.Point(poly.V3.U, 1 - poly.V3.V);

                    //var t1 = new System.Windows.Point(0.0, 0.0);
                    //var t2 = new System.Windows.Point(2.0, 0.0);
                    //var t3 = new System.Windows.Point(0.0, 2.0);
                    //builder.AddTriangle(p3, p2, p1, t3, t2, t1);
                    builder.AddTriangle(p3, p2, p1, t3, t2, t1);
                }
                group.Children.Add(new GeometryModel3D(builder.ToMesh(), mat));
                mat = null;
            }

            //create the untextured polygons... basically a copy and paste from above which sucks... TODO
            var bbuilder = new MeshBuilder();
            mat = Materials.LightGray;
            foreach (var poly in untexturedPolys)
            {
                Point3D p1 = new Point3D(poly.V1.X, poly.V1.Y, poly.V1.Z);
                Point3D p2 = new Point3D(poly.V2.X, poly.V2.Y, poly.V2.Z);
                Point3D p3 = new Point3D(poly.V3.X, poly.V3.Y, poly.V3.Z);
                var rotate = new RotateTransform3D();
                rotate.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 90);
                p1 = rotate.Transform(p1);
                p2 = rotate.Transform(p2);
                p3 = rotate.Transform(p3);

                if (!Clipping.DrawPoint(p1) || !Clipping.DrawPoint(p2) || !Clipping.DrawPoint(p3))
                {
                    continue;
                }
                bbuilder.AddTriangle(p3, p2, p1);
            }
            group.Children.Add(new GeometryModel3D(bbuilder.ToMesh(), mat));
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
