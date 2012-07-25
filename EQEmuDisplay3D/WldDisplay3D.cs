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
            BuildPlaceableas();
        }

        private IEnumerable<PolygonRender> _renderObjects = null;
        private void BuildPlaceableas()
        {
            List<PolygonRender> placeables = new List<PolygonRender>();
            string rem = "ACTORDEF";
            string rep = "DMSPRITEDEF";
            
            if (_placeables != null && _objects != null)
            {
                List<PolygonRender> polyr = new List<PolygonRender>();
                foreach(var p in _placeables.ObjectLocations)
                {
                    if (!p.HasFragmentReference) continue;
                    var mesh = _objects.ZoneMeshes.FirstOrDefault( x => x.FragmentName == p.ReferencedName.Replace(rem,rep) );
                    if (mesh != null)
                    {
                        polyr.Add(new PolygonRender(mesh.Polygons, p));
                    }
                }
                _renderObjects = polyr;
            }
        }

        public void RenderObjects()
        {
            if (_renderObjects == null) return;
            Model3DGroup group = Model as Model3DGroup;

            #region Texturing
            /*
            Dictionary<BitmapImage, List<PolygonRender> > polysByTexture = new Dictionary<BitmapImage, List<PolygonRender> >();            
            
            foreach (var obj in _renderObjects)
            {
                foreach (var p in obj.Polygons)
                {
                    if (p.Image == null) continue;

                    if (!polysByTexture.ContainsKey(p.Image))
                    {
                        polysByTexture[p.Image] = new List<PolygonRender>();   
                    }
                    polysByTexture[p.Image].Add(obj);
                }
            }

            var rotate = new RotateTransform3D();
            rotate.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 90);
            var builder = new MeshBuilder();
            Material mat = Materials.Gold;
            foreach (var obj in polysByTexture)
            {
                foreach (var pr in obj.Value)
                {
                    var transforms = pr.Location.GetTransforms();
                    float scaleY = pr.Location.ScaleY;
                    float scaleX = pr.Location.ScaleX;
                    float scaleZ = pr.Location.ScaleZ;

                    foreach(var poly in pr.Polygons.Where( x => x.Image == obj.Key ) )
                    {
                        Point3D p1 = new Point3D(poly.V1.X * scaleX, poly.V1.Y * scaleY, poly.V1.Z * scaleZ);
                        Point3D p2 = new Point3D(poly.V2.X * scaleX, poly.V2.Y * scaleY, poly.V2.Z * scaleZ);
                        Point3D p3 = new Point3D(poly.V3.X * scaleX, poly.V3.Y * scaleY, poly.V3.Z * scaleZ);

                        foreach (var t in transforms)
                        {
                            p1 = t.Transform(p1);
                            p2 = t.Transform(p2);
                            p3 = t.Transform(p3);
                        }

                        p1 = rotate.Transform(p1);
                        p2 = rotate.Transform(p2);
                        p3 = rotate.Transform(p3);

                        if (!Clipping.DrawPoint(p1) || !Clipping.DrawPoint(p2) || !Clipping.DrawPoint(p3))
                        {
                            continue;
                        }

                        //v coordinate - negate it to convert from opengl coordinates to directx
                        //var t1 = new System.Windows.Point(poly.V1.U, 1 - poly.V1.V);
                        //var t2 = new System.Windows.Point(poly.V2.U, 1 - poly.V2.V);
                        //var t3 = new System.Windows.Point(poly.V3.U, 1 - poly.V3.V);

                        //var t1 = new System.Windows.Point(0.0, 0.0);
                        //var t2 = new System.Windows.Point(2.0, 0.0);
                        //var t3 = new System.Windows.Point(0.0, 2.0);
                        //builder.AddTriangle(p3, p2, p1, t3, t2, t1);
                        //builder.AddTriangle(p3, p2, p1, t3, t2, t1);
                        builder.AddTriangle(p3, p2, p1);
                        
                    }
                }                        
            }
            group.Children.Add(new GeometryModel3D(builder.ToMesh(), mat));        
            */
            #endregion

            Material mat = Materials.Gold;
            var builder = new MeshBuilder();           
            var rotate = new RotateTransform3D();
            rotate.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 90);
            foreach (var obj in _renderObjects)
            {
                var transforms = obj.Location.GetTransforms();

                float scaleY = obj.Location.ScaleY;
                float scaleX = obj.Location.ScaleX;
                float scaleZ = obj.Location.ScaleZ;

                foreach (var poly in obj.Polygons)
                {
                    Point3D p1 = new Point3D(poly.V1.X * scaleX, poly.V1.Y * scaleY, poly.V1.Z * scaleZ);
                    Point3D p2 = new Point3D(poly.V2.X * scaleX, poly.V2.Y * scaleY, poly.V2.Z * scaleZ);
                    Point3D p3 = new Point3D(poly.V3.X * scaleX, poly.V3.Y * scaleY, poly.V3.Z * scaleZ);
                    
                    foreach (var t in transforms)
                    {
                        p1 = t.Transform(p1);
                        p2 = t.Transform(p2);
                        p3 = t.Transform(p3);
                    }                    

                    p1 = rotate.Transform(p1);
                    p2 = rotate.Transform(p2);
                    p3 = rotate.Transform(p3);

                    if (!Clipping.DrawPoint(p1) || !Clipping.DrawPoint(p2) || !Clipping.DrawPoint(p3))
                    {
                        continue;
                    }
                    builder.AddTriangle(p3, p2, p1);
                }            
            }
            group.Children.Add(new GeometryModel3D(builder.ToMesh(), mat));
         }

        public void UpdateAll()
        {
            RenderMesh(_zoneMeshes);
            RenderObjects();
        }
        
        public void RenderModel(ModelReference model)
        {
            var wld = _wld;
            TrackAnimationBuilder animation = null;
            List<MeshReference> meshrefs = new List<MeshReference>();
            foreach (var refs in model.References)
            {
                var meshref = wld.MeshReferences.Where(x => x.FragmentNumber == refs).FirstOrDefault();
                if (meshref != null)
                {
                    meshrefs.Add(meshref);
                    continue;
                }

                var skel = wld.SkeletonTrackReferences.Where(x => x.FragmentNumber == refs).FirstOrDefault();
                if (skel != null)
                {
                    var skelset = wld.SkeletonTrackSet.Where(x => x.FragmentNumber == skel.SkeletonTrackSetReference).FirstOrDefault();
                    if (skelset != null)
                    {
                        animation = new TrackAnimationBuilder(skelset, _wld);

                        foreach (var ms in skelset.MeshReferences)
                        {
                            var m = wld.MeshReferences.Where(x => x.FragmentNumber == ms).FirstOrDefault();
                            if (m != null) meshrefs.Add(m);
                        }
                    }
                }
            }

            List<Mesh> meshes = new List<Mesh>();
            foreach (var m in meshrefs)
            {
                var mesh = wld.ZoneMeshes.Where(x => x.FragmentNumber == m.FragmentReference).FirstOrDefault();
                if (mesh != null) meshes.Add(mesh);
            }

            if (meshes.Count > 0)
            {
                RenderMesh(meshes, animation);                
            }            
        }

        public void RenderMesh(IEnumerable<Mesh> meshes, TrackAnimationBuilder animation=null)
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

                    if (animation != null)
                    {
                        if (animation.SkeletonPieceTransforms.ContainsKey(poly.V1.BodyPiece))
                        {
                            var atrans = animation.SkeletonPieceTransforms[poly.V1.BodyPiece];
                            p1 = atrans.Transform(p1);
                        }

                        if (animation.SkeletonPieceTransforms.ContainsKey(poly.V2.BodyPiece))
                        {
                            var atrans = animation.SkeletonPieceTransforms[poly.V2.BodyPiece];
                            p2 = atrans.Transform(p2);
                        }

                        if (animation.SkeletonPieceTransforms.ContainsKey(poly.V3.BodyPiece))
                        {
                            var atrans = animation.SkeletonPieceTransforms[poly.V3.BodyPiece];
                            p3 = atrans.Transform(p3);
                        }
                    }

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
