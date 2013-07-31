using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ADev.RoutedValueConverter
{
    /// <summary>
    /// Provide Type comparing and checking method
    /// </summary>
    class TypeCastChecker
    {
        #region Private fields
        readonly Type TypeOfObject;

        bool _concurrent;
        IDictionary<Type, IDictionary<Type, bool>> _sourceDestinationCastableTypesLut;

        #endregion

        /// <summary>
        /// Creates thread-unsafe instance
        /// </summary>
        public TypeCastChecker() : this(false) { }

        /// <summary>
        /// Creates instance
        /// </summary>
        /// <param name="concurrent">
        /// <code>true</code> for thread-safe or 
        /// <code>false</code> for thread-unsafe
        /// </param>
        public TypeCastChecker(bool concurrent) 
        {
            _concurrent = concurrent;
            TypeOfObject = typeof(object);

            if (concurrent)
                _sourceDestinationCastableTypesLut = new ConcurrentDictionary<Type, IDictionary<Type, bool>>();
            else
                _sourceDestinationCastableTypesLut = new Dictionary<Type, IDictionary<Type, bool>>();
        }

        /// <summary>
        /// Check if the one type is castable to another type
        /// </summary>
        /// <param name="source">Source type</param>
        /// <param name="destination">Destination type</param>
        /// <returns><code>true</code> if castable</returns>
        public bool CheckIfCastable(Type source, Type destination)
        {
            bool isCastable;

            if (!CheckDestinationLutCached(source, destination, out isCastable))
            {
                isCastable = CheckIfCastableRecursive(source, destination);
                CacheIfCastableResult(source, destination, isCastable);
            }

            return isCastable;
        }

        #region Private methods

        bool CheckIfCastableRecursive(Type source, Type destination)
        {
            if (source == destination)
                return true;

            if (source == TypeOfObject)
                return false;

            return CheckIfCastableRecursive(source.BaseType, destination);
        }

        void CacheIfCastableResult(Type source, Type destination, bool isCastable)
        {
            var destinationsLut = GetDestinationsLut(source);
            destinationsLut[destination] = isCastable;
        }

        bool CheckDestinationLutCached(Type source, Type destination, out bool isCastable)
        {
            var destinationsLut = GetDestinationsLut(source);
            return (destinationsLut.TryGetValue(destination, out isCastable));
        }

        IDictionary<Type, bool> GetDestinationsLut(Type source)
        {
            IDictionary<Type, bool> destinationsLut;
            if (_sourceDestinationCastableTypesLut.TryGetValue(source, out destinationsLut))
                return destinationsLut;

            if (_concurrent)
                destinationsLut = new ConcurrentDictionary<Type, bool>();
            else
                destinationsLut = new Dictionary<Type, bool>();

            _sourceDestinationCastableTypesLut[source] = destinationsLut;

            return destinationsLut;
        }

        #endregion
    }
}
