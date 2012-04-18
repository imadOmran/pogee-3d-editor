using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

using zlib;

namespace EQEmu.Files.S3D
{
    public class S3D
    {
        private S3D()
        {

        }

        private IEnumerable<ArchiveFile> _files;

        public IEnumerable<ArchiveFile> Files
        {
            get { return _files; }
        }

        private void Decompress(byte[] deflated, int deflatedLen, byte[] inflated, int inflatedLen)
        {
            ZStream stream = new ZStream();
            stream.next_in = deflated;
            stream.avail_in = deflatedLen;

            stream.next_out = inflated;
            stream.avail_out = inflatedLen;

            stream.inflateInit();
            stream.inflate(zlibConst.Z_NO_FLUSH);
            stream.inflateEnd();
        }

        private void DecompressFile(ArchiveFile file, Stream stream)
        {
            int totalInflated = 0;
            int size = Marshal.SizeOf(typeof(PFSData));
            int offset = (int)file.Meta.Offset;
            List<byte> inflatedBytes = new List<byte>();

            stream.Seek(offset, SeekOrigin.Begin);

            while (totalInflated < file.Meta.Size)
            {
                var barray = new byte[size];
                stream.Read(barray, 0, size);

                var data = Functions.ByteArrayToStructure<PFSData>(barray);

                barray = new byte[data.DeflatedLength];
                stream.Read(barray, 0, (int)data.DeflatedLength);

                var inflated = new byte[data.InflatedLength];
                Decompress(barray, (int)data.DeflatedLength, inflated, (int)data.InflatedLength);
                inflatedBytes.AddRange(inflated);

                totalInflated += (int)data.InflatedLength;
            }
            file.Bytes = inflatedBytes.ToArray();
        }

        public static S3D Load(string file)
        {
            S3D s3d = null;

            //var fileInfo = new FileInfo(file);
            //var s3dFile = new byte[fileInfo.Length];

            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                int size = Marshal.SizeOf(typeof(PFSHeader));
                var barray = new byte[size];

                fs.Read(barray, 0, size);
                PFSHeader header = Functions.ByteArrayToStructure<PFSHeader>(barray);

                unsafe
                {
                    if (header.MagicCookie[0] != 'P' && header.MagicCookie[1] != 'F' && header.MagicCookie[2] != 'S' && header.MagicCookie[3] != ' ')
                    {
                        throw new ArgumentException();
                    }
                }


                int uint32size = Marshal.SizeOf(typeof(UInt32));

                //determine the file count
                fs.Seek(header.Offset, SeekOrigin.Begin);
                barray = new byte[uint32size];
                fs.Read(barray, 0, uint32size);

                int filecount = BitConverter.ToInt32(barray, 0);
                //var files = new ArchiveFile[filecount];
                var files = new List<ArchiveFile>();
                size = Marshal.SizeOf(typeof(PFSMeta));

                ArchiveFile directory = new ArchiveFile();

                for (int i = 0; i < filecount; i++)
                {
                    barray = new byte[size];
                    fs.Read(barray, 0, size);
                    var block = Functions.ByteArrayToStructure<PFSMeta>(barray);

                    var f = new ArchiveFile();
                    f.Meta = block;

                    if (block.CRC == 0x61580AC9)
                    {
                        directory = f;
                    }
                    else
                    {
                        files.Add(f);
                    }
                }

                if (directory.Meta.Offset == 0 || directory.Meta.Size == 0) throw new Exception("Could not find directory in archive");

                fs.Seek(directory.Meta.Offset, SeekOrigin.Begin);
                size = Marshal.SizeOf(typeof(PFSData));
                barray = new byte[size];
                fs.Read(barray, 0, size);

                var data = Functions.ByteArrayToStructure<PFSData>(barray);

                var deflatedBuffer = new byte[data.DeflatedLength];
                var inflatedBuffer = new byte[data.InflatedLength];

                fs.Read(deflatedBuffer, 0, (int)data.DeflatedLength);

                s3d = new S3D();
                s3d.Decompress(deflatedBuffer, (int)data.DeflatedLength, inflatedBuffer, (int)data.InflatedLength);

                if (directory.Meta.Size != data.InflatedLength)
                {
                    throw new Exception("Directory size mismatch");
                }

                List<string> fileNames = new List<string>();
                using (var ms = new MemoryStream(inflatedBuffer))
                {
                    ms.Seek(8, SeekOrigin.Begin);
                    var encoding = new ASCIIEncoding();
                    for (int i = 0; i < filecount - 1; i++)
                    {
                        int val = -1;
                        List<byte> namechars = new List<byte>();

                        while (val != 0)
                        {
                            val = ms.ReadByte();

                            if (val != 0)
                            {
                                namechars.Add((byte)val);
                            }
                            else
                            {
                                fileNames.Add(encoding.GetString(namechars.ToArray()));
                                ms.Seek(4, SeekOrigin.Current);
                                break;
                            }
                        }
                    }
                }

                s3d._files = files.OrderBy(x => x.Meta.Offset);
                for (int i = 0; i < s3d._files.Count(); i++)
                {
                    s3d._files.ElementAt(i).Name = fileNames.ElementAt(i).ToLower();
                    s3d.DecompressFile(s3d._files.ElementAt(i), fs);
                }
            }
            return s3d;
        }
    }
}
