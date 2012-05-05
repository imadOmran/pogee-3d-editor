using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Zone
{
    public class ZonePoint : Database.DatabaseObject
    {
        private int _id;
        private int _version;
        private int _number;
        private int _clientVersionMask;
        private int _targetZoneId;
        private int _targetInstance;

        private string _zone;

        private float _x;
        private float _y;
        private float _z;
        private float _targetX;
        private float _targetY;
        private float _targetZ;
        private float _heading;
        private float _targetHeading;
        private float _buffer;

        private ZonePoint()
            : base(null)
        {

        }

        public ZonePoint(QueryConfig config)
            : base(config)
        {

        }

        public float Buffer
        {
            get { return _buffer; }
            set
            {
                _buffer = value;
                Dirtied();
            }
        }

        public float TargetHeading
        {
            get { return _targetHeading; }
            set
            {
                _targetHeading = value;
                Dirtied();
            }
        }

        public float Heading
        {
            get { return _heading; }
            set
            {
                _heading = value;
                Dirtied();
            }
        }

        public float X
        {
            get { return _x; }
            set
            {
                _x = value;
                Dirtied();
            }
        }

        public float Y
        {
            get { return _y; }
            set
            {
                _y = value;
                Dirtied();
            }
        }

        public float Z
        {
            get { return _z; }
            set
            {
                _z = value;
                Dirtied();
            }
        }

        public float TargetX
        {
            get { return _targetX; }
            set
            {
                _targetX = value;
                Dirtied();
            }
        }

        public float TargetY
        {
            get { return _targetY; }
            set
            {
                _targetY = value;
                Dirtied();
            }
        }

        public float TargetZ
        {
            get { return _targetZ; }
            set
            {
                _targetZ = value;
                Dirtied();
            }
        }

        public string Zone
        {
            get { return _zone; }
            set
            {
                _zone = value;
                Dirtied();
            }
        }

        public int TargetInstance
        {
            get { return _targetInstance; }
            set
            {
                _targetInstance = value;
                Dirtied();
            }
        }

        public int TargetZoneId
        {
            get { return _targetZoneId; }
            set
            {
                _targetZoneId = value;
                Dirtied();
            }
        }

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                Dirtied();
            }
        }

        public int Version
        {
            get { return _version; }
            set
            {
                _version = value;
                Dirtied();
            }
        }

        public int Number
        {
            get { return _number; }
            set
            {
                _number = value;
                Dirtied();
            }
        }

        public int ClientVersionMask
        {
            get { return _clientVersionMask; }
            set
            {
                _clientVersionMask = value;
                Dirtied();
            }
        }

        public override string ToString()
        {
            return this.Zone.ToString() + " to " + this.TargetZoneId.ToString();
        }
    }
}
