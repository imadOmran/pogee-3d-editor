using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace EQEmu.Path
{
    public class Neighbor
    {
        private Node _node = null;
        private Int16 _id;
        private float _distance;
        private byte _teleport;
        private Int16 _doorId;

        public Int16 DoorId
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

        public float Distance
        {
            get
            {
                return _distance;
            }
            set
            {
                _distance = value;
            }
        }

        public bool Warp
        {
            get
            {
                return _teleport > 0;
            }
            set
            {
                if (value == true)
                {
                    _teleport = 1;
                }
                else _teleport = 0;
            }
        }

        [XmlIgnore]
        public Node Node
        {
            get
            {
                return _node;
            }
            set
            {
                _node = value;
            }
        }

        public Int16 Id
        {
            get { return _id; }
        }

        public Neighbor()
        {

        }

        public Neighbor(NeighborStruct neighbor)
        {
            ImportFromStruct(neighbor);
        }

        public void ImportFromStruct(NeighborStruct neighbor)
        {
            _id = neighbor.id;
            _distance = neighbor.distance;
            _teleport = neighbor.teleport;
            _doorId = neighbor.doorId;
        }
    }
}
