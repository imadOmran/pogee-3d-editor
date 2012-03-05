using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.ComponentModel;

using EQEmu.Database;

namespace EQEmu.Grids
{
    public class Waypoint : DatabaseObject
    {
        private int _gridId;
        [Browsable(false)]
        public int GridId
        {
            get { return _gridId; }
            set 
            { 
                _gridId = value;
                Dirtied();
            }
        }

        private Grid _grid;        
        [Browsable(false)]
        public Grid GridReference
        {
            get { return _grid; }
            set { _grid = value; }
        }

        private int _zoneId;
        public int ZoneId
        {
            get { return _zoneId; }
            set
            {
                _zoneId = value;
                Dirtied();
            }
        }

        private Point3D _location = new Point3D();
        public double X
        {
            get { return _location.X; }
            set
            {
                _location.X = value;
                Dirtied();
            }
        }
        public double Y
        {
            get { return _location.Y; }
            set
            {
                _location.Y = value;
                Dirtied();
            }
        }
        public double Z
        {
            get { return _location.Z; }
            set
            {
                _location.Z = value;
                Dirtied();
            }
        }

        private float _heading;
        public float Heading
        {
            get { return _heading; }
            set
            {
                _heading = value;
                Dirtied();
            }
        }

        public float HeadingDegrees
        {
            get { return _heading / 256 * 360; }
            set
            {
                _heading = value / 360 * 256;
                Dirtied();
            }
        }

        private int _pauseTime;
        public int PauseTime
        {
            get
            {
                return _pauseTime;
            }
            set
            {
                _pauseTime = value;
                Dirtied();
            }
        }

        private int _number;
        public int Number
        {
            get { return _number; }
            set { _number = value; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                Dirtied();
            }
        }

        private bool _running;
        public bool Running
        {
            get { return _running; }
            set
            {
                _running = value;
                Dirtied();
            }
        }

        public Waypoint(QueryConfig config)
            : base(config)
        {
            Dirty = false;
        }

        public Waypoint(int gridId, int zoneId, int number, QueryConfig config)
            : base(config)
        {
            GridId = gridId;
            ZoneId = zoneId;
            Number = number;
            Dirty = false;
        }

        /// <summary>
        /// Adjusts the heading to look at the specified point
        /// </summary>
        /// <param name="point"></param>
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
            this.HeadingDegrees = (float)degrees;
        }

        public override string ToString()
        {
            return string.Format("Waypoint: {0}, ({1:F3},{2:F3},{3:F3}) Pause:{4}", Number, X, Y, Z, PauseTime);
        }

        //public override string InsertString
        //{
        //    get
        //    {
        //        return "INSERT INTO grid_entries (gridid,zoneid,number,x,y,z,heading,pause,running,name) " +
        //        String.Format("VALUES (@GridID,{1},{2},{3},{4},{5},{6},{7},{8},'{9}');",
        //            GridId, ZoneId, Number, X, Y, Z, Heading, PauseTime, Convert.ToInt32(Running),Name);
        //    }
        //}

        //public override string UpdateString
        //{
        //    get
        //    {
        //        return "UPDATE grid_entries SET " +
        //            String.Format("x = {0}, y = {1}, z = {2}, heading = {4}, pause = {5}, running = {6}, name = '{7}' ",
        //                X, Y, Z, Number, Heading, PauseTime,Convert.ToInt32(Running),Name) +
        //            String.Format("WHERE gridid = @GridID AND zoneid = {1} AND number = {2};",
        //                GridId, ZoneId, Number);
        //    }
        //}

        //public override string DeleteString
        //{
        //    get
        //    {
        //        return String.Format(
        //            "DELETE FROM grid_entries WHERE gridid = @GridID AND zoneid = {1} AND number = {2};",
        //            GridId, ZoneId, Number);
        //    }
        //}
    }
}
