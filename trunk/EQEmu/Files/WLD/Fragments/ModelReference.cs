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
        public int params1;
        public fixed int params2[7];        
    };

    class ModelReference
    {
    }
}
