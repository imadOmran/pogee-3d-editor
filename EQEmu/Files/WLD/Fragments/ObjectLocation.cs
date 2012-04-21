using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

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
    }
}
