using System;

namespace ADev.RoutedValueConverter
{
    public static class BasicConverterActions
    {
        const string NullInputErrorMessage = "Input value is null";
        const string ParseErrorMessage = "Can not parse input value to desired type";
        const string OutOfRangeMessage = "Out of range";

        /// <summary>
        /// Object to string basic converter
        /// </summary>
        public static IConverterAction<object, string> ObjectToString
        {
            get
            {
                return new ConverterAction<object, string>((obj) => 
                { 
                    if(obj == null)
                        throw new ValueConversionException(obj, NullInputErrorMessage);

                    return obj.ToString(); 
                });
            }
        }

        #region From string

        public static IConverterAction<string, long> StringToLong
        {
            get
            {
                return new ConverterAction<string, long>((str) =>
                {
                    if(str == null)
                        throw new ValueConversionException(str, NullInputErrorMessage);

                    long result;
                    if (!long.TryParse(str, out result))
                        throw new ValueConversionException(str, ParseErrorMessage);

                    return result;
                });
            }
        }

        public static IConverterAction<string, ulong> StringToUnsignedLong
        {
            get
            {
                return new ConverterAction<string, ulong>((str) =>
                {
                    if (str == null)
                        throw new ValueConversionException(str, NullInputErrorMessage);

                    ulong result;
                    if (!ulong.TryParse(str, out result))
                        throw new ValueConversionException(str, ParseErrorMessage);

                    return result;
                });
            }
        }

        public static IConverterAction<string, int> StringToInt32
        {
            get
            {
                return new ConverterAction<string, int>((str) =>
                {
                    if (str == null)
                        throw new ValueConversionException(str, NullInputErrorMessage);

                    int result;
                    if (!int.TryParse(str, out result))
                        throw new ValueConversionException(str, ParseErrorMessage);

                    return result;
                });
            }
        }

        public static IConverterAction<string, uint> StringToUnsignedInt32
        {
            get
            {
                return new ConverterAction<string, uint>((str) =>
                {
                    if (str == null)
                        throw new ValueConversionException(str, NullInputErrorMessage);

                    uint result;
                    if (!uint.TryParse(str, out result))
                        throw new ValueConversionException(str, ParseErrorMessage);

                    return result;
                });
            }
        }

        public static IConverterAction<string, double> StringToDouble
        {
            get
            {
                return new ConverterAction<string, double>((str) =>
                {
                    if (str == null)
                        throw new ValueConversionException(str, NullInputErrorMessage);

                    double result;
                    if (!double.TryParse(str, out result))
                        throw new ValueConversionException(str, ParseErrorMessage);

                    return result;
                });
            }
        }

        public static IConverterAction<string, float> StringToFloat
        {
            get
            {
                return new ConverterAction<string, float>((str) =>
                {
                    if (str == null)
                        throw new ValueConversionException(str, NullInputErrorMessage);

                    float result;
                    if (!float.TryParse(str, out result))
                        throw new ValueConversionException(str, ParseErrorMessage);

                    return result;
                });
            }
        }

        public static IConverterAction<string, decimal> StringToDecimal
        {
            get
            {
                return new ConverterAction<string, decimal>((str) =>
                {
                    if (str == null)
                        throw new ValueConversionException(str, NullInputErrorMessage);

                    decimal result;
                    if (!decimal.TryParse(str, out result))
                        throw new ValueConversionException(str, ParseErrorMessage);

                    return result;
                });
            }
        } 

        public static IConverterAction<string, bool> StringToBoolean
        {
            get
            {
                return new ConverterAction<string, bool>((str) =>
                {
                    if (str == null)
                        throw new ValueConversionException(str, NullInputErrorMessage);

                    bool result;
                    if (!bool.TryParse(str, out result))
                        throw new ValueConversionException(str, ParseErrorMessage);

                    return result;
                });
            }
        }

        #endregion

        #region From int32

        public static IConverterAction<int, long> Int32ToLong
        {
            get
            {
                return new ConverterAction<int, long>((num) =>
                {
                    return num;
                });
            }
        }

