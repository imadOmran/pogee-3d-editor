using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;

using EQEmu.Database;

namespace EQEmu.RoamAreas
{
    public class RoamAreaEntry : Database.DatabaseObject
    {
        public RoamAreaEntry(int number, int roamAreaId, string zone, QueryConfig config)
            : base(config)
        {
            _roamAreaid = roamAreaId;
            _zone = zone;
            _number = number;
        }

        public RoamAreaEntry(QueryConfig config)
            : base(config)
        {

        }

        private RoamAreaEntry()
            : base(null)
        {

        }

        private int _number;
        public int Number
        {
            get { return _number; }
            set
            {
                _number = value;
                Dirtied();
            }
        }

        private string _zone = "";
        public string Zone
        {
            get { return _zone; }
            set
            {
                _zone = value;
                Dirtied();
            }
        }

        private int _roamAreaid = 0;
        public int RoamAreaId
        {
            get { return _roamAreaid; }
            set
            {
                _roamAreaid = value;
                Dirtied();
            }
        }

        private Point3D _position = new Point3D();
        public double X
        {
            get { return _position.X; }
            set
            {
                _position.X = value;
                Dirtied();
            }
        }

        public double Y
        {
            get { return _position.Y; }
            set
            {
                _position.Y = value;
                Dirtied();
            }
        }
    }
}
