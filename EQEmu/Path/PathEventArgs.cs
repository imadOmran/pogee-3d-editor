using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQEmu.Path
{
    public class PathModifiedEventArgs : EventArgs
    {
        private readonly Node _node;
        private readonly List<Node> _neighbors;

        public Node NodeReference
        {
            get { return _node; }
        }

        public List<Node> Neighbors
        {
            get { return _neighbors; }
        }

        public PathModifiedEventArgs(Node node, List<Node> neighbors)
        {
            _node = node;
            _neighbors = neighbors;
        }
    }
}
