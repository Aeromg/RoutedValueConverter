using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System;
using System.Diagnostics;

namespace ADev.MeshRouter
{
    [DebuggerDisplay("{Key.ToString()}")]
    /// <summary>
    /// Represent a node as minimal part of Mesh structure
    /// </summary>
    /// <typeparam name="TKey">Type of node key</typeparam>
    /// <typeparam name="TBridge">Type of bridge payload</typeparam>
    sealed class Node<TKey, TBridge>
    {
        IEnumerable<Node<TKey, TBridge>> _nextHopeNodesCache;

        static long TopNodeId = 0;

        /// <summary>
        /// Node key
        /// </summary>
        public TKey Key { get; set; }

        public readonly long NodeId;

        /// <summary>
        /// All node childs
        /// </summary>
        public IEnumerable<Node<TKey, TBridge>> Childs
        {
            get
            {
                var a = new Dictionary<int, bool>();
                return
                    from node in Mesh.Nodes
                    where this.Equals(node.Parent)
                    select node;
            }
        }

        /// <summary>
        /// gets any available routes from Source node
        /// </summary>
        public IEnumerable<Node<TKey, TBridge>> NextHopeNodes
        {
            get
            {
                return GetNextHopeNodes();
                //if (_nextHopeNodesCache == null)
                  //  _nextHopeNodesCache = GetNextHopeNodes();

                //return _nextHopeNodesCache;
            }
        }

        /// <summary>
        /// Unlink all dependenced childs from this node
        /// </summary>
        public void UnlinkChilds()
        {
            foreach (var child in Childs)
                child.Parent = null;
        }

        /// <summary>
        /// Parent node. Is directly linked with this node
        /// </summary>
        public Node<TKey, TBridge> Parent { get; set; }

        /// <summary>
        /// Enumerate of all bridges which Source is this node
        /// </summary>
        public IList<Bridge<TKey, TBridge>> Bridges { get; private set; }

        /// <summary>
        /// List of all deny step destinations
        /// </summary>
        public IList<Node<TKey, TBridge>> Walls { get; private set; }

        /// <summary>
        /// Mesh that contains this node
        /// </summary>
        public Mesh<TKey, TBridge> Mesh { get; set; }

        /// <summary>
        /// Enumerate all meshes which can be reached from this node
        /// </summary>
        public IEnumerable<Mesh<TKey, TBridge>> GatewayForMeshes
        {
            get
            {
                return
                    (from br in Bridges
                    where !br.Destination.Mesh.Equals(this)
                    select br.Destination.Mesh).Distinct();

            }
        }

        /// <summary>
        /// Check that current node or it parents is equals to given node
        /// </summary>
        /// <param name="node">Node to check</param>
        /// <returns><code>true</code> if the node is suitable</returns>
        public bool EqualsOrContainsParent(Node<TKey, TBridge> node)
        {
            if (this.Equals(node))
                return true;

            // do not use recursion here. It may be stack overflow
            var parent = Parent;

            while (parent != null)
            {
                if (parent.Equals(node))
                    return true;

                parent = parent.Parent;
            }

            return false;
        }

        /// <summary>
        /// Creates an empty node
        /// </summary>
        public Node()
        {
            NodeId = TopNodeId++;

            var bridgesObservable = new ObservableCollection<Bridge<TKey, TBridge>>();
            Bridges = bridgesObservable;
            bridgesObservable.CollectionChanged += NodeElementsChanged;

            var wallsObservable = new ObservableCollection<Node<TKey, TBridge>>();
            Walls = wallsObservable;
            wallsObservable.CollectionChanged += NodeElementsChanged;
        }

        void NodeElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CleanCaches();
        }

        // gets any available routes from this node
        // except routes to Walls elements
        IEnumerable<Node<TKey, TBridge>> GetNextHopeNodes()
        {
            lock (this)
            {
                // we can jump to destination using Bridge
                var nextNodes = new List<Node<TKey, TBridge>>();
                foreach (var bridge in Bridges)
                    if(!Walls.Contains(bridge.Destination))
                        nextNodes.Add(bridge.Destination);

                // or step directly to Source parent
                if (Parent != null && !Walls.Contains(Parent))
                    nextNodes.Add(Parent);

                return nextNodes;
            }
        }

        internal void CleanCaches()
        {
            _nextHopeNodesCache = null;
        }

        public bool Equals(Node<TKey, TBridge> obj)
        {
            if (obj == null)
                return false;

            return obj.NodeId == this.NodeId;
        }

        public override bool Equals(object obj)
        {
            if (obj is Node<TKey, TBridge>)
                return Equals((Node<TKey, TBridge>)obj);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
