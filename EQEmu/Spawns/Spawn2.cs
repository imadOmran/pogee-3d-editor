using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace EQEmu.Spawns
{
    public class Spawn2 : Database.DatabaseObject
    {
        private Point3D _position = new Point3D();
        private int _roamAreaId = 0;
        private string _zone = "";
        private int _version = 0;
        private double _heading = 0.0;
        private int _respawnTime = 0;
        private int _variance = 0;
        private int _gridId = 0;
        private int _condition = 0;
        private int _conditionValue = 1;
        private int _enabled = 1;
        private int _spawnGroupId = 0;
        private int _id = 0;

        public int Version
        {
            get { return _version; }
        }

        public int Variance
        {
            get { return _variance; }
        }

        public int Condition
        {
            get { return _condition; }
        }

        public int ConditionValue
        {
            get { return _conditionValue; }
        }

        public int Enabled
        {
            get { return _enabled; }
        }

        public double X
        {
            get
            {
                return _position.X;
            }
            set
            {
                _position.X = value;
                Dirtied();
            }
        }

        public double Y
        {
            get
            {
                return _position.Y;
            }
            set
            {
                _position.Y = value;
                Dirtied();
            }
        }

        public double Z
        {
            get
            {
                return _position.Z;
            }
            set
            {
                _position.Z = value;
                Dirtied();
            }
        }

        public double Heading
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

        public double HeadingDegrees
        {
            get { return _heading / 256 * 360; }
            set
            {
                _heading = value / 360 * 256;
                Dirtied();
            }
        }

        public int RespawnTime
        {
            get
            {
                return _respawnTime;
            }
            set
            {
                _respawnTime = value;
                Dirtied();
            }
        }

        public int GridId
        {
            get { return _gridId; }
            set
            {
                _gridId = value;
                Dirtied();
            }
        }

        public int RoamAreaId
        {
            get { return _roamAreaId; }
            set
            {
                _roamAreaId = value;
                Dirtied();
            }
        }

        public int SpawnGroupId
        {
            get { return _spawnGroupId; }
            set
            {
                _spawnGroupId = value;
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

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                Dirtied();
            }
        }

        //public override string InsertString
        //{
        //    get
        //    {
        //        return "INSERT INTO spawn2 (id,zone,spawngroupId,x,y,z,heading,respawntime,pathgrid,roam_area) VALUES " +
        //               String.Format("({0},'{1}',{2},{3},{4},{5},{6},{7},{8},{9});",Id,Zone,SpawnGroupId,X,Y,Z,Heading,RespawnTime,GridId,RoamAreaId);
        //    }
        //}

        //public override string DeleteString
        //{
        //    get
        //    {
        //        return String.Format("DELETE FROM spawn2 WHERE id = {0} AND zone = '{1}';",Id,Zone);
        //    }
        //}

        //public override string UpdateString
        //{
        //    get
        //    {
        //        return
        //            String.Format("UPDATE spawn2 SET spawngroupId = {0}, zone = '{1}', version = {2}, x = {3:F3}, y = {4:F3}, z = {5:F3}, heading = {6:F3}, respawntime = {7}, variance = {8}, pathgrid = {9}, roam_area = {10} ",
        //            SpawnGroupId, Zone, _version, X, Y, Z,Heading,RespawnTime,_variance,GridId,RoamAreaId) +
        //            String.Format("WHERE id = {0} AND zone = '{1}';", _id,Zone);
        //    }
        //}

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
            return String.Format("Spawn:{0} - SG {1}", Id, SpawnGroupId);
        }

        public Spawn2(EQEmu.Database.QueryConfig config)
            :base(config)
        {

        }
    }
}
