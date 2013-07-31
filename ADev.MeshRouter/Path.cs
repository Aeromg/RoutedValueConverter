using System.Collections.Generic;

namespace ADev.MeshRouter
{
    /// <summary>
    /// Represents resolved path between two nodes in Mesh structure
    /// </summary>
    /// <typeparam name="TKey">Type of node key</typeparam>
    /// <typeparam name="TBridge">Type of bridge payload</typeparam>
    public sealed class Path<TKey, TBridge>
    {
        /// <summary>
        /// Path steps one by one from first to last
        /// </summary>
        public IEnumerable<Step<TKey, TBridge>> Steps { get; internal set; }

        /// <summary>
        /// Sign if path resolved. Else path cannot be resolved
        /// </summary>
        public bool IsResolved { get; internal set; }

        /// <summary>
        /// Key of source node
        /// </summary>
        public TKey Source { get; internal set; }

        /// <summary>
        /// Key of destination node
        /// </summary>
        public TKey Destination { get; internal set; }

        /// <summary>
        /// Total path complexity
        /// </summary>
        public int Complexity { get; internal set; }

        /// <summary>
        /// Creates and returns path with empty steps that marks as cannot be resolved
        /// </summary>
        /// <returns></returns>
        internal static Path<TKey, TBridge> BuildNotResolved(TKey source, TKey destination) 
        {
            return new Path<TKey, TBridge>()
            {
                Steps = new Step<TKey, TBridge>[0],
                Source = source,
                Destination = destination,
                IsResolved = false,
                Complexity = 0
            };
        }
    }
}
