using System;

namespace ADev.RoutedValueConverter
{
    /// <summary>
    /// Functor-based converter action. Used in Routable ValueConverter as action for conversion rule
    /// </summary>
    public class ConverterAction : IConverterAction
    {
        readonly Func<object, object> ConverterFunc;
        readonly object DefaultResult;

        protected ConverterAction() { }
        /// <summary>
        /// Creates Converter action
        /// </summary>
        /// <param name="converterFunc">Converter function</param>
        /// <param name="defaultResult">Default return value if conversion is inpossible or input value is null</param>
        public ConverterAction(Func<object, object> converterFunc, object defaultResult = null)
        {
            ConverterFunc = converterFunc;
            DefaultResult = defaultResult;
        }

        protected void ReportConvertError(object value)
        {
            // does somthing here
        }

        #region IConverterAction members
        
        /// <summary>
        /// Convert value using action function
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>Converted value</returns>
        public virtual object Convert(object value)
        {
            if (value == null)
                return DefaultResult;

            return ConverterFunc(value);
        }

        #endregion
    }

    /// <summary>
    /// Functor-based converter action. Used in Routable ValueConverter as action for conversion rule
    /// </summary>
    /// <typeparam name="TResult">Output value type</typeparam>
    public class ConverterAction<TResult> : ConverterAction, IConverterAction<TResult>
    {
        static Type _destination;
        readonly Func<object, TResult> ConverterFunc;

        protected ConverterAction() { }

        /// <summary>
        /// Creates Converter action
        /// </summary>
        /// <param name="converterFunc">Converter function</param>
        public ConverterAction(Func<object, TResult> converterFunc)
        {
            ConverterFunc = converterFunc;
        }

        #region ConverterAction overrides + IConverterAction<TResult> members

        /// <summary>
        /// Conversion result type
        /// </summary>
        public Type Destination
        {
            get { return _destination; }
        }

        /// <summary>
        /// Convert value using action function
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>Converted value</returns>
        public new TResult Convert(object value)
        {
            if (value == null)
                return default(TResult);

            return ConverterFunc(value);
        }

        #endregion

        static ConverterAction()
        {
            _destination = typeof(TResult);
        }
    }

    /// <summary>
    /// Functor-based converter action. Used in Routable ValueConverter as action for conversion rule
    /// </summary>
    /// <typeparam name="T">Input value type</typeparam>
    /// <typeparam name="TResult">Output value type</typeparam>
    public class ConverterAction<T, TResult> : ConverterAction<TResult>, IConverterAction<T, TResult>
    {
        static Type _source;
        Func<T, TResult> ConverterFunc;

        /// <summary>
        /// Creates Converter action
        /// </summary>
        /// <param name="converterFunc">Converter function</param>
        public ConverterAction(Func<T, TResult> converterFunc)
        {
            ConverterFunc = converterFunc;
        }

        #region ConverterAction<TResult> overrides

        /// <summary>
        /// Convert value using action function
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>Converted value</returns>
        public new object Convert(object value)
        {
            return Convert((T)value);
        }

        #endregion

        #region IConverterAction<T, TResult> members

        /// <summary>
        /// Converter source value type
        /// </summary>
        public Type Source
        {
            get { return _source; }
        }

        /// <summary>
        /// Convert value using action function
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returnsConverted value></returns>
        public TResult Convert(T value)
        {
            return ConverterFunc(value);
        }

        #endregion

        static ConverterAction()
        {
            _source = typeof(T);
        }
    }
}