        public static IConverterAction<int, uint> Int32ToUnsignedInt32
        {
            get
            {
                return new ConverterAction<int, uint>((num) =>
                {
                    return num >= 0 ? (uint)num : 0;
                });
            }
        }

        public static IConverterAction<int, double> Int32ToDouble
        {
            get
            {
                return new ConverterAction<int, double>((num) =>
                {
                    return num;
                });
            }
        }

        public static IConverterAction<int, float> Int32ToFloat
        {
            get
            {
                return new ConverterAction<int, float>((num) =>
                {
                    return num;
                });
            }
        }

        public static IConverterAction<int, decimal> Int32ToDecimal
        {
            get
            {
                return new ConverterAction<int, decimal>((num) =>
                {
                    return num;
                });
            }
        }

        public static IConverterAction<int, bool> Int32ToBoolean
        {
            get
            {
                return new ConverterAction<int, bool>((num) =>
                {
                    return num > 0;
                });
            }
        }

        #endregion

        #region From long

        public static IConverterAction<long, int> LongToInt32
        {
            get
            {
                return new ConverterAction<long, int>((num) =>
                {
                    if (num > int.MaxValue)
                        return int.MaxValue;

                    if (num < int.MinValue)
                        return int.MinValue;

                    return (int)num;
                });
            }
        }

        public static IConverterAction<long, ulong> LongToUnsignedInt32
        {
            get
            {
                return new ConverterAction<long, ulong>((num) =>
                {
                    return num >= 0 ? (ulong)num : 0;
                });
            }
        }

        public static IConverterAction<long, double> LongToDouble
        {
            get
            {
                return new ConverterAction<long, double>((num) =>
                {
                    return num;
                });
            }
        }

        public static IConverterAction<long, float> LongToFloat
        {
            get
            {
                return new ConverterAction<long, float>((num) =>
                {
                    return num;
                });
            }
        }

        public static IConverterAction<long, decimal> LongToDecimal
        {
            get
            {
                return new ConverterAction<long, decimal>((num) =>
                {
                    return num;
                });
            }
        }

        public static IConverterAction<long, bool> LongToBoolean
        {
            get
            {
                return new ConverterAction<long, bool>((num) =>
                {
                    return num > 0;
                });
            }
        }

        public static IConverterAction<long, DateTime> LongToDateTime
        {
            get
            {
                return new ConverterAction<long, DateTime>((ticks) =>
                {
                    if(ticks > DateTime.MaxValue.Ticks || ticks < DateTime.MinValue.Ticks)
                        throw new ValueConversionException(ticks, OutOfRangeMessage);

                    return new DateTime(ticks);
                });
            }
        }

        #endregion

        #region From double

        public static IConverterAction<double, int> DoubleToInt32
        {
            get
            {
                return new ConverterAction<double, int>((num) =>
                {
                    if (num > int.MaxValue)
                        return int.MaxValue;

                    if (num < int.MinValue)
                        return int.MinValue;

                    return (int)num;
                });
            }
        }

        public static IConverterAction<double, uint> DoubleToUnsignedInt32
        {
            get
            {
                return new ConverterAction<double, uint>((num) =>
                {
                    return num >= 0 ? (uint)num : 0;
                });
            }
        }

        public static IConverterAction<double, float> DoubleToFloat
        {
            get
            {
                return new ConverterAction<double, float>((num) =>
                {
                    if (num > float.MaxValue)
                        return float.MaxValue;

                    if (num < float.MinValue)
                        return float.MinValue;

                    return (float)num;
                });
            }
        }

        public static IConverterAction<double, decimal> DoubleToDecimal
        {
            get
            {
                return new ConverterAction<double, decimal>((num) =>
                {
                    return (decimal)num;
                });
            }
        }

        public static IConverterAction<double, bool> DoubleToBoolean
        {
            get
            {
                return new ConverterAction<double, bool>((num) =>
                {
                    return num > 0;
                });
            }
        }

        #endregion

        #region From DateTime

        public static IConverterAction<DateTime, long> DateTimeToLong
        {
            get
            {
                return new ConverterAction<DateTime, long>((datetime) =>
                {
                    return datetime.Ticks;
                });
            }
        }

        #endregion
    }
}
