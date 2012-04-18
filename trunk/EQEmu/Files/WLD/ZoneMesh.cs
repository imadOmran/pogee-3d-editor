using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

using EQEmu.Files.WLD.Fragments;

namespace EQEmu.Files.WLD
{
    public class ZoneMesh
    {
        //private UInt32 _masterRef;
        //private UInt32 _id;

        private float _scale;
        private float _maxDist;
        private float _centerX, _centerY, _centerZ;
        private float _minX, _minY, _minZ, _maxX, _maxY, _maxZ;
        private int _textureIndex;
        private int _numVert, _numTexCoords, _numNorm, _numColor, _numPoly, _numPolyTex, _numVertTex, _numIndi;

        private List<Vertex> _vertices = new List<Vertex>();
        private List<Polygon> _polygons = new List<Polygon>();
        private List<PolyTexture> _polygonTextures = new List<PolyTexture>();

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

        public static ZoneMesh CreateZoneMesh(Fragment36Struct fragment)
        {
            ZoneMesh mesh = new ZoneMesh();

            mesh._textureIndex = (int)fragment.Fragment1;

            mesh._scale = 1.0f / (1 << fragment.Scale);
            mesh._maxDist = fragment.MaxDist;

            mesh._centerX = fragment.CenterX;
            mesh._centerY = fragment.CenterY;
            mesh._centerZ = fragment.CenterZ;

            mesh._minX = fragment.MinX;
            mesh._minY = fragment.MinY;
            mesh._minZ = fragment.MinZ;

            mesh._maxX = fragment.MaxX;
            mesh._maxY = fragment.MaxY;
            mesh._maxZ = fragment.MaxZ;

            mesh._numVert = fragment.VertexCount;
            mesh._numTexCoords = fragment.TexCoordsCount;
            mesh._numNorm = fragment.NormalsCount;
            mesh._numPoly = fragment.PolygonCount;
            mesh._numPolyTex = fragment.PolygonTexCount;
            mesh._numColor = fragment.ColorCount;
            mesh._numVertTex = fragment.VertexTexCount;
            mesh._numIndi = fragment.PolygonCount * 3;

            return mesh;
        }

        public int VertexCount
        {
            get { return _numVert; }
        }

        public int PolygonTextureCount
        {
            get { return _numPolyTex; }
        }

        public void BuildPolyTextures(byte[] polyTexs)
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
        }

        public void BuildVertices(byte[] vertices)
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

        public void BuildTextureCoords(byte[] textcoords)
        {
            int size = Marshal.SizeOf(typeof(TexCoordsOld));
            var barray = new byte[size];

            using (var ms = new MemoryStream(textcoords))
            {
                for (int i = 0; i < _numTexCoords; i++)
                {
                    ms.Read(barray, 0, size);
                    var tex = Functions.ByteArrayToStructure<TexCoordsOld>(barray);

                    _vertices.ElementAt(i).U = tex.tx / 256.0f;
                    _vertices.ElementAt(i).V = tex.tz / 256.0f;
                }
            }
        }

        public void BuildPolygons(byte[] polys)
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
