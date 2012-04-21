using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace EQEmu.Files.WLD.Fragments
{
    // 0x36 - Mesh
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public unsafe struct Fragment36Struct
    {
        public UInt32 Flags;                                           // Usually contains 0x00018003 for zone meshes and 0x00014003 for placeable objects
        public UInt32 Fragment1;                                       // References a 0x31 Texture List fragment. It tells the client which textures this mesh uses. For zone meshes, a single 0x31 fragment should be built that contains all the textures used in the entire zone. For placeable objects, there should be a 0x31 fragment that references only those textures used in that particular object.
        public UInt32 Fragment2;                                       // If this mesh is animated (not character models, but things like swaying flags and trees), this references a 0x2F Mesh Animated Vertices Reference fragment.
        public UInt32 Fragment3;                                       // Unknown
        public UInt32 Fragment4;                                       //This typically references the very first 0x03 Texture Bitmap Name(s) fragment in the .WLD file. I have no idea why.
        public float CenterX;                                  // For zone meshes this typically contains the X coordinate of the center of the mesh.
        public float CenterY;                                  // Same but for Y
        public float CenterZ;                                  // Same but for Z
        public fixed UInt32 Params2[3];                              // Unknown. Typically contains 0
        public float MaxDist;                                  // Given the values in CenterX, CenterY, and CenterZ, this seems to contain the maximum distance between any vertex and that position. It seems to define a radius from that position within which the mesh lies.
        public float MinX;                                             // Seems to contain the minimum X coordinate of any vertex in the mesh, in absolute coordinates
        public float MinY;                                             // Seems to contain the minimum Y coordinate of any vertex in the mesh, in absolute coordinates
        public float MinZ;                                             // Seems to contain the minimum Z coordinate of any vertex in the mesh, in absolute coordinates
        public float MaxX;                                             // Seems to contain the maximum X coordinate of any vertex in the mesh, in absolute coordinates.
        public float MaxY;                                             // Seems to contain the maximum Y coordinate of any vertex in the mesh, in absolute coordinates.
        public float MaxZ;                                             // Seems to contain the maximum Z coordinate of any vertex in the mesh, in absolute coordinates.
        public short VertexCount;                  // Number of vertices there are in the mesh. Normally this is three times the number of polygons, but this is by no means necessary as polygons can share vertices.
        public short TexCoordsCount;               // Number of texture coordinate pairs there are in the mesh. This should equal the number of vertices in the mesh.
        public short NormalsCount;                 // Number of vertex normal entries there are in the mesh. This should equal the number of vertices in the mesh.
        public short ColorCount;                   // Number of vertex color entries are in the mesh. This should equal the number of vertices in the mesh, or zero if there are no vertex color entries.
        public short PolygonCount;                 // Number of polygons there are in the mesh.
        public short VertexPieceCount;             // This seems to only be used when dealing with animated (mob) models. It tells how many VertexPiece entries there are.
        public short PolygonTexCount;              // Number of PolygonTex entries there are. Polygons are grouped together by texture and PolygonTex entries tell the client how many polygons there are that use a particular texture.
        public short VertexTexCount;               // Number of VertexTex entries there are. Vertices are grouped together by texture and VertexTex entries tell the client how many vertices there are that use a particular texture.
        public short Size9;                                // This seems to only be used when dealing with animated (mob) models. It tells how many entries are in the Data9 area.
        public short Scale;                                // This allows vertex coordinates to be stored as integral values instead of floating-point values, without losing precision based on mesh size. Vertex values are multiplied by (1 shl Scale) and stored in the vertex entries.
    };

    public class PolygonRender
    {
        private IEnumerable<Polygon> _polys = null;
        private ObjectLocation _location = null;

        public PolygonRender(IEnumerable<Polygon> polys, ObjectLocation location)
        {
            _polys = polys;
            _location = location;
        }

        public IEnumerable<Polygon> Polygons
        {
            get { return _polys; }
        }

        public ObjectLocation Location
        {
            get { return _location; }
        }
    }

    public class Mesh : Fragment<Fragment36Struct>
    {
        private float _scale;
        private float _maxDist;
        private float _centerX, _centerY, _centerZ;
        private float _minX, _minY, _minZ, _maxX, _maxY, _maxZ;
        private int _textureIndex;
        private int _numVert, _numTexCoords, _numNorm, _numColor, _numPoly, _numPolyTex, _numVertTex, _numIndi;

        private List<Vertex> _vertices = new List<Vertex>();
        private List<Polygon> _polygons = new List<Polygon>();
        private List<PolyTexture> _polygonTextures = new List<PolyTexture>();

        public Mesh(int num,int name)
            : base(num,name)
        {

        }

        public override string ToString()
        {
            return base.ToString() + " Mesh";
        }

        /*
        public BitmapImage Image
        {
            get;
            set;
        }
        */

        public override void Handler(Stream stream)
        {
            base.Handler(stream);
            var fragment = FragmentStruct;             

            switch (fragment.Flags)
            {
                case 0x00018003:  //zone mesh
                case 0x00014003: //object mesh
                    break;
                default:
                    break;
            }

            _textureIndex = (int)fragment.Fragment1 - 1;
            _scale = 1.0f / (1 << fragment.Scale);
            _maxDist = fragment.MaxDist;

            _centerX = fragment.CenterX;
            _centerY = fragment.CenterY;
            _centerZ = fragment.CenterZ;

            _minX = fragment.MinX;
            _minY = fragment.MinY;
            _minZ = fragment.MinZ;

            _maxX = fragment.MaxX;
            _maxY = fragment.MaxY;
            _maxZ = fragment.MaxZ;

            _numVert = fragment.VertexCount;
            _numTexCoords = fragment.TexCoordsCount;
            _numNorm = fragment.NormalsCount;
            _numPoly = fragment.PolygonCount;
            _numPolyTex = fragment.PolygonTexCount;
            _numColor = fragment.ColorCount;
            _numVertTex = fragment.VertexTexCount;
            _numIndi = fragment.PolygonCount * 3;

            int size = Marshal.SizeOf(typeof(RawVertex)) * VertexCount;
            var barray = new byte[size];

            stream.Read(barray, 0, size);
            BuildVertices(barray);

            //texture coords            
            //skip
            size = Marshal.SizeOf(typeof(TexCoordsOld)) * fragment.TexCoordsCount;
            barray = new byte[size];
            stream.Read(barray, 0, size);
            BuildTextureCoords(barray);

            //normals
            //skip
            stream.Seek(3 * fragment.NormalsCount, SeekOrigin.Current);

            //colors
            stream.Seek(4 * fragment.ColorCount, SeekOrigin.Current);

            //polygons
            size = 8 * fragment.PolygonCount;
            barray = new byte[size];
            stream.Read(barray, 0, size);
            BuildPolygons(barray);

            //vertexpiece skip
            stream.Seek(4 * fragment.VertexPieceCount, SeekOrigin.Current);

            size = Marshal.SizeOf(typeof(PolyTexture)) * PolygonTextureCount;
            barray = new byte[size];
            stream.Read(barray, 0, size);
            BuildPolyTextures(barray);
        }

        public IEnumerable<Vertex> Vertices
        {
            get { return _vertices; }
        }

        public IEnumerable<Polygon> Polygons
        {
            get { return _polygons; }
        }

        public IEnumerable<PolyTexture> PolygonTextures
        {
            get { return _polygonTextures; }
        }

        public int FragmentReference
        {
            get { return _textureIndex; }
        }

        public int VertexCount
        {
            get { return _numVert; }
        }

        public int PolygonTextureCount
        {
            get { return _numPolyTex; }
        }

        private void BuildPolyTextures(byte[] polyTexs)
        {
            int size = Marshal.SizeOf(typeof(PolyTexture));
            var barray = new byte[size];

            using (var ms = new MemoryStream(polyTexs))
            {
                for (int i = 0; i < PolygonTextureCount; i++)
                {
                    ms.Read(barray, 0, size);
                    var polytex = Functions.ByteArrayToStructure<PolyTexture>(barray);
                    _polygonTextures.Add(polytex);
                }
            }

            int poly = 0;
            foreach (var pt in _polygonTextures)
            {
                for (int i = 0; i < pt.polyCount; i++)
                {
                    _polygons.ElementAt(poly).TextureListIndex = pt.texIndex;
                    poly++;
                }
            }
        }

        private void BuildVertices(byte[] vertices)
        {
            int size = Marshal.SizeOf(typeof(RawVertex));
            var barray = new byte[size];

            using (var ms = new MemoryStream(vertices))
            {
                for (int i = 0; i < VertexCount; i++)
                {
                    ms.Read(barray, 0, size);
                    var rawvert = Functions.ByteArrayToStructure<RawVertex>(barray);
                    var vert = new Vertex(
                        _centerX + rawvert.x * _scale,
                        _centerY + rawvert.y * _scale,
                        _centerZ + rawvert.z * _scale);
                    _vertices.Add(vert);
                }
            }
        }

        private void BuildTextureCoords(byte[] textcoords)
        {
            int size = Marshal.SizeOf(typeof(TexCoordsOld));
            var barray = new byte[size];

            using (var ms = new MemoryStream(textcoords))
            {
                for (int i = 0; i < _numTexCoords; i++)
                {
                    ms.Read(barray, 0, size);
                    var tex = Functions.ByteArrayToStructure<TexCoordsOld>(barray);

                    //switch(version)

                    //256 is not based on the image size... 
                    //ie. 64x64 dividing by 256 is still necessary to get correct uv coordinates
                    _vertices.ElementAt(i).U = tex.tx / 256.0f;
                    _vertices.ElementAt(i).V = tex.tz / 256.0f;
                }
            }
        }

        private void BuildPolygons(byte[] polys)
        {
            int size = Marshal.SizeOf(typeof(RawPoly));
            var barray = new byte[size];

            using (var ms = new MemoryStream(polys))
            {
                for (int i = 0; i < _numPoly; i++)
                {
                    ms.Read(barray, 0, size);
                    var rawpoly = Functions.ByteArrayToStructure<RawPoly>(barray);

                    var polygon = new Polygon()
                    {
                        Solid = rawpoly.solid == 0x0010 ? false : true,
                        V1 = _vertices.ElementAt(rawpoly.v1),
                        V2 = _vertices.ElementAt(rawpoly.v2),
                        V3 = _vertices.ElementAt(rawpoly.v3)
                    };
                    _polygons.Add(polygon);
                }
            }
        }
    }
}
