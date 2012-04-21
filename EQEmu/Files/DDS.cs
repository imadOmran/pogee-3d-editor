using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Media.Imaging;

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

    public class Rgb565
    {
        private int _red;
        private int _green;
        private int _blue;

        public Rgb565(short colors)
        {
            _red = (colors >> 11) & 0x1f;
            _green = (colors >> 5) & 0x3f;
            _blue = colors & 0x1f;
        }

        public int Red
        {
            get { return _red; }
        }

        public int Green
        {
            get { return _green; }
        }

        public int Blue
        {
            get { return _blue; }
        }
    }

    public class DTX1Block
    {
        private DTX1Palette _palette;
        private byte[] _pixels;

        public DTX1Block(DTX1Palette palette, byte[] pixels)
        {
            _palette = palette;
            _pixels = pixels;
        }

        public Rgb565 GetPixel(int num)
        {
            Rgb565 color = null;

            var pixindex = num % 16;
            //which byte is needed (theres 16 in a block).. 2 bits per pixel
            //byte 1 has pixels 0 - 3
            //byte 2 has pixels 4 - 7
            //byte 3 has pixels 8 - 11
            //byte 4 has pixels 12 - 15
            var bindex = pixindex / 4;
            var b = _pixels[bindex];

            var palindex = (b >> (pixindex % 4)) & 0x03;

            switch (palindex)
            {
                case 0:
                    color = _palette.Color0;
                    break;
                case 1:
                    color = _palette.Color1;
                    break;
                case 2:
                    color = _palette.Color2;
                    break;
                case 3:
                    color = _palette.Color3;
                    break;
                default:
                    throw new Exception("invalid palette index");
            }

            return color;
        }
    }

    public class DTX1Palette
    {
        private Rgb565[] _colors = new Rgb565[4];

        public DTX1Palette(byte[] c0, byte[] c1)
        {
            var color0 = BitConverter.ToInt16(c0, 0);
            var color1 = BitConverter.ToInt16(c1, 0);

            _colors[0] = new Rgb565(color0);
            _colors[1] = new Rgb565(color1);

            short color2 = 0, color3 = 0;
            if (color0 > color1)
            {
                color2 = (short)(((color0 * 2 / 3) + (color1 / 3)));
                color3 = (short)(((color0 / 3) + (color1 * 2 / 3)));
            }
            else
            {
                color2 = (short)((color0 / 2) + (color1 * 2));
                color3 = 0;
            }

            _colors[2] = new Rgb565(color2);
            _colors[3] = new Rgb565(color3);
        }

        public Rgb565 Color0
        {
            get { return _colors[0]; }
        }

        public Rgb565 Color1
        {
            get { return _colors[1]; }
        }

        public Rgb565 Color2
        {
            get { return _colors[2]; }
        }

        public Rgb565 Color3
        {
            get { return _colors[3]; }
        }
    }


    public class DDS
    {
        private DDSHeader _header;
        private Bitmap _bitmap;

        public Bitmap Bitmap
        {
            get { return _bitmap; }
        }

        private void DXT1(Stream stream)
        {
            var pixelW = _header.dwWidth;
            var pixelH = _header.dwHeight;

            var rows = pixelW / 4;
            var cols = pixelH / 4;
            var paleteCount = rows * cols;

            byte[] color1 = new byte[2];
            byte[] color2 = new byte[2];
            var pixelblock = new byte[4];

            List<DTX1Block> blocks = new List<DTX1Block>();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    stream.Read(color1, 0, 2);
                    stream.Read(color2, 0, 2);

                    var palette = new DTX1Palette(color1, color2);
                    stream.Read(pixelblock, 0, 4);
                    var block = new DTX1Block(palette, pixelblock);
                    blocks.Add(block);
                }
            }

            //lay the pixels out in a bitmap fashion
            int[] pixels = new int[pixelW * pixelH * 4];
            Color[] cpixels = new Color[pixelW * pixelH];
            var bmp = new Bitmap((int)pixelW, (int)pixelH);

            //var bsource = System.Windows.Media.Imaging.BitmapSource.Create(pixelW,pixelH,96,96, System.Windows.Media.PixelFormats.Rgb24,null,

            /*
            for (int i = 0; i < pixelH; i++)
            {
                for(int j = 0; j < pixelW; j++)
                {
                    switch(i%5)
                    {
                    case 0:
                        bmp.SetPixel(j, i, Color.Blue);
                        break;
                    case 1:
                        bmp.SetPixel(j, i, Color.Green);
                        break;
                    case 2:
                        bmp.SetPixel(j, i, Color.White);
                        break;
                    case 3:
                        bmp.SetPixel(j, i, Color.Black);
                        break;
                    case 4:
                        bmp.SetPixel(j, i, Color.Red);
                        break;                    
                    }
                }
            }*/

            
            for (int i = 0; i < pixelH; i++)
            {
                var offset = (int)(i * pixelH); //* 4;                
                for (int j = 0; j < pixelW; j++)
                {
                    int blockoffset = i / 4;
                    var block = blocks.ElementAt(blockoffset + (j / 4 + i / 4));

                    //which pixel in block is needed
                    int p = (i % 4) * 4 + j % 4;
                    var c = block.GetPixel(p);

                    //pixels[j * offset]
                    var color = Color.FromArgb(0,c.Red,c.Green,c.Blue);
                    //var color = Color.FromArgb(0, 250, 0, 0);
                    //cpixels[offset+j] = Color.FromArgb(255,c.Red,c.Green,c.Blue);
                    bmp.SetPixel(j, i, color);
                }
            }            
            _bitmap = bmp;           
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

            int size = Marshal.SizeOf(typeof(DDSHeader));
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
