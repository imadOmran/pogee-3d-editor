using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Drawing;

using FreeImageAPI;

using EQEmu.Files.WLD.Fragments;
using EQEmu.Files.S3D;

namespace EQEmu.Files.WLD
{
    public class WLD
    {
        private static byte[] _encarr = { 0x95, 0x3A, 0xC5, 0x2A, 0x95, 0x7A, 0x95, 0x6A };

        public static string DecodeFileName(byte[] name)
        {
            for (int i = 0; i < name.Count(); i++)
            {
                byte c = (byte)name[i];
                name[i] = (byte)(c ^ _encarr[i % 8]);
            }

            var encoding = new ASCIIEncoding();
            var str = encoding.GetString(name);
            return str.Substring(0, str.Length - 1);
        }

        private Dictionary<string, BitmapImage> _bitmaps = new Dictionary<string, BitmapImage>();
        private string[] _strings;
        private string _sHash;

        public enum Format
        {
            Old,
            New
        }

        private S3D.S3D _files = null;
        private Format _format;
        private List<object> _fragments = new List<object>();

        public Format FileVersion
        {
            get { return _format; }
        }

        public S3D.S3D Files
        {
            get { return _files; }
            set
            {
                _files = value;
                CreateBitmaps();
            }
        }

        public string StringHash
        {
            get { return _sHash; }
        }

        public string[] Strings
        {
            get { return _strings; }
        }

        public IEnumerable<Mesh> ZoneMeshes
        {
            get
            {
                return _fragments.Where(x => x as Mesh != null).Cast<Mesh>();
            }
        }

        public IEnumerable<ModelReference> Models
        {
            get
            {
                return _fragments.Where(x => x as ModelReference != null).Cast<ModelReference>();
            }
        }

        public IEnumerable<Texture> Textures
        {
            get
            {
                return _fragments.Where(x => x as Texture != null).Cast<Texture>();
            }
        }

        public IEnumerable<TextureList> TextureLists
        {
            get
            {
                return _fragments.Where(x => x as TextureList != null).Cast<TextureList>();
            }
        }

        public IEnumerable<BitmapInfoReference> BitmapInfoReferences
        {
            get
            {
                return _fragments.Where(x => x as BitmapInfoReference != null).Cast<BitmapInfoReference>();
            }
        }

        public IEnumerable<BitmapInfo> BitmapInfos
        {
            get
            {
                return _fragments.Where(x => x as BitmapInfo != null).Cast<BitmapInfo>();
            }
        }

        public IEnumerable<BitmapName> BitmapNames
        {
            get
            {
                return _fragments.Where(x => x as BitmapName != null).Cast<BitmapName>();
            }
        }

        public IEnumerable<ObjectLocation> ObjectLocations
        {
            get
            {
                return _fragments.Where(x => x as ObjectLocation != null).Cast<ObjectLocation>();
            }
        }

        public IEnumerable<MeshReference> MeshReferences
        {
            get
            {
                return _fragments.Where(x => x as MeshReference != null).Cast<MeshReference>();
            }
        }

        public IEnumerable<SkeletonTrackReference> SkeletonTrackReferences
        {
            get
            {
                return _fragments.Where(x => x as SkeletonTrackReference != null).Cast<SkeletonTrackReference>();
            }
        }

        public IEnumerable<SkeletonTrackSet> SkeletonTrackSet
        {
            get
            {
                return _fragments.Where(x => x as SkeletonTrackSet != null).Cast<SkeletonTrackSet>();
            }
        }

        public IEnumerable<SkeletonPieceTrackReference> SkeletonPieceReferences
        {
            get
            {
                return _fragments.Where(x => x as SkeletonPieceTrackReference != null).Cast<SkeletonPieceTrackReference>();
            }
        }

        public IEnumerable<SkeletonPieceTrack> SkeletonPieces
        {
            get
            {
                return _fragments.Where(x => x as SkeletonPieceTrack != null).Cast<SkeletonPieceTrack>();
            }
        }

        public string GetStringAtHashIndex(int num)
        {
            string val = "";
            int index = num;
            char c;

            if ( index < _sHash.Count() && _sHash.ElementAt(index) == '\0')
            {
                index++;
            }

            while ( index < _sHash.Count() && (c = _sHash.ElementAt(index++)) != '\0')
            {
                val += c;
            }

            return val;
        }

