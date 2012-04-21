using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Media.Media3D;

namespace EQEmu.Files.WLD.Fragments
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public unsafe struct Fragment15Struct
    {
        public int fragmentReference;
        public int flags;             
        public int fragment1;         
        public float x;
        public float y;
        public float z;
        public float rotateZ;
        public float rotateY;
        public float rotateX;
        public fixed float params1[3];
        public float scaleY;
        public float scaleX;
        public int fragment2;
        public int params2;

    };

    public class ObjectLocation : Fragment<Fragment15Struct>
    {
        public ObjectLocation(int num,int name)
            : base(num,name)
        {

        }

        public override string ToString()
        {
            return base.ToString() + " ObjectLocation";
        }

        public override void Handler(System.IO.Stream stream)
        {
            base.Handler(stream);            

        }
        
        public IEnumerable<Transform3D> GetTransforms()
        {
            List<Transform3D> transforms = new List<Transform3D>();

            RotateTransform3D rotateZ = new RotateTransform3D();
            rotateZ.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), FragmentStruct.rotateZ / 512 * 360);

            RotateTransform3D rotateY = new RotateTransform3D();
            rotateY.Rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), FragmentStruct.rotateY / 512 * 360);

            RotateTransform3D rotateX = new RotateTransform3D();
            rotateX.Rotation = new AxisAngleRotation3D(new Vector3D(1, 0, 0), FragmentStruct.rotateX / 512 * 360);

            TranslateTransform3D translate = new TranslateTransform3D();
            translate.OffsetX = FragmentStruct.x;
            translate.OffsetY = FragmentStruct.y;
            translate.OffsetZ = FragmentStruct.z;

            transforms.Add(rotateX);
            transforms.Add(rotateY);
            transforms.Add(rotateZ);
            transforms.Add(translate);

            return transforms;
        }

        public bool HasFragmentReference
        {
            get { return this.FragmentStruct.fragmentReference < 0; }
        }

        public int FragmentReference
        {
            get { return this.FragmentStruct.fragmentReference; }
        }

        public string ReferencedName
        {
            get;
            set;
        }
        
        public float X
        {
            get { return this.FragmentStruct.x; }
        }

        public float Y
        {
            get { return this.FragmentStruct.y; }
        }

        public float Z
        {
            get { return this.FragmentStruct.z; }
        }

        public float ScaleX
        {
            get { return this.FragmentStruct.scaleX; }
        }

        public float ScaleY
        {
            get { return this.FragmentStruct.scaleY; }
        }
    }
}
