using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EQEmu.Files.WLD.Fragments
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct Fragment13Struct
    {
        public int flags;
        public int params1;
    };


    public class SkeletonPieceTrackReference : Fragment<Fragment13Struct>
    {
        private int _fragmentRef;

        public SkeletonPieceTrackReference(int num, int nameRef)
            : base(num, nameRef)
        {

        }

        public override void Handler(System.IO.Stream stream)
        {
            var barray = new byte[4];
            stream.Read(barray, 0, 4);
            _fragmentRef = BitConverter.ToInt32(barray, 0) - 1;

            base.Handler(stream); 
        }

        public int SkeletonPieceTrackRef
        {
            get { return _fragmentRef; }
        }

        public override string ToString()
        {
            return base.ToString() + " SkeletonPieceTrackReference " + this.FragmentName;
        }
    }
}
