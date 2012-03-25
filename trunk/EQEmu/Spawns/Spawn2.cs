using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.ComponentModel;

namespace EQEmu.Spawns
{
    public class Spawn2 : Database.DatabaseObject
    {
        public enum Animation
        {
            Standing = 0,
            Sitting = 1,
            Crouching = 2,
            Dead = 3,
            Looting = 4
        };

        private Point3D _position = new Point3D();
        private int _roamAreaId = 0;
        private string _zone = "";
        private int _version = 0;
        private double _heading = 0.0;
        private int _respawnTime = 640;
        private int _variance = 0;
        private int _gridId = 0;
        private int _condition = 0;
        private int _conditionValue = 1;
        private int _enabled = 1;
        private int _spawnGroupId = 0;
        private int _id = 0;
        private Animation _animation = Animation.Standing;

        private SpawnGroup _spawnGroupRef = null;

        public Animation IdleAnimation
        {
            get { return _animation; }
            set
            {
                _animation = value;
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

        public int Variance
        {
            get { return _variance; }
        }

        public int Condition
        {
            get { return _condition; }
            set
            {
                _condition = value;
                Dirtied();
            }
        }

        public int ConditionValue
        {
            get { return _conditionValue; }
            set
            {
                _conditionValue = value;
                Dirtied();
            }
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
            get 
            {
                if (_spawnGroupRef != null)
                {
                    return _spawnGroupId;
                }
                else
                {
                    return _spawnGroupRef.Id;
                }
            }
            set
            {
                _spawnGroupId = value;
                Dirtied();
            }
        }

        [Browsable(false)]
        public SpawnGroup SpawnGroupRef
        {
            get { return _spawnGroupRef; }
            set
            {
                _spawnGroupRef = value;
                _spawnGroupId = value.Id;
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