        private void CreateBitmaps()
        {
            List<string> fileNames = new List<string>();
            foreach (var m in ZoneMeshes)
            {
                foreach (var pt in m.PolygonTextures)
                {
                    //var textIndex = TextureLists.ElementAt(0).TextureReference.ElementAt(pt.texIndex);
                    var textIndex = TextureLists.FirstOrDefault(x => x.FragmentNumber == m.FragmentReference).TextureReference.ElementAt(pt.texIndex);
                    var texture = Textures.FirstOrDefault(x => x.FragmentNumber == textIndex);
                    if (texture == null) continue;

                    var infoRef = BitmapInfoReferences.FirstOrDefault(x => x.FragmentNumber == texture.FragmentReference);
                    if (infoRef == null) continue;

                    var bmpInfo = BitmapInfos.FirstOrDefault(x => x.FragmentNumber == infoRef.FragmentReference);
                    if (bmpInfo == null) continue;

                    List<string> names = new List<string>();
                    foreach (var info in bmpInfo.FragmentReferences)
                    {
                        var name = BitmapNames.FirstOrDefault(x => x.FragmentNumber == info);
                        //var name = BitmapNames.ElementAt(info);
                        if (name == null || fileNames.Exists(x => x == name.File)) continue;
                        fileNames.Add(name.File);
                    }

                    //now create the bitmaps
                }
            }

            foreach (var f in fileNames.Where(x => x.ToLower().Contains(".bmp")))
            {
                var img = Files.Files.FirstOrDefault(x => x.Name == f.ToLower());
                if (img == null) continue;
                var bmp = new BitmapImage();
                bmp.CacheOption = BitmapCacheOption.OnLoad;

                var ms = new MemoryStream(img.Bytes);
                bmp.BeginInit();
                bmp.StreamSource = ms;
                bmp.EndInit();
                bmp.Freeze();
                _bitmaps[f.ToLower()] = bmp;
            }

            foreach (var f in fileNames.Where(x => x.ToLower().Contains(".dds")))
            {
                var tmpdir = Environment.GetEnvironmentVariable("TEMP") + "\\tmpimages";
                Directory.CreateDirectory(tmpdir);

                var img = Files.Files.FirstOrDefault(x => x.Name == f.ToLower());
                if (img == null) continue;

                var bytes = img.Bytes;

                using (var ms = new MemoryStream(bytes))
                {
                    var converted = new MemoryStream();
                    var dib = FreeImage.LoadFromStream(ms);
                    FreeImage.FlipVertical(dib);
                    FreeImage.SaveToStream(dib, converted, FREE_IMAGE_FORMAT.FIF_BMP);

                    converted.Seek(0, SeekOrigin.Begin);
                    var bmp = new BitmapImage();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.BeginInit();
                    bmp.StreamSource = converted;
                    bmp.EndInit();
                    bmp.Freeze();
                    _bitmaps[f.ToLower()] = bmp;
                }
            }

            foreach (var m in ZoneMeshes)
            {
                foreach (var p in m.Polygons)
                {
                    //p.Image = GetImage(TextureLists.ElementAt(0), p.TextureListIndex);
                    var tlist = TextureLists.FirstOrDefault(x => x.FragmentNumber == m.FragmentReference);
                    if (tlist == null) continue;
                    p.Image = GetImage(tlist, p.TextureListIndex);
                }
            }
        }

        public BitmapImage GetImage(TextureList list, int index)
        {
            var textIndex = list.TextureReference.ElementAt(index);
            var texture = Textures.FirstOrDefault(x => x.FragmentNumber == textIndex);
            if (texture == null) return null;

            var infoRef = BitmapInfoReferences.FirstOrDefault(x => x.FragmentNumber == texture.FragmentReference);
            if (infoRef == null) return null;

            var bmpInfo = BitmapInfos.FirstOrDefault(x => x.FragmentNumber == infoRef.FragmentReference);
            if (bmpInfo == null) return null;

            if (bmpInfo.FragmentReferences.Count > 0)
            {
                var name = BitmapNames.FirstOrDefault(x => x.FragmentNumber == bmpInfo.FragmentReferences.ElementAt(0));
                if (name == null) return null;

                if (_bitmaps.ContainsKey(name.File.ToLower()))
                {
                    return _bitmaps[name.File.ToLower()];
                }
                else return null;
            }
            else return null;
        }

        /*
        private void Fragment22Handler(Stream stream)
        {
            int size = Marshal.SizeOf(typeof(Fragment22));
            var barray = new byte[size];

            stream.Read(barray, 0, size);
            var frag = Functions.ByteArrayToStructure<Fragment22>(barray);

            stream.Seek((12 * frag.size1) + (8 * frag.size2) + ((frag.size5) * 7 * 4), SeekOrigin.Current);
            
            size = Marshal.SizeOf( typeof(short) );
            barray = new byte[size];
            stream.Read(barray, 0, size);
            var d6size = BitConverter.ToInt16(barray, 0);            

            // PVS
            int rp = 0;
            int sz = d6size;
            int current_id = 0;
            while (rp < sze)
            {
                var c = d6size;

                switch (c)
                {
                    case 0x3E:
                        current_id += c;
                        break;
                    case 0x3F:
                        if (rp + 2 > sz) throw new Exception("Invalid data in fragment22 handler");
                        
                        stream.Read(barray, 0, size);
                        var s1 = BitConverter.ToInt16(barray, 0);
                        stream.Read(barray, 0, size);
                        var s2 = BitConverter.ToInt16(barray, 0);

                        int count = s1 + (s2 << 8);
                        current_id += count;
                        break;
                    case 0x7F:
                        int keep = (c & 7);
                        int skip = (c >> 3) & 7;
                        current_id += skip;
                        for (int i = 0; i < keep; i++)
                        {
                            //Region.Visibility.Add(id)
                        }
                        break;
                }

                stream.Read(barray, 0, size);
                d6size = BitConverter.ToInt16(barray, 0);
            }
        }*/

