using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace EQEmu.Map
{
    public class Map
    {
        private Header _header;
        private List<Face> _faces;

        public List<Face> Triangles
        {
            get
            {
                return _faces;
            }
        }


        public static Map LoadFile(string file)
        {
            Map m = new Map();

            FileStream fs = new FileStream(file, FileMode.Open);

            Stream s = fs;
            m.ReadHeader(ref s);
            m.ReadFaces(ref s);
            fs.Close();

            return m;
        }

        private void ReadFaces(ref Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            int bufferSize = Marshal.SizeOf(typeof(Face));

            _faces = new List<Face>();

            byte[] face = new byte[bufferSize];

            for (int i = 0; i < _header.faceCount; i++)
            {
                br.Read(face, 0, bufferSize);
                _faces.Add(Functions.ByteArrayToStructure<Face>(face));
            }
        }

        private void ReadHeader(ref Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            byte[] buffer = new byte[Marshal.SizeOf(typeof(Header))];

            br.Read(buffer, 0, Marshal.SizeOf(typeof(Header)));
            _header = Functions.ByteArrayToStructure<Header>(buffer);
        }
    }
}
