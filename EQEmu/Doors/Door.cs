using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Doors
{
        public class Door : DatabaseObject
        {
            public Door(QueryConfig config)
                :base(config)
            {

            }

            //http://www.eqemulator.net/wiki/wikka.php?wakka=OpenType
            public enum OpenTypes
            {
                Normal90DegreeDoorSwingBackward_0 = 0,
                Normal90DegreeDoorSwingBackward_1 = 1,
                Normal90DegreeDoorSwingBackward_2 = 2,
                Normal90DegreeDoorSwingBackward_3 = 3,
                Normal90DegreeDoorSwingBackward_4 = 4,

                Normal90DegreeDoorSwingForward_0 = 5,
                Normal90DegreeDoorSwingForward_1 = 6,
                Normal90DegreeDoorSwingForward_2 = 7,

                Normal90DegreeDoorSwingBackward_5 = 8,

                SlidesForward_0 = 10,
                SlidesForward_1 = 11,
                SlidesForward_2 = 12,

                SlidesFurtherForward_0 = 15,
                SlidesFurtherForward_1 = 16,
                SlidesFurtherForward_2 = 17,

                SlidesEvenFurtherForward_0 = 20,
                SlidesEvenFurtherForward_1 = 21,
                SlidesEvenFurtherForward_2 = 22,

                SlidesFurthestFoward_0 = 25,
                SlidesFurthestFoward_1 = 26,
                SlidesFurthestFoward_2 = 27,

                Rotates90DegreesClockwiseAndReturns = 30,

                Rotates90DegreesClockwiseAndReturnsFaster = 35,

                Rotates90DegreesAndJumpsBack = 36,

                Rotates90DegreesClockwiseAndReturnsSlower = 40,

                SlideSidewaysOpenAndClosesSlowly = 45,

                NoDoorShowingInvisible_0 = 50,
                NoDoorShowingInvisible_1 = 53,
                NoDoorShowingInvisible_2 = 54,

                ClickablePortal = 58,

                VerticalLiftLow_0 = 60,
                VerticalLiftLow_1 = 61,
                VerticalLiftLow_2 = 62,

                VerticalLiftExtremelyHigh_0 = 63,
                VerticalLiftExtremelyHigh_1 = 64,

                VerticalLiftLowMidHeight_0 = 65,
                VerticalLiftLowMidHeight_1 = 66,
                VerticalLiftLowMidHeight_2 = 67,

                VerticalLiftExtremelyHigh_2 = 68,
                VerticalLiftExtremelyHigh_3 = 69,

                VerticalLiftMidHeight_0 = 70,
                VerticalLiftMidHeight_1 = 71,
                VerticalLiftMidHeight_2 = 72,

                VerticalLiftExtremelyHigh_4 = 73,
                VerticalLiftExtremelyHigh_5 = 74,

                VerticalLiftHighHeight_0 = 75,
                VerticalLiftHighHeight_1 = 76,
                VerticalLiftHighHeight_2 = 77,

                MovesSlightlyForward_0 = 78,

                MovesSlightlyBackward_0 = 79,

                MovesSlightlyForward_1 = 80,

                MovesSlightlyBackwards_1 = 81,

                RevolvingDoorCounterClockwise = 100,

                RevolvingDoorCounterClockwiseFast = 101,

                RevolvingDoorCounterClockwiseFaster = 102,

                ContinousCounterClockwiseRotation = 106,

                ContinousCounterClockwiseRotationFaster = 107,

                SlideSidewaysOpenAndCloseImmediately = 109,

                //The following all need invert_state to 1 to work correctly. If invert_state is 0, they don't move but you take damage still if you touch them (Thanks Qadar, for this information):

                SpinNonstop4SawDamage = 115,
                SpinPause4SawDamage = 116,
                MovesDownThenUp30SpearDamage = 120,
                MovesLeftThenRight30SpearDamage = 125,
                SwingsBackAndForth4PendulumDamage = 130,
                NoMovement4BladeDamage = 135,
                MovesUpAndDown4CrushDamage = 140,
                MovesUpThenDownNonstop_0 = 142,
                MovesUpThenDownNonstop_1 = 143,
                MovesUpThenDownNonstop_2 = 144,
                DownThenUpSlow10CrushDamage = 145,
                DownThenUpFast50CrushDamage = 146,
                SlideFastLeftBackSlowly10CrushDamage = 150,
                SlideSlowLeftBackSlowly10CrushDamage = 151,
                SlideFastLeftBackQuickly50CrushDamage = 152
            }

            private string _zone;
            public string Zone
            {
                get { return _zone; }
                set { _zone = value; }
            }

            private string _name;
            public string Name
            {
                get
                {
                    return _name;
                }
                set
                {
                    _name = value;
                    Dirtied();
                }
            }

            private int _id;
            public int Id
            {
                get { return _id; }
                set
                {
                    _id = value;
                    Dirtied();
                }
            }

            private int _doorId;
            public int DoorId
            {
                get
                {
                    return _doorId;
                }
                set
                {
                    _doorId = value;
                }
            }

            private float _heading;
            public float Heading
            {
                get
                {
                    return _heading;
                }
                set
                {
                    _heading = value;
                    Dirtied();
                }
            }

            public float HeadingDegrees
            {
                get
                {
                    return _heading / 512 * 360;
                }
                set
                {
                    _heading = value / 360 * 512;
                    Dirtied();
                }
            }
            
            private OpenTypes _openType;
            public OpenTypes OpenType
            {
                get
                {
                    return _openType;
                }
                set
                {
                    _openType = value;
                    Dirtied();
                }
            }

            private int _guild;
            public int Guild
            {
                get
                {
                    return _guild;
                }
                set
                {
                    _guild = value;
                    Dirtied();
                }
            }

            private int _lockPick;
            public int LockPick
            {
                get
                {
                    return _lockPick;
                }
                set
                {
                    _lockPick = value;
                    Dirtied();
                }
            }

            private int _keyItem;
            public int KeyItem
            {
                get
                {
                    return _keyItem;
                }
                set
                {
                    _keyItem = value;
                    Dirtied();
                }
            }

            private uint _numKeyRing;
            public uint NumKeyRing
            {
                get
                {
                    return _numKeyRing;
                }
                set
                {
                    _numKeyRing = value;
                    Dirtied();
                }
            }

            private int _triggerDoor;
            public int TriggerDoor
            {
                get
                {
                    return _triggerDoor;
                }
                set
                {
                    _triggerDoor = value;
                    Dirtied();
                }
            }

            private int _triggerType;
            public int TriggerType
            {
                get
                {
                    return _triggerType;
                }
                set
                {
                    _triggerType = value;
                    Dirtied();
                }
            }

            private int _doorIsOpen;
            public int DoorIsOpen
            {
                get
                {
                    return _doorIsOpen;
                }
                set
                {
                    _doorIsOpen = value;
                    Dirtied();
                }
            }

            private int _doorParam;
            public int DoorParam
            {
                get
                {
                    return _doorParam;
                }
                set
                {
                    _doorParam = value;
                    Dirtied();
                }
            }

            private string _destinationZone;
            public string DestinationZone
            {
                get
                {
                    return _destinationZone;
                }
                set
                {
                    _destinationZone = value;
                    Dirtied();
                }
            }

            private int _destinationInstance;
            public int DestinationInstance
            {
                get { return _destinationInstance; }
                set 
                { 
                    _destinationInstance = value;
                    Dirtied();
                }
            }

            private float _destinationHeading;
            public float DestinationHeading
            {
                get { return _destinationHeading; }
                set 
                { 
                    _destinationHeading = value;
                    Dirtied();
                }
            }

            private int _invertState;
            public int InvertState
            {
                get { return _invertState; }
                set 
                { 
                    _invertState = value;
                    Dirtied();
                }
            }

            private int _incline;
            public int Incline
            {
                get { return _incline; }
                set 
                { 
                    _incline = value;
                    Dirtied();
                }
            }

            private int _size;
            public int Size
            {
                get { return _size; }
                set { 
                    _size = value;
                    Dirtied();
                }
            }

            private float _buffer;
            public float Buffer
            {
                get { return _buffer; }
                set 
                {
                    _buffer = value;
                    Dirtied();
                }
            }

            private int _clientVersionMask;
            public int ClientVersionMask
            {
                get { return _clientVersionMask; }
                set 
                { 
                    _clientVersionMask = value;
                    Dirtied();
                }
            }

            private int _isLdonDoor;
            public int IsLdonDoor
            {
                get { return _isLdonDoor; }
                set 
                { 
                    _isLdonDoor = value;
                    Dirtied();
                }
            }

            private float _x;
            public float X
            {
                get { return _x; }
                set
                {
                    _x = value;
                    Dirtied();
                }
            }

            private float _y;
            public float Y
            {
                get { return _y; }
                set
                {
                    _y = value;
                    Dirtied();
                }
            }

            private float _z;
            public float Z
            {
                get { return _z; }
                set
                {
                    _z = value;
                    Dirtied();
                }
            }

            private int _version;
            public int Version
            {
                get { return _version; }
                set
                {
                    _version = value;
                    Dirtied();
                }
            }

            public void LookAt(Point3D p)
            {
                double a = p.X - this.X;
                double b = p.Y - this.Y;
                double degrees = Math.Atan(b / a) * 180 / Math.PI;

                if (a == 0)
                {
                    if (b > 0) degrees = 45.0;
                    else if (b < 0) degrees = 270.0;
                    else degrees = 0.0;
                }
                else if (b == 0)
                {
                    if (a > 0) degrees = 0.0;
                    else if (a < 0) degrees = 180.0;
                    else degrees = 0.0;
                }
                else
                {
                    if (a > 0)
                    {
                        if (b > 0)
                        {
                            //quadrant 1                   
                            degrees = (90 - degrees) % 360;
                        }
                        else if (b < 0)
                        {
                            //quadrant 4
                            degrees = (90 - degrees) % 360;
                        }
                    }
                    else
                    {
                        if (b > 0)
                        {
                            //quadrant 2
                            degrees = (270 - degrees) % 360;
                        }
                        else
                        {
                            //quadrant 3
                            //degrees = (180 + degrees) % 360;
                            degrees = (270 - degrees) % 360;
                        }
                    }
                }
                degrees += 90;
                this.HeadingDegrees = (float)degrees;
            }

            //public override string InsertString
            //{
            //    get { throw new NotImplementedException(); }
            //}

            //public override string UpdateString
            //{
            //    get { throw new NotImplementedException(); }
            //}

            //public override string DeleteString
            //{
            //    get { throw new NotImplementedException(); }
            //}
        }
}
