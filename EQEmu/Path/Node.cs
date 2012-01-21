using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;

namespace EQEmu.Path
{
    ///<summary>
    ///Represents an individual node or waypoint in a path
    ///</summary>
    public class Node : INotifyPropertyChanged
    {
        private UInt16 _id = 0;
        private Point3D _location = new Point3D(0, 0, 0);
        private float _bestZ = 0;

        private List<Neighbor> _neighbors = new List<Neighbor>();

        public int Id { get; set; }

        [XmlIgnore]
        public double X
        {
            get
            {
                return _location.X;
            }
            set
            {
                _location.X = value;
                UpdateNode();
            }
        }

        [XmlIgnore]
        public double Y
        {
            get
            {
                return _location.Y;
            }
            set
            {
                _location.Y = value;
                UpdateNode();
            }
        }

        [XmlIgnore]
        public double Z
        {
            get
            {
                return _location.Z;
            }
            set
            {
                _location.Z = value;
                UpdateNode();
            }
        }

        public float BestZ
        {
            get
            {
                return _bestZ;
            }
            set
            {
                _bestZ = value;
            }
        }

        /// <summary>
        /// DO NOT modify the xyz using this use the X,Y,Z properties
        /// </summary>
        public Point3D Location
        {
            get
            {
                return _location;
            }
        }

        /*
        public Point3D Location
        {
            get
            {
                return _location;
            }
            set
            {
                _location = value;
                UpdateNode();
            }
        }
        */

        public List<Neighbor> Neighbors
        {
            get
            {
                return _neighbors;
            }
            set
            {
                _neighbors = value;
            }
        }

        public List<Node> ConnectedNodes
        {
            get
            {
                return TraverseConnectedNodes();
            }
        }

        public Node()
        {

        }

        public Node(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }


        public Node(NodeStruct node)
        {
            ImportFromStruct(node);
        }

        public List<Node> TraverseConnectedNodes()
        {
            List<Node> cNodes = new List<Node>();
            cNodes.Add(this);
            foreach (Neighbor n in Neighbors)
            {
                cNodes.Add(n.Node);
                n.Node.TraverseConnectedNodes(ref cNodes);
            }

            return cNodes.Distinct().ToList<Node>();
        }

        public void TraverseConnectedNodes(ref List<Node> traversedNodes)
        {
            foreach (Neighbor n in Neighbors)
            {
                if (traversedNodes.Contains(n.Node))
                {
                    continue;
                }
                else
                {
                    traversedNodes.Add(n.Node);
                    n.Node.TraverseConnectedNodes(ref traversedNodes);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{3}:({0:F3},{1:F3},{2:F3})", X, Y, Z, Id);
        }

        public void SetLocation(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        ///<summary>
        ///Connects this node to the target node - distance calculations are also stored
        ///</summary>
        public void ConnectToNode(Node target)
        {
            if (this == target) throw new Exception("Cannot connect node to itself");

            //return if this target node is already in the neighbors list
            foreach (Neighbor neighbor in Neighbors)
            {
                if (neighbor.Node == target) return;
            }
            Neighbors.Add(new Neighbor() { Node = target, Distance = Path.CalculateDistance(this, target) });
            //SubscribeToNode(target);
            target.PropertyChanged += NodePropertyChanged;
            NotifyPropertyChanged("Neighbors");
        }

        public void ConnectToNodeTwoWay(Node target)
        {
            ConnectToNode(target);
            target.ConnectToNode(this);
        }

        public void ConnectToNodeAsWarp(Node target)
        {
            ConnectToNode(target);
            foreach (Neighbor n in Neighbors)
            {
                if (n.Node == target)
                {
                    n.Warp = true;
                    n.Distance = 0.0f;
                    return;
                }
            }
        }

        public void DisconnectNode(Node target)
        {
            Neighbors.RemoveAll(x => x.Node == target);
            //UnsubscribeToNode(target);
            target.PropertyChanged -= NodePropertyChanged;
            NotifyPropertyChanged("Neighbors");
        }

        public void DisconnectNodeTwoWay(Node target)
        {
            target.DisconnectNode(this);
            DisconnectNode(target);
        }

        public void DisconnectAll()
        {
            foreach (Neighbor neighbor in Neighbors)
            {
                this.UnsubscribeToNode(neighbor.Node);
                neighbor.Node.UnsubscribeToNode(this);
            }

            Neighbors.Clear();
            NotifyPropertyChanged("Neighbors");
        }

        public void DisconnectAllTwoWay()
        {
            foreach (Neighbor n in Neighbors)
            {
                DisconnectNodeTwoWay(n.Node);
            }
        }

        public void SubscribeToNode(Node target)
        {
            target.PropertyChanged += NodePropertyChanged;
        }

        public void UnsubscribeToNode(Node target)
        {
            target.PropertyChanged -= NodePropertyChanged;
        }

        void NodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Node nodeChanged = sender as Node;
            switch (e.PropertyName)
            {
                case "Location":
                    var query = Neighbors.Select(x => x).Where(y => y.Node == nodeChanged);
                    foreach (Neighbor neighbor in query)
                    {
                        CalculateDistance(neighbor);
                    }
                    break;
                default:
                    break;
            }
        }

        public void ImportFromStruct(NodeStruct node)
        {
            _id = node.id;
            _location.X = node.v.x;
            _location.Y = node.v.y;
            _location.Z = node.v.z;
            _bestZ = node.bestZ;
        }

        private void UpdateNode()
        {
            //need to calculate new distances on immediate neighbors
            SetNeighborDistances();
            //need to calculate new distances on nodes that are connected to this node
            //but aren't necessary accessible from this node (i.e one way connection)

            //when a node is connected it needs to subscribe to the property change event                
            NotifyPropertyChanged("Location");
        }

        private void SetNeighborDistances()
        {
            foreach (Neighbor neighbor in Neighbors)
            {
                CalculateDistance(neighbor);
            }
        }

        private void CalculateDistance(Neighbor neighbor)
        {
            float distance = 0.0f;
            if (!neighbor.Warp)
            {
                distance = Path.CalculateDistance(this, neighbor.Node);
            }
            neighbor.Distance = distance;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
