using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media.Media3D;

namespace EQEmuDisplay3D
{
    public class ViewClipping
    {
        private float _xMin = 0.0f;
        public float XMin
        { 
            get { return _xMin; }
            set { _xMin = value; ClippingChanged(); }
        }

        private float _xMax = 0.0f;
        public float XMax
        {
            get { return _xMax; }
            set { _xMax = value; ClippingChanged(); }
        }

        private float _yMin = 0.0f;
        public float YMin
        {
            get { return _yMin; }
            set { _yMin = value; ClippingChanged(); }
        }

        private float _yMax = 0.0f;
        public float YMax
        {
            get { return _yMax; }
            set { _yMax = value; ClippingChanged(); }
        }

        private float _zMin = 0.0f;
        public float ZMin
        { 
            get { return _zMin; }
            set { _zMin = value; ClippingChanged(); }
        }

        private float _zMax = 0.0f;
        public float ZMax
        {
            get { return _zMax; }
            set { _zMax = value; ClippingChanged(); }
        }

        public delegate void ClippingChangedHandler(object sender, EventArgs e);
        public event ClippingChangedHandler OnClippingChanged;
        private void ClippingChanged()
        {
            if (OnClippingChanged != null)
            {
                OnClippingChanged(this, new EventArgs());
            }
        }

        public bool DrawPoint(Point3D p)
        {
            if (XMin != 0 && p.X < XMin) return false;
            if (XMax != 0 && p.X > XMax) return false;

            if (YMin != 0 && p.Y < YMin) return false;
            if (YMax != 0 && p.Y > YMax) return false;

            if (ZMin != 0 && p.Z < ZMin) return false;
            if (ZMax != 0 && p.Z > ZMax) return false;
            
            return true;
        }
    }

    public interface IDisplay3D
    {
        Model3D Model { get; }
        ViewClipping Clipping { get; set; }
    }
}
