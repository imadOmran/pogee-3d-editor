using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EQEmu
{
    namespace Types
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct Vertex
        {
            public float x;
            public float y;
            public float z;
        }
    }
}
