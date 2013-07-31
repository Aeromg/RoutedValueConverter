using System.Diagnostics;

namespace ADev.MeshRouter
{
    [DebuggerDisplay("Key:{Key}, Bridge:{Bridge}")]
    /// <summary>
    /// Represents routing path step
    /// </summary>
    /// <typeparam name="TKey">Type of node key</typeparam>
    /// <typeparam name="TBridge">Type of bridge payload</typeparam>
    public struct Step<TKey, TBridge>
    {
        /// <summary>
        /// Step node key
        /// </summary>
        public TKey Key { get; internal set; }

        /// <summary>
        /// Step bridge payload
        /// </summary>
        public TBridge Bridge { get; internal set; }

        /// <summary>
        /// Step to parent node without using bridge
        /// </summary>
        public bool IsDirect
        {
            get
            {
                return Bridge == null;
            }
        }
    }
}
