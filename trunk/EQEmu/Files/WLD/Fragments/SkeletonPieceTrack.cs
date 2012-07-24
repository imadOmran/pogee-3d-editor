using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EQEmu.Files.WLD.Fragments
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct Fragment12Struct
    {
        public int flags;
        public int size;
        public short rotateDenominator;
        public short rotateXNumerator;
        public short rotateYNumerator;
        public short rotateZNumerator;
        public short shiftXNumerator;
        public short shiftYNumerator;
        public short shiftZNumerator;
        public short shiftDenominator;
    };


    public class SkeletonPieceTrack : Fragment<Fragment12Struct>
    {
        private double _rotateX;
        private double _rotateY;
        private double _rotateZ;

        private double _rotateXDegrees;
        private double _rotateYDegrees;
        private double _rotateZDegrees;

        private double _shiftX;
        private double _shiftY;
        private double _shiftZ;

        public SkeletonPieceTrack(int num, int nameRef)
            : base(num, nameRef)
        {

        }

        public override void Handler(System.IO.Stream stream)
        {            
            base.Handler(stream);

            if (FragmentStruct.rotateDenominator != 0)
            {
                _rotateX = (double)FragmentStruct.rotateXNumerator / (double)FragmentStruct.rotateDenominator * Math.PI / 2;
                _rotateY = (double)FragmentStruct.rotateYNumerator / (double)FragmentStruct.rotateDenominator * Math.PI / 2;
                _rotateZ = (double)FragmentStruct.rotateZNumerator / (double)FragmentStruct.rotateDenominator * Math.PI / 2;

                _rotateXDegrees = _rotateX / 4 * 180;
                _rotateYDegrees = _rotateY / 4 * 180;
                _rotateZDegrees = _rotateZ / 4 * 180;
            }
            else
            {
                _rotateX = _rotateY = _rotateZ = 0;
                _rotateXDegrees = _rotateYDegrees = _rotateZDegrees = 0;
            }

            if (FragmentStruct.shiftDenominator != 0)
            {
                _shiftX = (double)FragmentStruct.shiftXNumerator / (double)FragmentStruct.shiftDenominator;
                _shiftY = (double)FragmentStruct.shiftYNumerator / (double)FragmentStruct.shiftDenominator;
                _shiftZ = (double)FragmentStruct.shiftZNumerator / (double)FragmentStruct.shiftDenominator;
            }
            else
            {
                _shiftX = _shiftY = _shiftZ = 0;
            }
        }

        public override string ToString()
        {
            return base.ToString() + " SkeletonPieceTrack " + this.FragmentName;
        }

        public double RotateX
        {
            get { return _rotateX; }            
        }

        public double RotateY
        {
            get { return _rotateY; }
        }

        public double RotateZ
        {
            get { return _rotateZ; }
        }

        public double RotateXDegrees
        {
            get { return _rotateXDegrees; }
        }

        public double RotateYDegrees
        {
            get { return _rotateYDegrees; }
        }

        public double RotateZDegrees
        {
            get { return _rotateZDegrees; }
        }

        public double ShiftX
        {
            get { return _shiftX; }
        }

        public double ShiftY
        {
            get { return _shiftY; }
        }

        public double ShiftZ
        {
            get { return _shiftZ; }
        }
        
    }
}
