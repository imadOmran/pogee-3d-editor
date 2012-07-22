using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EQEmu.Files.WLD.Fragments
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct Fragment2DStruct
    {
        public int params1;
    };

    public class MeshReference : Fragment<Fragment2DStruct>
    {
        private int _fragmentRef;

        public MeshReference(int num, int nameRef)
            : base(num, nameRef)
        {

        }

        public int FragmentReference
        {
            get { return _fragmentRef; }
        }

        public override string ToString()
        {
            return base.ToString() + " MeshReference " + FragmentName;
        }

        public override void Handler(System.IO.Stream stream)
        {
            var barray = new byte[4];
            stream.Read(barray, 0, 4);            
            _fragmentRef = BitConverter.ToInt32(barray, 0) - 1;
            base.Handler(stream);
        }
    }
}
