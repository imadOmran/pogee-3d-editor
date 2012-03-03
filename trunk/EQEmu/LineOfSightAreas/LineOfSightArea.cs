using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EQEmu.LineOfSightAreas
{
    public class ZoneLineOfSightAreas
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Header
        {
            public uint polygonCount;
        }

        private ObservableCollection<LineOfSightArea> _areas = new ObservableCollection<LineOfSightArea>();
        public ObservableCollection<LineOfSightArea> Areas
        {
            get { return _areas; }
        }

        public static ZoneLineOfSightAreas LoadFileBinary(string file)
        {
            ZoneLineOfSightAreas zone = new ZoneLineOfSightAreas();

            FileStream fs = new FileStream(file.ToLower(), FileMode.Open);

            Stream s = fs;
            var header = zone.ReadHeader(ref s);

            for (int i = 0; i < header.polygonCount; i++)
            {
                var area = LineOfSightArea.LoadFromStream(ref s);
                zone.Areas.Add(area);
            }

            /*
            foreach (var area in zone.Areas)
            {
                if (area.Vertices.First().X != area.Vertices.Last().X && area.Vertices.First().Y != area.Vertices.Last().Y)
                {
                    Point3D p = new Point3D(area.Vertices.First().X, area.Vertices.First().Y, 0);
                    area.Vertices.Add(p);
                }
            }*/
            
            fs.Close();            

            return zone;
        }

        public void SaveFileBinary(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Create);
            Stream s = fs;
            
            WriteHeader(ref s);
            WriteAreas(ref s);
            fs.Close();
        }

        private void WriteHeader(ref Stream stream)
        {
            Header header = new Header();
            header.polygonCount = (uint)Areas.Count;
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(Functions.StructToByteArray(header));
        }

        private void WriteAreas(ref Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            foreach (var area in Areas)
            {
                var header = new LineOfSightArea.Header();
                header.maxZ = (float)area.MaxZ;
                header.minZ = (float)area.MinZ;
                header.version = 1;
                //+1 to account for the vertex we have to add to connect poly first = last
                header.vertexCount = area.Vertices.Count+1;

                bw.Write(Functions.StructToByteArray(header));
                foreach (var vertex in area.Vertices)
                {
                    var v = new LineOfSightArea.Vertex();
                    v.x = (float)vertex.X; v.y = (float)vertex.Y; v.z = 0;
                    bw.Write(Functions.StructToByteArray(v));
                }

                //one final vertex write to connect poly
                var last = new LineOfSightArea.Vertex();
                last.x = (float)area.Vertices[0].X; 
                last.y = (float)area.Vertices[0].Y;
                last.z = 0;
                bw.Write(Functions.StructToByteArray(last));
            }
        }

        public void AddArea(LineOfSightArea area)
        {
            this.Areas.Add(area);
        }

        public void RemoveArea(LineOfSightArea area)
        {
            Areas.Remove(area);
        }

        private Header ReadHeader(ref Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            int size = Marshal.SizeOf(typeof(Header));

            byte[] buffer = new byte[size];
            br.Read(buffer, 0, size);

            return Functions.ByteArrayToStructure<Header>(buffer);
        }
    }

    public class LineOfSightArea
    {
        //[StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct Header
        {
		    public short version;
            public int vertexCount;
            public float minZ;
            public float maxZ;
        }

        public struct Vertex
        {
            public float x;
            public float y;
            public float z;
        }


        private double _minZ = 0.0;
        public double MinZ
        {
            get { return _minZ; }
            set
            {
                _minZ = value;
            }
        }

        private double _maxZ = 0.0;
        public double MaxZ
        {
            get { return _maxZ; }
            set
            {
                _maxZ = value;
            }
        }
        private short _version = 0;

        private ObservableCollection<Point3D> _vertices = new ObservableCollection<Point3D>();  
        [Browsable(false)]
        public ObservableCollection<Point3D> Vertices
        {
            get { return _vertices; }
        }

        public void RemoveVertex(Point3D vertex)
        {
            /*
            Point3D first = Vertices.First();

            if (vertex.X == first.X && vertex.Y == first.Y)
            {
                Vertices.Remove(Vertices.First());
                Vertices.Remove(Vertices.Last());

                Point3D duplicate = new Point3D(Vertices.First().X, Vertices.First().Y, 0);
                Vertices.Add(duplicate);
            }
            else { Vertices.Remove(vertex); }
            */
            Vertices.Remove(vertex);
        }

        public void AddVertex(Point3D vertex)
        {
            //first check if the polygon is connected , i.e first vertex = last
            /*
            if (Vertices.Count > 1)
            {
                var first = Vertices.First();
                var second = Vertices.ElementAt(Vertices.Count - 1);              

                if (first.X != second.X && first.Y != second.Y)
                {
                    Vertices.Add(new Point3D(first.X, first.Y, 0));
                }
            }

            Vertices.Remove(Vertices.ElementAt(Vertices.Count - 1));
            */
            Vertices.Add(vertex);
            //complete connection
            //var duplicate = new Point3D(Vertices.First().X, Vertices.First().Y, 0);
            //Vertices.Add(duplicate);
           
        }

        public static LineOfSightArea LoadFileBinary(string file)
        {
            var los = new LineOfSightArea();

            FileStream fs = new FileStream(file.ToLower(), FileMode.Open);

            Stream s = fs;
            var header = los.ReadHeader(ref s);
            //p.ReadNodes(ref s, header);
            fs.Close();

            return los;
        }

        public static LineOfSightArea LoadFromStream(ref Stream stream)
        {
            var area = new LineOfSightArea();

            BinaryReader br = new BinaryReader(stream);
            int size = Marshal.SizeOf(typeof(Header));

            byte[] buffer = new byte[size];
            br.Read(buffer, 0, size);

            var header = Functions.ByteArrayToStructure<Header>(buffer);

            size = Marshal.SizeOf(typeof(Vertex));

            for (int i = 0; i < header.vertexCount; i++)
            {
                buffer = new byte[size];
                br.Read(buffer, 0, size);
                var vertex = Functions.ByteArrayToStructure<Vertex>(buffer); 
                
                //don't add the last one... as it's value needs to tbe the same as the first vertex
                if (i != header.vertexCount - 1)
                {
                    area.Vertices.Add(new Point3D(vertex.x, vertex.y, vertex.z));
                }
            }
            area._maxZ = header.maxZ;
            area._minZ = header.minZ;
            area._version = header.version;

            return area;
        }

        public bool GetClosestVertex( Point3D input, out Point3D output )
        {
            bool vertNear = false;
            output = new Point3D( 0, 0, 0 );            

            foreach ( Point3D point in Vertices ) {
                if ( Functions.Distance( new Point3D( point.X, point.Y, 0 ), new Point3D( input.X, input.Y, 0 ) ) < 5.0 ) {
                    output.X = point.X; 
                    output.Y = point.Y; 
                    output.Z = MaxZ;
                    vertNear = true;
                    break;
                }
            }

            return vertNear;
        }

        public void MoveVertex(Point3D vertex, Point3D location)
        {
            Point3D closest = new Point3D();
            if (GetClosestVertex(vertex, out closest))
            {
                closest.Z = 0;
                var index = Vertices.IndexOf(closest);

                location.Z = 0;
                Vertices[index] = location;
            }
        }

        private Header ReadHeader(ref Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            int size = Marshal.SizeOf(typeof(Header));
            
            byte[] buffer = new byte[size];
            br.Read(buffer,0,size);

            return Functions.ByteArrayToStructure<Header>(buffer);
        }

        public static LineOfSightArea LoadFileXml(string file)
        {
            var los = new LineOfSightArea();

            return los;
        }
    }
}
