using System;

namespace ADev.RoutedValueConverter
{
    public interface IConverterAction
    {
        object Convert(object value);
    }

    public interface IConverterAction<TResult> : IConverterAction
    {
        Type Destination { get; }
        new TResult Convert(object value);
    }
    public interface IConverterAction<T, TResult> : IConverterAction<TResult>
    {
        Type Source { get; }
        TResult Convert(T value);
    }
}
