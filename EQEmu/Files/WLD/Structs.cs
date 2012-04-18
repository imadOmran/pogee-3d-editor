using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace EQEmu.Files.WLD
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    struct WLDHeader
    {
        public UInt32 Magic;                                           // This always contains 0x54503D02. It identifies the file as a .WLD file.      
        public UInt32 Version;                                 // For old-format .WLD files: 0x00015500. For new-format .WLD files: 0x1000C800
        public UInt32 FragmentCount;                           // Contains the number of fragments in the .WLD file, minus 1
        public UInt32 Header3;                                 // Should contain the number of 0x22 BSP Region fragments in the file
        public UInt32 Header4;                                 // Unknown purpose. Should contain 0x000680D4
        public UInt32 StringHashSize;                  // Contains the size of the string hash in bytes - Hash has to be decoded
        public UInt32 Header6;                                 // Unknown purpose
    }

    // Standard WLD fragment
    // Note: Reference has one more field but I have included it in the reference fragments structs
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    struct BasicWLDFragment
    {
        public UInt32 Size;                                            // Size of the fragment in bytes. Padded to be divisible by 4 (not always the case)
        public UInt32 Id;                                              // The fragment type
        public UInt32 NameRef;                                 // Each fragment can have a string name, which is stored in encoded form in the .WLD file�s string hash.
        // The NameReference gives a way to retrieve the name. If the fragment has a string name, its NameReference
        // should contain the negative value of the string�s index in the string hash. For example, if the string is
        // at position 31 in the string hash, then NameReference should contain �31. Values greater than 0 mean that
        // the fragment doesn�t have a string name.
    };





    // 0x22 - BSP Region
    // Note: For each node in the BSP tree there is a 0x22 fragment that describes the region. It contains information on the region's 
    // bounds and an optional reference to a 0x36 fragment if there are polygons in that region. Also contains an RLE-encoded array of
    // data that tells the client which regions are "nearby".
    /*
    struct Fragment22
    {
        public UInt32 flags, fragment1, size1, size2, params1, size3, size4, params2, size5, size6;
    }; */
                
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    struct RawVertex
    {
        public short x, y, z;
    }

    public class Vertex
    {
        private float _x, _y, _z;
        private float _u, _v;

        public Vertex(float x, float y, float z)
        {
            _x = x; _y = y; _z = z;
        }

        public float X
        {
            get { return _x; }
            set
            {
                _x = value;
            }
        }

        public float Y
        {
            get { return _y; }
            set
            {
                _y = value;
            }
        }

        public float Z
        {
            get { return _z; }
            set
            {
                _z = value;
            }
        }

        public float U
        {
            get { return _u; }
            set
            {
                _u = value;
            }
        }

        public float V
        {
            get { return _v; }
            set
            {
                _v = value;
            }
        }
    }



    // Polygon Texture Structure
    // Note: One render call will be executed x times, where x is the number of different textures in the mesh
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct PolyTexture
    {
        public short polyCount;
        public short texIndex;
    };

    // The texture mapping coordinates for the old zone files
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    struct TexCoordsOld 
    {
        public short tx, tz;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct RawPoly
    {
        public short solid, v1, v2, v3;
    };

    public class Polygon
    {
        private bool _solid;
        private Vertex _v1;
        private Vertex _v2;
        private Vertex _v3;

        public BitmapImage Image
        {
            get;
            set;
        }

        public int TextureListIndex
        {
            get;
            set;
        }

        //private short _v1;
        //private short _v2;
        //private short _v3;

        public Polygon(bool solid, Vertex v1, Vertex v2, Vertex v3)
        {
            _solid = solid;
            _v1 = v1;
            _v2 = v2;
            _v3 = v3;
        }

        public Polygon()
        {

        }

        public bool Solid
        {
            get { return _solid; }
            set
            {
                _solid = value;
            }
        }

        public Vertex V1
        {
            get { return _v1; }
            set
            {
                _v1 = value;
            }
        }

        public Vertex V2
        {
            get { return _v2; }
            set
            {
                _v2 = value;
            }
        }

        public Vertex V3
        {
            get { return _v3; }
            set
            {
                _v3 = value;
            }
        }
    }
}
