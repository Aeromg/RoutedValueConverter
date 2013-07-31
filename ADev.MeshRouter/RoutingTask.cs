
namespace ADev.MeshRouter
{
    struct RoutingTask<TKey, TBridge>
    {
        public Node<TKey, TBridge> StartNode;
        public Node<TKey, TBridge> EndNode;

        public bool IsCorrect
        {
            get
            {
                return StartNode != null && EndNode != null;
            }
        }
    }
}
