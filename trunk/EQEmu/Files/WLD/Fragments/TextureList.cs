using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EQEmu.Files.WLD.Fragments
{
    // 0x31 - Texture List
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct Fragment31Struct
    {
        public UInt32 flags;						// Must contain 0
        public UInt32 size;						// The number of texture references this fragment contains
    };

    public class TextureList : Fragment<Fragment31Struct>
    {
        private List<int> _textureReference = new List<int>();

        public TextureList(int num)
            : base(num)
        {

        }

        public IEnumerable<int> TextureReference
        {
            get { return _textureReference; }
        }

        public override string ToString()
        {
            return base.ToString() + " TextureReference";
        }

        public override void Handler(System.IO.Stream stream)
        {
            base.Handler(stream);
            var frag = FragmentStruct;
            
            var barray = new byte[4];
            for (int i = 0; i < frag.size; i++)
            {
                stream.Read(barray, 0, 4);
                //var val = BitConverter.ToInt32(barray, 0);

                //read in references off by 1?
                var val = BitConverter.ToInt32(barray, 0) - 1;
                _textureReference.Add(val);
            }
        }
    }
}
