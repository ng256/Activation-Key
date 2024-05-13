/***************************************************************

•   File: ActivationKeyConverter.cs

•   Description.

	ActivationKeyConverter  is a converter for the ActivationKey
	data  type. It allows  you    to  convert objects    of type
	ActivationKey to and from other datatypes.

***************************************************************/

using System.ComponentModel;
using System.Globalization;

namespace System.Security.Activation
{
    /// <summary>
    /// Converts <see cref = "ActivationKey" /> between other types.
    /// </summary> 
    public sealed class ActivationKeyConverter : TypeConverter
    {
        /// <inheritdoc cref="TypeConverter.CanConvertFrom(Type)"/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string) || sourceType == typeof(byte[]))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc cref="TypeConverter.CanConvertTo(Type)"/>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string) || destinationType == typeof(byte[]))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        /// <inheritdoc cref="TypeConverter.ConvertFrom(ITypeDescriptorContext, CultureInfo, object)"/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            switch (value)
            {
                case string str:
                    return new ActivationKey(str);
                case byte[] bytes:
                    return new ActivationKey(bytes);
                default:
                    return base.ConvertFrom(context, culture, value);
            }
        }

        /// <inheritdoc cref="TypeConverter.ConvertTo(ITypeDescriptorContext, CultureInfo, object, Type)"/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            switch (value)
            {
                case ActivationKey activationKey when destinationType == typeof(string):
                    return activationKey.ToString();
                case ActivationKey activationKey when destinationType == typeof(byte[]):
                    return activationKey.ToBinary();
                default:
                    return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}