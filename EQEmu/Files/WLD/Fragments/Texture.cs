using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace EQEmu.Files.WLD.Fragments
{
    // 0x30 - Texture - Reference
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public unsafe struct Fragment30Struct
    {
        public UInt32 flags;                                           // Typically 1. If set to 1, then the Pair field exists.
        public UInt32 params1;                                 // Bit 0 - 1 if texture isn't transparent. 
                                                                        // Bit 1 - 1 if texture is masked (tree leaves). 
                                                                        // Bit 2 - 1 if the texture is semi-transparent but not masked.
                                                                        // Bit 3 - 1 if the texture is masked and semi-transparent.
                                                                        // Bit 4 - 1 if the texture is masked but not semi-transparent.
                                                                        // Bit 31 - Apparently must be 1 if the texture isn�t transparent.
                                                                        // *Note* - To make a fully transparent texture, set Params1 to 0.
        public UInt32 params2;                                 // Typically contains 0x004E4E4E, but I�ve also seen 0xB2B2B2. Could this be an RGB reflectivity value?
        public fixed float params3[2];                               // [0] - Typically contains 0. Its purpose is unknown.
                                                                        // [1] - Typically contains 0 for transparent textures and 0.75 for all others. Its purpose is unknown.
    };

    public class Texture : Fragment<Fragment30Struct>
    {
        private int _fragmentReference = 0;
        bool _masked = false;
        bool _semiTrans = false;
        bool _transparent = false;
        bool _empty = false;

        public Texture(int num,int name)
            : base(num,name)
        {

        }

        public int FragmentReference
        {
            get { return _fragmentReference; }
        }
        
        public bool Masked
        {
            get { return _masked; }
            set
            {
                _masked = value;
            }
        }

        public bool SemiTransparent
        {
            get { return _semiTrans; }
            set
            {
                _semiTrans = value;
            }
        }

        public bool Transparent
        {
            get { return _transparent; }
            set
            {
                _transparent = value;
            }
        }

        public bool Empty
        {
            get { return _empty; }
            set
            {
                _empty = value;
            }
        }

        public override string ToString()
        {
            return base.ToString() + " Texture";
        }

        public override void Handler(System.IO.Stream stream)
        {
            base.Handler(stream);
            var frag = FragmentStruct;

            if ((frag.flags & (1 << 2)) != 0)
            {
                stream.Seek(8, SeekOrigin.Current);
            }

            var barray = new byte[4];
            stream.Read(barray, 0, 4);

            _fragmentReference = BitConverter.ToInt32(barray, 0) - 1;

            if (frag.params1 == 0)
            {
                _transparent = true;                
            }
            else
            {
                if ((frag.params1 & (1 << 1)) != 0)
                {
                    _transparent = false;
                }

                if ((frag.params1 & (1 << 2)) != 0)
                {
                    _masked = true;
                }

                if ((frag.params1 & (1 << 3)) != 0)
                {                    
                    _semiTrans = true;
                    _masked = false;
                }

                if ((frag.params1 & (1 << 4)) != 0)
                {
                    _semiTrans = true;
                    _masked = true;
                }

                if ((frag.params1 & (1 << 5)) != 0)
                {
                    _masked = true;
                    _semiTrans = false;
                }
            }
        }
    }
}
