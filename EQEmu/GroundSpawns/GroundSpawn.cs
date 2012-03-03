using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace EQEmu.GroundSpawns
{
    public class ItemIDChangedEventArgs : EventArgs
    {
        private int _itemID;

        public ItemIDChangedEventArgs(int itemId)
        {
            _itemID = itemId;
        }

        public int ItemID
        {
            get { return _itemID; }
        }
    }

    public class PositionChangedEventArgs : EventArgs
    {
        private Point3D _point;

        public PositionChangedEventArgs(Point3D newPoint)
        {
            _point = newPoint;
        }

        public Point3D Position
        {
            get { return _point; }
        }
    }

    public class GroundSpawn : Database.DatabaseObject
    {
        private int _id;
        private int _zoneId;
        private int _version;
        private int _item;
        private int _maxAllowed;

        private long _respawnTimer;

        private float _minX;
        private float _maxX;
        private float _minY;
        private float _maxY;
        private float _maxZ;
        private float _heading;

        private string _name;
        private string _comment;
        private string _itemName;

        public float MaxX
        {
            get { return _maxX; }
            set
            {
                _maxX = value;
                Dirtied();
                OnPositionChanged();
            }
        }
        public float MaxY
        {
            get { return _maxY; }
            set
            {
                _maxY = value;
                Dirtied();
                OnPositionChanged();
            }
        }
        public float MaxZ
        {
            get { return _maxZ; }
            set
            {
                _maxZ = value;
                Dirtied();
                OnPositionChanged();
            }
        }
        public float Heading
        {
            get { return _heading; }
            set
            {
                _heading = value;
            }
        }
        public float MinY
        {
            get { return _minY; }
            set
            {
                _minY = value;
                Dirtied();
                OnPositionChanged();
            }
        }
        public float MinX
        {
            get { return _minX; }
            set
            {
                _minX = value;
                Dirtied();
                OnPositionChanged();
            }
        }

        public long RespawnTimer
        {
            get { return _respawnTimer; }
            set
            {
                _respawnTimer = value;
                Dirtied();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                Dirtied();
            }
        }
        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                Dirtied();
            }
        }
        
        public int MaxAllowed
        {
            get { return _maxAllowed; }
            set
            {
                _maxAllowed = value;
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
        public int ZoneId
        {
            get { return _zoneId; }
            set
            {
                _zoneId = value;
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
        public int Item
        {
            get { return _item; }
            set
            {
                _item = value;
                Dirtied();
                OnItemIDChanged(value);
            }
        }

        public string ItemName
        {
            get { return _itemName; }
            set
            {
                _itemName = value;
            }
        }

        public event ItemIDChangedHandler ItemIDChanged;
        private void OnItemIDChanged(int id)
        {
            var e = ItemIDChanged;
            if (e != null)
            {
                e(this, new ItemIDChangedEventArgs(id));
            }
        }
        public delegate void ItemIDChangedHandler(object sender, ItemIDChangedEventArgs args);

        public event PositionChangedHandler PositionChanged;
        public delegate void PositionChangedHandler(object sender, PositionChangedEventArgs args);

        private void OnPositionChanged()
        {
            var e = PositionChanged;
            if (e != null)
            {
                e(this, new PositionChangedEventArgs(MidPoint));
            }
        }

        public void CenterPositionAt(Point3D p)
        {
            var xlen = MaxX - MinX;
            var ylen = MaxY - MinY;

            MaxX = (float)(p.X + xlen / 2);
            MinX = (float)(p.X - xlen / 2);
            MaxY = (float)(p.Y + ylen / 2);
            MinY = (float)(p.Y - ylen / 2);
            MaxZ = (float)p.Z;
        }

        public Point3D MidPoint
        {
            get
            {
                return new Point3D((MaxX + MinX) / 2, (MaxY + MinY) / 2, MaxZ);
            }
        }

        public GroundSpawn(Database.QueryConfig config) : base(config)
        {
            Dirty = false;
        }

        public override string ToString()
        {
            return Item.ToString() + ":" + ItemName;
        }
    }
}
