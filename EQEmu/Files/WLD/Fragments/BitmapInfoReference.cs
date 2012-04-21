using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace EQEmu.Files.WLD.Fragments
{
    // 0x05 - Texture Bitmap Info Reference
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct Fragment05Struct
    {
        public UInt32 reference;                                       // Points to a 0x04 fragment
        public UInt32 flags;                                           // Unknown - Usually contains 0x50
    };

    public class BitmapInfoReference : Fragment<Fragment05Struct>
    {
        private uint _bitmapFragmentReference;        

        public uint FragmentReference
        {
            get { return _bitmapFragmentReference; }
        }

        public BitmapInfoReference(int num,int name)
            : base(num,name)
        {

        }

        public override string ToString()
        {
            return base.ToString() + " BitmapInfoReference";
        }

        public override void Handler(Stream stream)
        {
            base.Handler(stream);
            var frag = FragmentStruct;

            //_bitmapFragmentReference = frag.reference - 1;
            _bitmapFragmentReference = frag.reference - 1;
        }
    }
}
