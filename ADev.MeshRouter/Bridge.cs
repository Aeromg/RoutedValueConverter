
namespace ADev.MeshRouter
{
    /// <summary>
    /// Represents bridge between two nodes
    /// </summary>
    /// <typeparam name="TKey">Type of node key</typeparam>
    /// <typeparam name="TPayload">Type of bridge payload</typeparam>
    sealed class Bridge<TKey, TPayload>
    {
        /// <summary>
        /// Source node
        /// </summary>
        public Node<TKey, TPayload> Source { get; set; }    // may be field is redundant?

        /// <summary>
        /// Destination node
        /// </summary>
        public Node<TKey, TPayload> Destination { get; set; }

        /// <summary>
        /// Bridge payload.
        /// It may be used for keepig some marking or bridge method or something else
        /// </summary>
        public TPayload Payload { get; set; }

        /// <summary>
        /// Bridge complexity.
        /// It may be something like a complexity of algorithm or distance or else
        /// </summary>
        public int Complexity { get; set; }
    }
}