        public void ResolveMeshNames()
        {
            foreach (var m in this.ZoneMeshes.Where( x => x.FragmentNameRef < 0 ))
            {
                m.FragmentName = GetStringAtHashIndex(Math.Abs(m.FragmentNameRef));
            }
        }

        public void ResolveObjectLocationNames()
        {
            foreach (var m in ObjectLocations.Where(x => x.HasFragmentReference))
            {
                m.ReferencedName = GetStringAtHashIndex(Math.Abs(m.FragmentReference));
            }
        }

        public static WLD Load(Stream stream)
        {
            WLD wld = new WLD();
            int size = Marshal.SizeOf(typeof(WLDHeader));
            var barray = new byte[size];
            stream.Read(barray, 0, size);

            var header = Functions.ByteArrayToStructure<WLDHeader>(barray);

            if (header.Magic != 0x54503d02)
            {
                throw new Exception("Invalid file format");
            }

            if (header.Version == 0x00015500)
            {
                wld._format = Format.Old;
            }
            else if (header.Version == 0x1000C800)
            {
                wld._format = Format.New;
            }
            else
            {
                throw new Exception("Unknown file version");
            }

            //var shash = stream.Position;
            barray = new byte[header.StringHashSize];
            stream.Read(barray, 0, (int)header.StringHashSize);
            wld._sHash = WLD.DecodeFileName(barray);
            wld._strings = wld._sHash.Split('\0');

            var fragCount = header.FragmentCount;

            stream.Seek(size + header.StringHashSize, SeekOrigin.Begin);

            int fragSize = Marshal.SizeOf(typeof(BasicWLDFragment));

            for (int i = 0; i < header.FragmentCount; i++)
            {
                barray = new byte[fragSize];
                stream.Read(barray, 0, fragSize);
                var fragment = Functions.ByteArrayToStructure<BasicWLDFragment>(barray);
                int nameRef = (int)fragment.NameRef;

                var position = stream.Position;

                switch (fragment.Id)
                {
                    case 0x03:
                        var bmpname = new BitmapName(i, nameRef);
                        bmpname.Handler(stream);
                        wld._fragments.Add(bmpname);
                        break;
                    case 0x04:
                        var binfo = new BitmapInfo(i, nameRef);
                        binfo.Handler(stream);
                        wld._fragments.Add(binfo);
                        break;
                    case 0x05:
                        var bitmapInfoRef = new BitmapInfoReference(i, nameRef);
                        bitmapInfoRef.Handler(stream);
                        wld._fragments.Add(bitmapInfoRef);
                        break;
                    case 0x09:
                        break;
                    case 0x10:
                        var skelset = new SkeletonTrackSet(i, nameRef);
                        skelset.Handler(stream);
                        skelset.FragmentName = wld.GetStringAtHashIndex(0-skelset.FragmentNameRef);
                        wld._fragments.Add(skelset);
                        break;
                    case 0x11:
                        var skeltrackRef = new SkeletonTrackReference(i, nameRef);
                        skeltrackRef.Handler(stream);                        
                        wld._fragments.Add(skeltrackRef);
                        break;
                    case 0x12:
                        var skelpiece = new SkeletonPieceTrack(i, nameRef);
                        skelpiece.Handler(stream);
                        wld._fragments.Add(skelpiece);
                        break;
                    case 0x13:
                        var skelpref = new SkeletonPieceTrackReference(i, nameRef);
                        skelpref.Handler(stream);
                        //skelpref.FragmentName = wld.GetStringAtHashIndex(0 - skelpref.FragmentNameRef);
                        wld._fragments.Add(skelpref);
                        break;
                    case 0x14:
                        var modelref = new ModelReference(i, nameRef);
                        modelref.Handler(stream);
                        modelref.FragmentName = wld.GetStringAtHashIndex(0 - modelref.FragmentNameRef);
                        wld._fragments.Add(modelref);
                        modelref.MagicString = wld.GetStringAtHashIndex(modelref.MagicStringRef);
                        break;
                    case 0x15:
                        var objlocation = new ObjectLocation(i, nameRef);
                        objlocation.Handler(stream);
                        wld._fragments.Add(objlocation);
                        break;
                    case 0x22:
                        //num_0x22++;
                        break;
                    case 0x2d:
                        var meshref = new MeshReference(i, nameRef);
                        meshref.Handler(stream);
                        wld._fragments.Add(meshref);
                        break;
                    case 0x31:
                        var tlist = new TextureList(i, nameRef);
                        tlist.Handler(stream);
                        wld._fragments.Add(tlist);
                        break;
                    case 0x30:
                        var texture = new Texture(i, nameRef);
                        texture.Handler(stream);
                        wld._fragments.Add(texture);
                        break;
                    // Grab the number of vertices and polygons
                    case 0x36:
                        var mesh = new Mesh(i, nameRef);
                        mesh.Handler(stream);
                        wld._fragments.Add(mesh);
                        break;
                }
                stream.Seek(position + fragment.Size - 4, SeekOrigin.Begin);
            }

            return wld;
        }
    }
}
