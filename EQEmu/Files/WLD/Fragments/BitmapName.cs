using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace EQEmu.Files.WLD.Fragments
{
    // 0x03 - Texture Bitmap Names
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct Fragment03Struct
    {
        public UInt32 size;                                            // Number of texture filenames in this fragment. Should always be 1.
        public short nameLen;                                  // Length of the texture name in bytes
    };

    public class BitmapName : Fragment<Fragment03Struct>
    {
        public BitmapName(int num,int name)
            : base(num,name)
        {

        }

        public string File { get; set; }

        public override void Handler(Stream stream)
        {
            var position = stream.Position;
            base.Handler(stream);
            var frag = FragmentStruct;
                        
            var barray = new byte[frag.nameLen];
            stream.Read(barray, 0, frag.nameLen);
            var decoded = WLD.DecodeFileName(barray);

            this.File = decoded;
        }

        public override string ToString()
        {
            return base.ToString() + " BitmapName";
        }
    }
}
