using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Media.Media3D;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.ComponentModel;

namespace EQEmu.Path
{
        /// <summary>
        /// Manager class for a set of connected nodes forming pathing information
        /// </summary>
        public class Path
        {
            //private List<Node> _nodes = new List<Node>();
            private ObservableCollection<Node> _nodes = new ObservableCollection<Node>();

            public ObservableCollection<Node> Nodes
            {
                get
                {
                    return _nodes;
                }
            }

            /// <summary>
            /// Returns Node elements that have zero neighbors
            /// </summary>
            public ObservableCollection<Node> DisconnectedNodes
            {
                get
                {
                    ObservableCollection<Node> list = new ObservableCollection<Node>();
                    var query = _nodes.Select(x => x).Where(x => x.Neighbors.Count == 0);
                    foreach (Node n in query)
                    {
                        list.Add(n);
                    }
                    return list;
                }
            }

            public Path()
            {                
                _nodes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(NodesCollectionChanged);                
            }

            private void NodesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                UpdateNodes();
            }

            public void AddNode(Node node)
            {
                Nodes.Add(node);
                UpdateNodes();
                OnNodeAdded(node);
            }

            public void RemoveNode(Node node)
            {
                //OnNodeRemoved(node);
                List<Node> neighbors = new List<Node>();

                node.DisconnectAll();
                foreach (Node n in Nodes)
                {
                    if (n.Neighbors.RemoveAll(x => x.Node == node) > 0)
                    {
                        neighbors.Add(n);
                    }
                }
                Nodes.Remove(node);
                UpdateNodes();
                OnNodeRemoved(node, neighbors);
            }            

            public ObservableCollection<Node> GetInaccessibleNodes(Node n)
            {
                if (n == null) return new ObservableCollection<Node>();

                ObservableCollection<Node> unreachableNodes = new ObservableCollection<Node>();
                
                IEnumerable<Node> connectedNodes = n.ConnectedNodes;

                var query = Nodes.Select(x => x).Where(y => connectedNodes.Contains(y) == false);
                foreach (Node node in query)
                {
                    unreachableNodes.Add(node);
                }

                return unreachableNodes;
            }

            public ObservableCollection<Node> GetInaccessibleNodes(int n)
            {
                return this.GetInaccessibleNodes(Nodes.ElementAt(n));
            }

            public ObservableCollection<Node> GetNonNeighbors(int n)
            {
                return GetNonNeighbors(Nodes.ElementAt(n));
            }

            public ObservableCollection<Node> GetNonNeighbors(Node n)
            {
                if (n == null) return new ObservableCollection<Node>();

                ObservableCollection<Node> nonNeighbors = new ObservableCollection<Node>();                

                List<Node> neighborNodes = new List<Node>();
                neighborNodes.Add(n);
                foreach (Neighbor neighbor in n.Neighbors)
                {
                    neighborNodes.Add(neighbor.Node);
                }

                var query = Nodes.Select(x => x).Where(y => neighborNodes.Contains(y) == false);
                foreach (Node node in query)
                {
                    nonNeighbors.Add(node);
                }
                return nonNeighbors;
            }

