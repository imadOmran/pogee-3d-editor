using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EQEmu.Path
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct HeaderStruct
    {
        public Int32 version;
        public Int32 pathNodeCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct NodeStruct
    {
        public UInt16 id;
        public Types.Vertex v;
        public float bestZ;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct NeighborStruct
    {
        public Int16 id;
        public float distance;
        public byte teleport;
        public Int16 doorId;
    }
}
