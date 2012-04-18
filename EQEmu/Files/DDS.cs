using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace EQEmu.Files
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    /*
        http://msdn.microsoft.com/en-us/library/windows/desktop/bb943982(v=vs.85).aspx 
    */
    unsafe struct DDSHeader
    {
        public uint dwSize;
        public uint dwFlags;
        public uint dwHeight;
        public uint dwWidth;
        public uint dwPitchOrLinearSize;
        public uint dwDepth;
        public uint dwMipMapCount;
        public fixed uint dwReserved1[11];
        public DDSPixelFormat ddspf;
        public uint dwCaps;
        public uint dwCaps2;
        public uint dwCaps3;
        public uint dwCaps4;
        public uint dwReserved2;
    };

    /*
        http://msdn.microsoft.com/en-us/library/windows/desktop/bb943984(v=vs.85).aspx
     */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    struct DDSPixelFormat
    {
        public uint dwSize;
        public uint dwFlags;
        public uint dwFourCC;
        public uint dwRGBBitCount;
        public uint dwRBitMask;
        public uint dwGBitMask;
        public uint dwBBitMask;
        public uint dwABitMask;
    };

    public class DDS
    {
        private DDSHeader _header;

        private void DXT1(Stream stream)
        {
        }

        public static DDS Load(Stream stream)
        {
            DDS dds = new DDS();
            int magic;
            
            //read the magic number
            var barray = new byte[4];
            stream.Read(barray, 0, 4);
            magic = BitConverter.ToInt32(barray, 0);
            if (magic != 0x20534444)
            {
                throw new FileFormatException("DDS magic number did not match");
            }

            int size = Marshal.SizeOf( typeof(DDSHeader) );
            barray = new byte[size];
            stream.Read(barray, 0, size);

            var header = dds._header = Functions.ByteArrayToStructure<DDSHeader>(barray);

            if (dds._header.dwSize != 124)
            {
                throw new FileFormatException("DDS size (" + dds._header.dwSize.ToString() + ") is not 124");
            }

            /*
             DDPF_FOURCC	Texture contains compressed RGB data; dwFourCC contains valid data. 
             */
            if ((header.ddspf.dwFlags & 0x04) != 0)
            {
                var fourcc = header.ddspf.dwFourCC;

                var firstcc = (char)((byte)(header.ddspf.dwFourCC));
                var secondcc = (char)((byte)(header.ddspf.dwFourCC >> 8));
                var thirdcc = (char)((byte)(header.ddspf.dwFourCC >> 16));
                var fourthcc = (char)((byte)(header.ddspf.dwFourCC >> 24));

                switch (thirdcc)
                {
                    case 'T':
                        //DXT?
                        switch (fourthcc)
                        {
                            case '1':
                                dds.DXT1(stream);
                                break;
                        }

                        break;
                    case '1':
                        //dx10 probably
                        break;
                    default:
                        break;
                }

            }

            return dds;
        }
    }
}
