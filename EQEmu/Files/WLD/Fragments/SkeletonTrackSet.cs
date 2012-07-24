using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EQEmu.Files.WLD.Fragments
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct Fragment10Struct
    {
        public int flags;
        public int size1;
        public int fragment;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct SkeletonTrackSetEntry1
    {
        public int nameRef;
        public int flags;
        public int fragment1;
        public int fragment2;
        public int entryCount;
    }

    public class SkeletonTrackSet : Fragment<Fragment10Struct>
    {
        private List<int> _meshRefs = new List<int>();

        public SkeletonTrackSet(int num, int nameRef)
            : base(num, nameRef)
        {

        }

        public IEnumerable<int> MeshReferences
        {
            get { return _meshRefs; }
        }

        public override string ToString()
        {
            return base.ToString() + " SkeletonTrackSet " + FragmentName;
        }

        public override void Handler(System.IO.Stream stream)
        {
            base.Handler(stream);

            var fragment = FragmentStruct;            
            if ( (fragment.flags & 0x01) > 0)
            {                
                //param1 ... skip
                stream.Seek(12, System.IO.SeekOrigin.Current);
            }

            if ((fragment.flags & 0x02) > 0)
            {
                //param2 ... skip
                stream.Seek(4, System.IO.SeekOrigin.Current);
            }

            int size = Marshal.SizeOf(typeof(SkeletonTrackSetEntry1));
            var barray = new byte[size];
            for (int i = 0; i < fragment.size1; i++)
            {
                stream.Read(barray, 0, size);
                var entry1 = Functions.ByteArrayToStructure<SkeletonTrackSetEntry1>(barray);
                List<int> skelTree = new List<int>();
                for (int j = 0; j < entry1.entryCount; j++)
                {
                    stream.Read(barray, 0, 4);
                    skelTree.Add(BitConverter.ToInt32(barray, 0));
                    //stream.Seek(4, System.IO.SeekOrigin.Current);
                }
            }

            if ( (fragment.flags & (1 << 9)) > 0)
            {
                stream.Read(barray, 0, 4);
                var size2 = BitConverter.ToInt32(barray, 0);
                for (int k = 0; k < size2; k++)
                {
                    stream.Read(barray, 0, 4);
                    _meshRefs.Add(BitConverter.ToInt32(barray, 0) - 1);
                }
            }
        }
    }
}
