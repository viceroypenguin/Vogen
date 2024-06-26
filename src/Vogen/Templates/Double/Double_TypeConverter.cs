﻿
        class VOTYPETypeConverter : global::System.ComponentModel.TypeConverter
        {
            public override global::System.Boolean CanConvertFrom(global::System.ComponentModel.ITypeDescriptorContext context, global::System.Type sourceType)
            {
                return sourceType == typeof(global::System.Double) || sourceType == typeof(global::System.String) || base.CanConvertFrom(context, sourceType);
            }

            public override global::System.Object ConvertFrom(global::System.ComponentModel.ITypeDescriptorContext context, global::System.Globalization.CultureInfo culture, global::System.Object value)
            {
                return value switch
                {
                    global::System.Double doubleValue => VOTYPE.__Deserialize(doubleValue),
                    global::System.Int64 longValue => VOTYPE.__Deserialize((global::System.Double)longValue),
                    global::System.String stringValue when !global::System.String.IsNullOrEmpty(stringValue) && global::System.Double.TryParse(stringValue, global::System.Globalization.NumberStyles.Float | global::System.Globalization.NumberStyles.AllowThousands, culture.NumberFormat, out var result) => VOTYPE.__Deserialize(result),
                    _ => base.ConvertFrom(context, culture, value),
                };
            }

            public override bool CanConvertTo(global::System.ComponentModel.ITypeDescriptorContext context, global::System.Type sourceType)
            {
                return sourceType == typeof(global::System.Double) || sourceType == typeof(global::System.String) || base.CanConvertTo(context, sourceType);
            }

            public override object ConvertTo(global::System.ComponentModel.ITypeDescriptorContext context, global::System.Globalization.CultureInfo culture, global::System.Object value, global::System.Type destinationType)
            {
                if (value is VOTYPE idValue)
                {
                    if (destinationType == typeof(global::System.Double))
                    {
                        return idValue.Value;
                    }

                    if (destinationType == typeof(global::System.String))
                    {
                        return idValue.Value.ToString(culture);
                    }
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }