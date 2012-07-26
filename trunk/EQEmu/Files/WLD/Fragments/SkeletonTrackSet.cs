using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Media3D;

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

    public class SkeletonTrackSetEntry
    {
        private SkeletonTrackSetEntry1 _entry1;
        private List<int> _skeletonTree = new List<int>();

        public SkeletonTrackSetEntry(SkeletonTrackSetEntry1 entry,List<int> skeletonTree)
        {
            _entry1 = entry;
            _skeletonTree = skeletonTree;            
        }

        public SkeletonTrackSetEntry1 EntryStruct
        {
            get { return _entry1; }
        }

        public IEnumerable<int> SkeletonTree
        {
            get { return _skeletonTree; }
        }
    }


    public class SkeletonTrackSet : Fragment<Fragment10Struct>
    {
        private List<int> _meshRefs = new List<int>();
        private List<SkeletonTrackSetEntry> _entries = new List<SkeletonTrackSetEntry>();

        public SkeletonTrackSet(int num, int nameRef)
            : base(num, nameRef)
        {

        }

        public IEnumerable<int> MeshReferences
        {
            get { return _meshRefs; }
        }

        public IEnumerable<SkeletonTrackSetEntry> Entries
        {
            get { return _entries; }
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
                _entries.Add(new SkeletonTrackSetEntry(entry1, skelTree));
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

    public class TrackAnimationBuilder
    {
        private SkeletonTrackSet _trackSet;
        private WLD _wld;
        private Dictionary<int, Transform3D> _transforms = new Dictionary<int, Transform3D>();

        public TrackAnimationBuilder(SkeletonTrackSet trackSet, WLD wld)
        {
            _trackSet = trackSet;
            _wld = wld;

            BuildNode(0, null);
        }

        public Dictionary<int, Transform3D> SkeletonPieceTransforms
        {
            get { return _transforms; }
        }

        private void BuildNode(int index, Transform3D parentTransform)
        {
            var tgroup = new Transform3DGroup();

            var root = _trackSet.Entries.ElementAt(index);
            var fragRef = _wld.SkeletonPieceReferences.FirstOrDefault(x => x.FragmentNumber == root.EntryStruct.fragment1 - 1);
            if (fragRef != null)
            {
                var frag = _wld.SkeletonPieces.FirstOrDefault(x => x.FragmentNumber == fragRef.SkeletonPieceTrackRef);
                if (frag != null)
                {
                    var shift = new TranslateTransform3D(frag.ShiftX, frag.ShiftY, frag.ShiftZ);
                    tgroup.Children.Add(shift);

                    var xRotate = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), frag.RotateXDegrees));
                    var yRotate = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), frag.RotateYDegrees));
                    var zRotate = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), frag.RotateZDegrees));
                    tgroup.Children.Add(xRotate);
                    tgroup.Children.Add(yRotate);
                    tgroup.Children.Add(zRotate);

                    if (parentTransform != null)
                    {
                        tgroup.Children.Add(parentTransform);
                    }
                }
            }

            _transforms[index] = tgroup;

            foreach (var node in root.SkeletonTree)
            {
                BuildNode(node, tgroup);
            }
        }
    }
}
