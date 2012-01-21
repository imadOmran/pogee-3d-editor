using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Windows.Media.Media3D;

using HelixToolkit;
using HelixToolkit.Wpf;
using EQEmu.Map;

namespace EQEmuDisplay3D
{
    public class MapDisplay3D : IDisplay3D
    {
        private readonly EQEmu.Map.Map _map;

        private readonly Model3D _group = new Model3DGroup();
        public System.Windows.Media.Media3D.Model3D Model
        {
            get { return _group; }
        }


        private ViewClipping _clipping = new ViewClipping();
        public ViewClipping Clipping
        {
            get { return _clipping; }
            set {
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

        public void UpdateAll()
        {
            if (_map == null || _map.Triangles == null) return;

            Model3DGroup group = Model as Model3DGroup;

            group.Children.Clear();

            List<Face> faces = _map.Triangles;            
            MeshBuilder builder = new MeshBuilder();

            foreach (Face face in faces)
            {
                Point3D p1 = new Point3D(face.v1.x,face.v1.y,face.v1.z);
                Point3D p2 = new Point3D(face.v2.x,face.v2.y,face.v2.z);
                Point3D p3 = new Point3D(face.v3.x,face.v3.y,face.v3.z);

                if( !Clipping.DrawPoint(p1) || !Clipping.DrawPoint(p2) || !Clipping.DrawPoint(p3) )
                {
                    continue;
                }

                builder.AddTriangle(p1, p2, p3);                   
            }
            group.Children.Add(new GeometryModel3D(builder.ToMesh(), Materials.Gray));                        
        }

        public MapDisplay3D(EQEmu.Map.Map map)
        {
            _map = map;
            UpdateAll();
        }
    }
}
