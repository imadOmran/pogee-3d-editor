using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EQEmu.Files.WLD.Fragments
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct Fragment11Struct
    {
        public int params1;
    };

    public class SkeletonTrackReference : Fragment<Fragment11Struct>
    {
        private int _fragmentRef;

        public SkeletonTrackReference(int num, int nameRef)
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

        public int SkeletonTrackSetReference
        {
            get { return _fragmentRef; }
        }

        public override string ToString()
        {
            return base.ToString() + " SkeletonTrackReference " + this.FragmentName;
        }
    }
}
