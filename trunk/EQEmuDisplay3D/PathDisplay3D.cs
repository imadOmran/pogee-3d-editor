using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

using HelixToolkit;
using HelixToolkit.Wpf;

using EQEmu;

namespace EQEmuDisplay3D
{
    public class PathDisplay3D : IDisplay3D
    {
        private Dictionary<EQEmu.Path.Node, Model3DCollection> _mapping = new Dictionary<EQEmu.Path.Node, Model3DCollection>();
        //private Dictionary<EQEmu.Path.Node, Point3D> _mappingOrigin = new Dictionary<EQEmu.Path.Node, Point3D>();

        private EQEmu.Path.Path _path;

        private Model3D _model = new Model3DGroup();

        public PathDisplayOptions Options
        {
            get;
            private set;
        }

        public System.Windows.Media.Media3D.Model3D Model
        {
            get
            {
                return _model;
            }
        }

        private ViewClipping _clipping;
        public ViewClipping Clipping
        {
            get { return _clipping; }
            set
            {
                if (_clipping != null)
                {
                    _clipping.OnClippingChanged -= Clipping_OnClippingChanged;
                }

                _clipping = value;
                value.OnClippingChanged += new ViewClipping.ClippingChangedHandler(Clipping_OnClippingChanged);
                UpdateAll();
            }
        }

        public PathDisplay3D(EQEmu.Path.Path path)
        {
            //Model.Content = new Model3DGroup();
            //Model3DGroup group = new Model3DGroup();
            Options = new PathDisplayOptions()
            {
                ShowTextLabels = false
            };
            //_clipping = new ViewClipping();
            

            _path = path;
            InitMapping();
            MeshBuild();

            foreach (EQEmu.Path.Node node in path.Nodes)
            {
                CreateNode(node);
                node.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(node_PropertyChanged);
            }

            path.NodeAdded += new EQEmu.Path.Path.NodeModifiedHandler(path_NodeAdded);
            path.NodeRemoved += new EQEmu.Path.Path.NodeModifiedHandler(path_NodeRemoved);

            //Model.Content = group;
        }

        private void InitMapping()
        {
            foreach (EQEmu.Path.Node node in _path.Nodes)
            {
                _mapping[node] = null;
            }
        }

        void Clipping_OnClippingChanged(object sender, EventArgs e)
        {
            UpdateAll();
        }

        void path_NodeRemoved(object sender, EQEmu.Path.PathModifiedEventArgs e)
        {
            RemoveNode(e.NodeReference, e.Neighbors);
        }

        public void UpdateAll2()
        {                          
            foreach (EQEmu.Path.Node node in _path.Nodes)
            {
                CreateNode(node);
            }             
        }

        public void UpdateAll()
        {
            foreach (EQEmu.Path.Node node in _path.Nodes)
            {
                UpdateNode(node);
            }
        }


        private GeometryModel3D _nodeModel;
        private void MeshBuild()
        {
            //build the box for the nodes
            MeshBuilder builder = new MeshBuilder();            
            builder.AddBox(new Point3D(0, 0, 0), 2, 2, 2);
            _nodeModel = new GeometryModel3D(builder.ToMesh(), Materials.Green);            
        }

        public GeometryModel3D GetNodeModel()
        {
            return _nodeModel.CloneCurrentValue();
        }

        private void UpdateNode(EQEmu.Path.Node node)
        {
            Model3DGroup group = Model as Model3DGroup;

            if (Clipping != null && !Clipping.DrawPoint(node.Location))
            {
                foreach (Model3D model in _mapping[node])
                {
                    group.Children.Remove(model);
                }
            }
            else
            {
                foreach (Model3D model in _mapping[node])
                {
                    if (group.Children.Contains(model))
                    {
                        continue;
                    }
                    group.Children.Add(model);
                }                            
            }
        }
        
        private void CreateNode(EQEmu.Path.Node node)
        {
            Model3DGroup group = Model as Model3DGroup;
            MeshBuilder builder;

            Model3DCollection collection = new Model3DCollection();

            if (_mapping[node] != null)
            {
                foreach (Model3D model in _mapping[node])
                {
                    group.Children.Remove(model);
                }
            }              

            if (Clipping != null && !Clipping.DrawPoint(node.Location)) return;

            //builder.AddBox(node.Location, 2, 2, 2);

            _mapping[node] = collection;

            GeometryModel3D box = GetNodeModel();
            box.Transform = new TranslateTransform3D(node.X,node.Y,node.Z);
            collection.Add(box);
            //collection.Add(new GeometryModel3D(builder.ToMesh(), Materials.Green));
            //_mapping[node] = new GeometryModel3D(builder.ToMesh(), Materials.Green);

            if (Options.ShowTextLabels)
            {
                GeometryModel3D text = TextCreator.CreateTextLabelModel3D(node.Id.ToString(), BrushHelper.CreateGrayBrush(5), true, 2,
                                                                            new Point3D(node.X, node.Y, node.Z + 5), new Vector3D(1, 0, 0), new Vector3D(0, 0, 1));
                text.Transform = new ScaleTransform3D(new Vector3D(-1, 1, 1), new Point3D(node.X, node.Y, node.Z));
                collection.Add(text);
            }

            foreach (EQEmu.Path.Neighbor neighbor in node.Neighbors)
            {
                if (neighbor.Node == null) continue;

                builder = new MeshBuilder();
                builder.AddArrow(new Point3D(node.X, node.Y, node.Z), new Point3D(neighbor.Node.X, neighbor.Node.Y, neighbor.Node.Z), 0.5);
                //builder.AddPipe(new Point3D(n.X, n.Y, n.Z), new Point3D(neighbor.Node.X, neighbor.Node.Y, neighbor.Node.Z), 0.5, 0.5, 50);
                collection.Add(new GeometryModel3D(builder.ToMesh(), Materials.Yellow));
            }

            foreach (Model3D model in collection)
            {
                group.Children.Add(model);
            }

            //group.Children.Add(_mapping[node]);
        }

        private void RemoveNode(EQEmu.Path.Node node,List<EQEmu.Path.Node> neighbors=null)
        {
            Model3DGroup group = Model as Model3DGroup;
            node.PropertyChanged -= node_PropertyChanged;
                                    
            foreach (Model3D model in _mapping[node])
            {
                group.Children.Remove(model);
            }

            if (neighbors != null && neighbors.Count > 0)
            {
                foreach (EQEmu.Path.Node n in neighbors)
                {
                    CreateNode(n);                    
                }
            }

            _mapping[node] = null;
        }

        void path_NodeAdded(object sender, EQEmu.Path.PathModifiedEventArgs e)
        {
            _mapping[e.NodeReference] = null;
            CreateNode(e.NodeReference);
            e.NodeReference.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(node_PropertyChanged);
        }

        void node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            EQEmu.Path.Node node = sender as EQEmu.Path.Node;
            CreateNode(node);
            foreach (EQEmu.Path.Neighbor neighbor in node.Neighbors)
            {
                CreateNode(neighbor.Node);
            }
        }
    }

    public class PathDisplayOptions
    {
        public bool ShowTextLabels { get; set; }
    }
}
