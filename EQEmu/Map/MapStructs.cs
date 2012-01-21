using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EQEmu.Map
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public UInt32 version;
        public UInt32 faceCount;
        public UInt16 nodeCount;
        public UInt32 faceList;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Face
    {
        public Types.Vertex v1;
        public Types.Vertex v2;
        public Types.Vertex v3;
        public float nx, ny, nz, nd;
    }
}