            private void UpdateNodes()
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    Nodes.ElementAt(i).Id = i;         
                }
            }

            public static Path LoadFile(string file)
            {
                Path p = new Path();
                FileStream fs = new FileStream(file.ToLower(), FileMode.Open);
                
                Stream s = fs;
                HeaderStruct header = p.ReadHeader(ref s);
                p.ReadNodes(ref s, header);
                fs.Close();

                return p;
            }

            public void SaveAsXml(string file)
            {
                FileStream fs = new FileStream(file, FileMode.Create);
                Stream s = fs;

                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(this.GetType());
                x.Serialize(Console.Out, this);
                x.Serialize(s, this);
            }

            public void SaveAsBinary(string file)
            {
                FileStream fs = new FileStream(file, FileMode.Create);

                Stream s = fs;
                WriteHeader(ref s);
                WriteNodes(ref s);
                fs.Close();
            }

            /// <summary>
            /// Connects node at index n1 to node at index n2. The connection is two way.
            /// </summary>
            /// <param name="n1"></param>
            /// <param name="n2"></param>
            public void NodeTwoWayConnect(int n1, int n2)
            {
                Nodes.ElementAt(n1).ConnectToNode(Nodes.ElementAt(n2));
                Nodes.ElementAt(n2).ConnectToNode(Nodes.ElementAt(n1));
            }

            /// <summary>
            /// Connects node at index n1 to node at index n2 and sets the calculated distance at 0.  This connection is flagged as a warp
            /// </summary>
            /// <param name="n1"></param>
            /// <param name="n2"></param>
            public void NodeWarpConnect(int n1, int n2)
            {
                Nodes.ElementAt(n1).ConnectToNodeAsWarp(Nodes.ElementAt(n2));
            }

            /// <summary>
            /// Connects node at index n1 to node at index n2 - this connection is only one way
            /// </summary>
            /// <param name="n1">base node</param>
            /// <param name="n2">target node</param>
            public void NodeSingleConnect(int n1, int n2)
            {
                Nodes.ElementAt(n1).ConnectToNode(Nodes.ElementAt(n2));
            }

            /// <summary>
            /// Disconnects node at index n1 from node at index n2.  If n2 is connected to n1 the connection remains.
            /// </summary>
            /// <param name="n1"></param>
            /// <param name="n2"></param>

            public void NodeSingleDisconnect(int n1, int n2)
            {
                Nodes.ElementAt(n1).DisconnectNode(Nodes.ElementAt(n2));
            }

            public void NodeSingleDisconnect(Node n1, Node n2)
            {
                n1.DisconnectNode(n2);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="n1"></param>
            /// <param name="n2"></param>

            public void NodeTwoWayDisconnect(int n1, int n2)
            {
                Nodes.ElementAt(n1).DisconnectNodeTwoWay(Nodes.ElementAt(n2));
            }

            public void NodeTwoWayDisconnect(Node n1, Node n2)
            {
                n1.DisconnectNodeTwoWay(n2);
            }

            public void NodeDisconnectAll()
            {
                foreach (Node n in Nodes)
                {
                    n.DisconnectAll();
                }
            }

            public void NodeDisconnectAll(int n1)
            {
                Nodes.ElementAt(n1).DisconnectAll();
            }

            public static float CalculateDistance(Node n1, Node n2)
            {
                Point3D p1 = new Point3D(n1.X, n1.Y, n1.Z);
                Point3D p2 = new Point3D(n2.X, n2.Y, n2.Z);

                return (float)Math.Sqrt((Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.Z - p1.Z, 2)));
            }

            public static float CalculateDistance(Point3D p1, Point3D p2)
            {
                return (float)Math.Sqrt((Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.Z - p1.Z, 2)));
            }

            public Node GetNearbyNode(Point3D pt,double threshhold=5.0)
            {
                double minDist = 9999.0;
                double dist = 9999.0;
                EQEmu.Path.Node candidate = null;                                

                foreach (EQEmu.Path.Node n in Nodes)
                {
                    dist = EQEmu.Path.Path.CalculateDistance(n.Location, pt);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        candidate = n;
                    }
                }

                if (candidate != null && minDist < threshhold)
                {
                    return candidate;
                }
                else
                {
                    return null;
                }
            }

            private void WriteHeader(ref Stream s)
            {
                HeaderStruct header = new HeaderStruct();

                header.pathNodeCount = Nodes.Count;
                header.version = 2;

                BinaryWriter bw = new BinaryWriter(s);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("EQEMUPATH"));

                bw.Write(Functions.StructToByteArray(header));
            }

            private void WriteNodes(ref Stream s)
            {
                BinaryWriter bw = new BinaryWriter(s);

                NodeStruct node = new NodeStruct();
                for (int i = 0; i < Nodes.Count; i++)
                {
                    node.bestZ = 0.0f;
                    node.id = (ushort)i;
                    node.v.x = (float)Nodes.ElementAt(i).X;
                    node.v.y = (float)Nodes.ElementAt(i).Y;
                    node.v.z = (float)Nodes.ElementAt(i).Z;
                    //node.bestZ = (float)Nodes.ElementAt(i).Z;
                    node.bestZ = Nodes.ElementAt(i).BestZ;

                    //this should probably be removed at some point
                    //and corrected in the path data itself
                    if (node.bestZ == 0)
                    {
                        node.bestZ = node.v.z;
                    }

                    bw.Write(Functions.StructToByteArray(node));

                    int neighborCount = Nodes.ElementAt(i).Neighbors.Count;

                    NeighborStruct neighbor = new NeighborStruct();
                    List<Neighbor> nList = Nodes.ElementAt(i).Neighbors;
                    for (int j = 0; j < 50; j++)
                    {
                        if (j >= neighborCount)
                        {
                            neighbor.id = -1;
                            neighbor.doorId = 0;
                            neighbor.distance = 999999.0F;
                            neighbor.teleport = 0;
                        }
                        else
                        {
                            neighbor.distance = nList.ElementAt(j).Distance;
                            neighbor.doorId = nList.ElementAt(j).DoorId;

                            if (nList.ElementAt(j).Warp) neighbor.teleport = 1;
                            else neighbor.teleport = 0;

                            for (int k = 0; k < Nodes.Count; k++)
                            {
                                if (Nodes.ElementAt(k) == nList.ElementAt(j).Node)
                                {
                                    neighbor.id = (short)k;
                                }
                            }
                        }

                        bw.Write(Functions.StructToByteArray(neighbor));
                    }
                }

            }

            private HeaderStruct ReadHeader(ref Stream s)
            {
                BinaryReader br = new BinaryReader(s);
                int size = Marshal.SizeOf(typeof(HeaderStruct));

                //magic string
                byte[] buffer = new byte[10];
                br.Read(buffer, 0, 9);

                string magicStr = Encoding.ASCII.GetString(buffer);
                if (magicStr != "EQEMUPATH\0") throw new FormatException("Path::ReadHeader - Magic string did not match");

                buffer = new byte[size];
                br.Read(buffer, 0, size);

                return Functions.ByteArrayToStructure<HeaderStruct>(buffer);
            }

            private void ReadNodes(ref Stream s, HeaderStruct header)
            {
                BinaryReader br = new BinaryReader(s);

                int size = Marshal.SizeOf(typeof(NodeStruct));
                int nsize = Marshal.SizeOf(typeof(NeighborStruct));

                byte[] buffer = new byte[size];
                byte[] neighbor = new byte[nsize];

                List<Neighbor> nList;
                Node n;
                for (int i = 0; i < header.pathNodeCount; i++)
                {
                    br.Read(buffer, 0, size);

                    n = new Node(Functions.ByteArrayToStructure<NodeStruct>(buffer));

                    nList = new List<Neighbor>();
                    for (int j = 0; j < 50; j++)
                    {
                        br.Read(neighbor, 0, nsize);

                        NeighborStruct neighborData = Functions.ByteArrayToStructure<NeighborStruct>(neighbor);
                        if (neighborData.id == -1) continue;
                        nList.Add(new Neighbor(neighborData));
                    }
                    n.Neighbors = nList;

                    _nodes.Add(n);
                }

                ConnectNodes();
            }

            /// <summary>
            /// This is an initialization method - do not use
            /// </summary>
            private void ConnectNodes()
            {
                UpdateNodes();
                for (int i = 0; i < _nodes.Count; i++)
                {
                    foreach (Neighbor n in _nodes.ElementAt(i).Neighbors)
                    {
                        if (n.Id >= 0 && n.Id < _nodes.Count)
                        {
                            n.Node = _nodes.ElementAt(n.Id);
                            _nodes.ElementAt(i).SubscribeToNode(n.Node);
                        }
                    }
                }               
            }

            public void ProcessConnections(Map.Map map)
            {
                throw new NotImplementedException();
            }

            public delegate void NodeModifiedHandler(object sender,PathModifiedEventArgs e);
            
            private void OnNodeAdded(Node node)
            {
                if (NodeAdded != null)
                {
                    NodeAdded(this, new PathModifiedEventArgs(node,new List<Node>()));
                }
            }

            private void OnNodeRemoved(Node node,List<Node>neighbors)
            {
                if (NodeRemoved != null)
                {
                    NodeRemoved(this, new PathModifiedEventArgs(node,neighbors));
                }
            }

            public event NodeModifiedHandler NodeAdded;
            public event NodeModifiedHandler NodeRemoved;
        }
}
