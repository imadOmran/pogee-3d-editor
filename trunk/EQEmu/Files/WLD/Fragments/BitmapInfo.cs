using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace EQEmu.Files.WLD.Fragments
{    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct Fragment04Struct
    {
        public UInt32 flags;                                           // Bit 3 is 1 if texture is animated, if bit 4 is 1, params2 exits
        public UInt32 size;                                            // Contains the number of 0x03 fragment references
    };

    public class BitmapInfo : Fragment<Fragment04Struct>
    {
        private bool _animated = false;
        private int _numBmp = 0;
        private UInt32 _delay = 0;
        private List<int> _bitmapNameReferences = new List<int>();
        
        public bool IsAnimated
        {
            get { return _animated; }
            set
            {
                _animated = value;
            }
        }

        public int NumberBitmaps
        {
            get { return _numBmp; }
            set
            {
                _numBmp = value;
            }
        }

        public uint Delay
        {
            get { return _delay; }
            set
            {
                _delay = value;
            }
        }

        public List<int> FragmentReferences
        {
            get { return _bitmapNameReferences; }
        }

        public BitmapInfo(int num)
            : base(num)
        {

        }

        public override string ToString()
        {
            return base.ToString() + " BitmapInfo";
        }

        public override void Handler(Stream stream)
        {
            base.Handler(stream);

            var frag = FragmentStruct;
            
            var barray = new byte[4];
            stream.Read(barray, 0, 4);
            var delay = BitConverter.ToInt32(barray, 0);

            if ((frag.flags & (1 << 3)) != 0)
            {
                _animated = true;
                _delay = (uint)delay;
            }
            else
            {
                _animated = false;
            }

            _numBmp = (int)frag.size;
            
            for (int i = 0; i < _numBmp; i++)
            {
                //stream.Read(barray, 0, 4);
                //not sure on this
                //var num = BitConverter.ToInt32(barray, 0) - 1;
                //_bitmapNameReferences.Add(num);                

                _bitmapNameReferences.Add(FragmentNumber - _numBmp + i);
            }
        }
    }
}
