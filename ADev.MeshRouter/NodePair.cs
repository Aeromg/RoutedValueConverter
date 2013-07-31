
namespace ADev.MeshRouter
{
    struct NodePair<TKey, TBridge>
    {
        public Node<TKey, TBridge> Source;
        public Node<TKey, TBridge> Destination;
    }
}
