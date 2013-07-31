using System;
using System.Runtime.Serialization;
using System.Security.Permissions;


namespace ADev.RoutedValueConverter
{
    [Serializable]
    public class ValueConversionException : Exception
    {
        const string ConversationMessage = "Input value can not be converted to expected type.";

        public object SourceValue { get; private set; }
        public IConverterAction Converter { get; private set; }

        public ValueConversionException(object sourceValue)
            : base(ConversationMessage)
        {
            SourceValue = sourceValue;
        }

        public ValueConversionException(object sourceValue, string message)
            : base(String.Format("{0} {1}", ConversationMessage, message))
        {
            SourceValue = sourceValue;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
            {
                info.AddValue("SourceValue", SourceValue);
                info.AddValue("Converter", Converter.ToString());
            }
        }

    }
}
