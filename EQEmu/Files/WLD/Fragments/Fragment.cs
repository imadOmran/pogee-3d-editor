﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace EQEmu.Files.WLD.Fragments
{
    public class Fragment<T> where T : struct
    {
        private readonly int _num;
        private readonly int _nameRef;
        private T _fragment;

        public Fragment(int num,int nameRef)
        {
            _num = num;
            _nameRef = nameRef;
        }

        protected T FragmentStruct
        {
            get { return _fragment; }
        }

        public int FragmentNumber
        {
            get { return _num; }
        }

        public int FragmentNameRef
        {
            get { return _nameRef; }
        }

        public string FragmentName
        {
            get;
            set;
        }

        public virtual void Handler(Stream stream)
        {
            int size = Marshal.SizeOf(typeof(T));
            var barray = new byte[size];
            stream.Read(barray, 0, size);
            _fragment = Functions.ByteArrayToStructure<T>(barray);            
        }

        public override string ToString()
        {
            return "Fragment:"+FragmentNumber.ToString();
        }
    }    
}
