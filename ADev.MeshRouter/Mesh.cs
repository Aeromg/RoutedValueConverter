using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ADev.MeshRouter
{
    /// <summary>
    /// Represents simply Mesh structure where can add or remove nodes
    /// </summary>
    /// <typeparam name="TKey">Type of node key</typeparam>
    /// <typeparam name="TBridge">Type of bridge payload</typeparam>
    public sealed class Mesh<TKey, TBridge>
    {
        /// <summary>
        /// Lookup table for quick find node by key without inspecting everyone
        /// </summary>
        readonly IDictionary<TKey, Node<TKey, TBridge>> KeyToNodesLut;

        internal IEnumerable<Node<TKey, TBridge>> Nodes
        {
            get
            {
                return KeyToNodesLut.Values;
            }
        }

        /// <summary>
        /// Creates an empty thread-unsafe Mesh structure
        /// </summary>
        public Mesh() : this(false) { }

        /// <summary>
        /// Creates an empty mesh structure
        /// </summary>
        /// <param name="concurrent">Sign if structure must be thread-safe</param>
        public Mesh(bool concurrent)
        {
            if (concurrent)
                KeyToNodesLut = new ConcurrentDictionary<TKey, Node<TKey, TBridge>>();
            else
                KeyToNodesLut = new Dictionary<TKey, Node<TKey, TBridge>>();
        }

        #region Public methods

        /// <summary>
        /// Add new node with given key to mesh. Method does nothing if node with same key already exists
        /// </summary>
        /// <param name="key">Node key</param>
        public void AddNode(TKey key)
        {
            GetOrCreateNode(key);
        }

        /// <summary>
        /// Remove the node with given key from mesh and unlink all childs. 
        /// Method does nothing if the node with given key does not exist in this mesh
        /// </summary>
        /// <param name="key"></param>
        public void RemoveNodeAndUnlinkChilds(TKey key)
        {
            Node<TKey, TBridge> node;
            if (!TryGetNode(key, out node))
                return;

            node.UnlinkChilds();
            KeyToNodesLut.Remove(key);
        }

        /// <summary>
        /// Set the node with one key as parent for the node with other key.
        /// If one or both of nodes are not exists, method will create it automatic
        /// </summary>
        /// <exception cref="Exception">Throws exception if parent node has child as parent node (deadlock)</exception>
        /// <param name="child">Key of node which must be as child</param>
        /// <param name="parent">Key of node which must be set as parent</param>
        public void SetParent(TKey child, TKey parent)
        {
            var childNode = GetOrCreateNode(child);
            var parentNode = GetOrCreateNode(parent);

#if DEBUG
            if (parentNode.EqualsOrContainsParent(childNode))
                throw new Exception();
#endif
            childNode.Parent = parentNode;
        }

        /// <summary>
        /// Removes parent from given child node.
        /// Method does nothing if the node not exists
        /// </summary>
        /// <param name="child">Child node key</param>
        public void RemoveParent(TKey child)
        {
            Node<TKey, TBridge> childNode;

            if (TryGetNode(child, out childNode))
                childNode.Parent = null;
        }

        /// <summary>
        /// Set or replace the bridge between node with one key and node with other key.
        /// If one or both of nodes are not exists, method will create it automatic
        /// </summary>
        /// <param name="source">Source node key</param>
        /// <param name="destination">Destination node key</param>
        /// <param name="bridge">Bridge payload</param>
        /// <param name="complexity">Complexity of bridge</param>
        public void SetBridge(TKey source, TKey destination, TBridge bridge, int complexity)
        {
            var sourceNode = GetOrCreateNode(source);
            var destinationNode = GetOrCreateNode(destination);

#if DEBUG
            if (sourceNode == destinationNode)
                throw new Exception();
#endif

            var nodes = new NodePair<TKey, TBridge>()
            {
                Source = sourceNode,
                Destination = destinationNode
            };
            RemoveBridge(ref nodes);
            sourceNode.Bridges.Add(
                new Bridge<TKey, TBridge>()
                {
                    Source = sourceNode,
                    Destination = destinationNode,
                    Payload = bridge,
                    Complexity = complexity
                }
            );
        }

        /// <summary>
        /// Removes bridge between node with one key and node with other key.
        /// Method does nothing if the bridge are not exist
        /// </summary>
        /// <param name="sourceKey">Source node key</param>
        /// <param name="destinationKey">Destination node key</param>
        public void RemoveBridge(TKey sourceKey, TKey destinationKey)
        {
            Node<TKey, TBridge> sourceNode;
            Node<TKey, TBridge> destinationNode;

            if (!TryGetNode(sourceKey, out sourceNode))
                return;

            if (!TryGetNode(destinationKey, out destinationNode))
                return;

            var nodes = new NodePair<TKey, TBridge>()
            {
                Source = sourceNode,
                Destination = destinationNode
            };

            RemoveBridge(ref nodes);
        }

        /// <summary>
        /// Set the deny wall between Source node and Destination node.
        /// If the some node are not exist method will create it
        /// </summary>
        /// <param name="source">Source node key</param>
        /// <param name="destination">Destination node key</param>
        public void SetWall(TKey source, TKey destination)
        {
            var sourceNode = GetOrCreateNode(source);
            var destinationNode = GetOrCreateNode(destination);

            sourceNode.Walls.Add(destinationNode);
        }

        /// <summary>
        /// Removes the deny wall between two nodes.
        /// Method does nothing if source or destination node are not exists
        /// </summary>
        /// <param name="source">Source node key</param>
        /// <param name="destination">Destination node key</param>
        public void RemoveWall(TKey source, TKey destination)
        {
            Node<TKey, TBridge> sourceNode;
            Node<TKey, TBridge> destinationNode;

            if (!TryGetNode(source, out sourceNode))
                return;

            if (!TryGetNode(destination, out destinationNode))
                return;

            sourceNode.Walls.Remove(destinationNode);
        }

        /// <summary>
        /// Check the mesh contains node with given key
        /// </summary>
        /// <param name="key">Node key to find</param>
        /// <returns><code>true</code> if the node are exists</returns>
        public bool ContainsKey(TKey key)
        {
            return KeyToNodesLut.ContainsKey(key);
        }

        internal bool TryGetNode(TKey key, out Node<TKey, TBridge> node)
        {
            return KeyToNodesLut.TryGetValue(key, out node);
        }

        #endregion

        #region Private methods

        void RemoveBridge(ref NodePair<TKey, TBridge> nodes)
        {
            Bridge<TKey, TBridge> bridge;
            if (!TryGetBridge(ref nodes, out bridge))
                return;

            nodes.Source.Bridges.Remove(bridge);
        }

        bool TryGetBridge(ref NodePair<TKey, TBridge> nodes, out Bridge<TKey, TBridge> bridge)
        {
            var source = nodes.Source;
            var destination = nodes.Destination;

            bridge =
                (from bridgeItem in source.Bridges
                 where bridgeItem.Destination == destination
                 select bridgeItem).FirstOrDefault();

            return bridge != null;
        }

        Node<TKey, TBridge> GetOrCreateNode(TKey key)
        {
            Node<TKey, TBridge> node;
            if (!TryGetNode(key, out node))
                node = KeyToNodesLut[key] = CreateNode(key);

            return node;
        }

        Node<TKey, TBridge> CreateNode(TKey key)
        {
            return new Node<TKey, TBridge>()
            {
                Key = key,
                Mesh = this
            };
        }

        // get all last hopes nodes to given mesh represented by this mesh
        IEnumerable<Node<TKey, TBridge>> GetMeshGateway(Mesh<TKey, TBridge> mesh)
        {
            var thisMeshNodes = KeyToNodesLut.Values;
            return
                from node in thisMeshNodes
                where node.GatewayForMeshes.Contains(mesh)
                select node;
        }

        #endregion

    }
}
