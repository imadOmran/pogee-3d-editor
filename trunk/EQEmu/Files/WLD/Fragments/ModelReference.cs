using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EQEmu.Files.WLD.Fragments
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public unsafe struct Fragment14Struct
    {
        public int flags;
        public int fragment1;
        public int size1;
        public int size2;
        public int fragment2;
        //public int params1;
        //public fixed Int32 params2[7];
    };

    public class ModelReference : Fragment<Fragment14Struct>
    {
        private int _magicString;
        private List<int> _fragmentRefs = new List<int>();

        public ModelReference(int num, int nameRef)
            : base(num, nameRef)
        {

        }

        public override string ToString()
        {
            return base.ToString() + " Model " + this.FragmentName;
        }

        public override void Handler(System.IO.Stream stream)
        {
            base.Handler(stream);
            var fragment = FragmentStruct;
            bool frag2Zero = false;

            //check bits 1,2,8                        
            if ((fragment.flags & 0x8) == 0) frag2Zero = true;

            _magicString = fragment.fragment1;

            if ((fragment.flags & 0x1) > 0)
            {
                stream.Seek(4, System.IO.SeekOrigin.Current);
            }
            if ((fragment.flags & 0x2) > 0)
            {
                stream.Seek(4 * 7, System.IO.SeekOrigin.Current);
            }

            for (int i = 0; i < fragment.size1; i++)
            {
                var barray = new byte[4];
                stream.Read(barray, 0, 4);                
                var entry1size = BitConverter.ToInt32(barray, 0);
                for (int j = 0; j < entry1size; j++)
                {
                    stream.Seek(8, System.IO.SeekOrigin.Current);
                }
            }

            for (int i = 0; i < fragment.size2; i++)
            {
                var barray = new byte[4];
                stream.Read(barray, 0, 4);
                _fragmentRefs.Add(BitConverter.ToInt32(barray, 0) - 1);
            }
        }

        public IEnumerable<int> References
        {
            get { return _fragmentRefs; }
        }

        public int MagicStringRef
        {
            get { return 0 - _magicString; }
        }

        public string MagicString
        {
            get;
            set;
        }
    }
}
