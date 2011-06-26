using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Nop.Core.Domain.Shipping
{
    /// <summary>
    /// Represents a shipping option
    /// </summary>
    public partial class ShippingOption
    {
        /// <summary>
        /// Gets or sets the system name of shipping rate computation method
        /// </summary>
        public virtual string ShippingRateComputationMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets a shipping rate
        /// </summary>
        public virtual decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets a shipping option name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets a shipping option description
        /// </summary>
        public virtual string Description { get; set; }
    }


    public class ShippingOptionTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                ShippingOption shippingOption = null;
                string valueStr = value as string;
                if (!String.IsNullOrEmpty(valueStr))
                {
                    try
                    {
                        using (var tr = new StringReader(valueStr))
                        {
                            var xmlS = new XmlSerializer(typeof(ShippingOption));
                            shippingOption = (ShippingOption)xmlS.Deserialize(tr);
                        }
                    }
                    catch
                    {
                        //xml error
                    }
                }
                return shippingOption;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                ShippingOption shippingOption = value as ShippingOption;
                if (shippingOption != null)
                {
                    var sb = new StringBuilder();
                    using (var tw = new StringWriter(sb))
                    {
                        var xmlS = new XmlSerializer(typeof(ShippingOption));
                        xmlS.Serialize(tw, value);
                        string serialized = sb.ToString();
                        return serialized;
                    }
                }
                else
                {
                    return "";
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
