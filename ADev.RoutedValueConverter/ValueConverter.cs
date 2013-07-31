using ADev.MeshRouter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ADev.RoutedValueConverter
{
    /// <summary>
    /// <para>
    /// Router-based Value Converter
    /// </para>
    /// <para>
    /// Provides value conversion methods
    /// </para>
    /// </summary>
    public class ValueConverter
    {
        readonly Router<Type, IConverterAction> TypeRouter;
        readonly Mesh<Type, IConverterAction> TypeMesh;
        readonly TypeCastChecker CastChecker;
        readonly bool Concurrent;

        IConverterAction _defaultConverterAction;
        IDictionary<Type, IDictionary<Type, Func<object, object>>> _summaryFunctCache;
        
        /// <summary>
        /// Creates thread unsafe value converter
        /// </summary>
        public ValueConverter() : this(false) { }

        /// <summary>
        /// Creates value converter
        /// </summary>
        /// <param name="concurrent">Set or unset thread-safe model
        /// <code>true</code>for enable thread-safe model
        /// <code>false</code>for disable thread-safe model</param>
        public ValueConverter(bool concurrent)
        {
            Concurrent = concurrent;
            TypeRouter = new Router<Type, IConverterAction>();
            TypeMesh = new Mesh<Type, IConverterAction>(concurrent);
            TypeRouter.Meshes.Add(TypeMesh);
            CastChecker = new TypeCastChecker(concurrent);
            _defaultConverterAction = new ConverterAction((val) => { return null; });

            CleanCaches();
        }

        #region Public methods

        /// <summary>
        /// Set value converter action using Functor
        /// </summary>
        /// <typeparam name="T">Source type</typeparam>
        /// <typeparam name="TResult">Destination type</typeparam>
        /// <param name="func">Converter function</param>
        /// <param name="complexity">Expected complexity of conversion function</param>
        public void SetConverterAction<T, TResult>(Func<T, TResult> func, int complexity)
        {
            var action = new ConverterAction<T, TResult>(func);
            SetConverterAction<TResult>(typeof(T), action, complexity);
        }

        public void SetConverterAction<T, TResult>(IConverterAction<T, TResult> action, int complexity) 
        {
            SetConverterAction(typeof(T), typeof(TResult), action, complexity);
        }

        /// <summary>
        /// Set value converter action using Converter Action
        /// </summary>
        /// <typeparam name="TResult">Destination type</typeparam>
        /// <param name="source">Source type</param>
        /// <param name="action">Converter action</param>
        /// <param name="complexity">Expected complexity of conversion</param>
        public void SetConverterAction<TResult>(Type source, IConverterAction<TResult> action, int complexity) {
            var destination = typeof(TResult);
            SetConverterAction(source, destination, action, complexity);
        }

        /// <summary>
        /// Set value converter action using Converter Action
        /// </summary>
        /// <param name="source">Source type</param>
        /// <param name="destination">Destination type</param>
        /// <param name="action">Converter action</param>
        /// <param name="complexity">Expected complexity of conversion</param>
        public void SetConverterAction(Type source,
                                       Type destination,
                                       IConverterAction action,
                                       int complexity)
        {
            if (source.IsInterface || destination.IsInterface)
                throw new NotSupportedException("Value conversion between Interfaces does not supported.");

            TypeMesh.SetBridge(source, destination, action, complexity);
            UpdateBaseTypeNodes(source);
            UpdateBaseTypeNodes(destination);
            CleanCaches();
        }

        /// <summary>
        /// Removes converter action
        /// </summary>
        /// <param name="source">Source type</param>
        /// <param name="destination">Destination type</param>
        public void RemoveConverterAction(Type source, Type destination)
        {
            if (source.IsInterface || destination.IsInterface)
                throw new NotSupportedException("Value conversion between Interfaces does not supported.");

            TypeMesh.RemoveBridge(source, destination);
        }

        /// <summary>
        /// Set conversion between types allow or disallow. Typicaly use: 
        /// <code>SetDenyRule(typeof(object), typeof(int), true)</code>
        /// will block any method of convert object to int including string to object to int
        /// </summary>
        /// <param name="source">Source type</param>
        /// <param name="destination">Destination type</param>
        /// <param name="deny"><code>true</code>for block conversion</param>
        public void SetDenyRule(Type source, Type destination, bool deny = true)
        {
            CleanCaches();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert from one value type to another
        /// </summary>
        /// <typeparam name="TResult">Resulting type</typeparam>
        /// <param name="value">Value to convert</param>
        /// <param name="denyCastCheck">Force use converting rules, ingoring type casting</param>
        /// <returns>Converted value</returns>
        public TResult Convert<TResult>(object value, bool denyCastCheck = false)
        {
            var result = Convert(value, default(TResult), typeof(TResult), denyCastCheck);
            return (TResult)result;
        }

        /// <summary>
        /// Convert from one value type to another
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="nullValue">Return value if the conversion is impossible or input value is null</param>
        /// <param name="destination">Expected result value type</param>
        /// <param name="denyCastCheck">Force use converting rules, ingoring type casting</param>
        /// <returns>Converted value</returns>
        public object Convert(object value, object nullValue, Type destination, bool denyCastCheck = false) 
        {
            if(value == null)
                return nullValue;

            var source = value.GetType();

            if (!denyCastCheck && CastChecker.CheckIfCastable(source, destination))
                return value;

            var converterFunction = GetSummaryConverterFunction(source, destination);

            var result = converterFunction.Invoke(value);
            if (result == null)
                return nullValue;

            return result;
        }

        #endregion

        #region Private methods

        void UpdateBaseTypeNodes(Type type)
        {
            Type sourceType = type;
            var baseType = type.BaseType;
            while (baseType != null)
            {
                TypeMesh.SetParent(sourceType, baseType);
                sourceType = baseType;
                baseType = baseType.BaseType;
            }
        }

        Func<object, object> GetSummaryConverterFunction(Type source, Type destination)
        {
            Func<object, object> func;
            if (TryGetCachedConverterFunc(source, destination, out func))
                return func;

            func = BuildSummaryConverterFunction(source, destination);
            CacheConverterFunc(source, destination, func);

            return func;
        }

        Func<object, object> BuildSummaryConverterFunction(Type source, Type destination)
        {
            var path = TypeRouter.Route(source, destination);
            if (!path.IsResolved)
                return _defaultConverterAction.Convert;

            var bridgesList = new List<Func<object, object>>();
            foreach(var step in path.Steps)
                if(step.Bridge != null)
                    bridgesList.Add(step.Bridge.Convert);

            var bridges = bridgesList.ToArray();

            Func<object, object> summaryFunc = (val) =>
            {
                object result = val;
                for (int i = 0; i < bridges.Length; i++)
                    result = bridges[i](result);

                return result;
            };

            return summaryFunc;
        }

        void CacheConverterFunc(Type source, Type destination, Func<object, object> func)
        {
            var destinations = GetDestinationsFuncCache(source);
            destinations[destination] = func;
        }

        bool TryGetCachedConverterFunc(Type source, Type destination, out Func<object, object> func)
        {
            var destinations = GetDestinationsFuncCache(source);
            return destinations.TryGetValue(destination, out func);
        }

        IDictionary<Type, Func<object, object>> GetDestinationsFuncCache(Type source)
        {
            IDictionary<Type, Func<object, object>> destinations;
            if (!_summaryFunctCache.TryGetValue(source, out destinations))
            {
                if (Concurrent)
                    destinations = new ConcurrentDictionary<Type, Func<object, object>>();
                else
                    destinations = new Dictionary<Type, Func<object, object>>();

                _summaryFunctCache[source] = destinations;
            }
            
            return destinations;
        }

        void CleanCaches()
        {
            if (Concurrent)
                _summaryFunctCache = new ConcurrentDictionary<Type, IDictionary<Type, Func<object, object>>>();
            else
                _summaryFunctCache = new Dictionary<Type, IDictionary<Type, Func<object, object>>>();
        }

        #endregion
    }
}
