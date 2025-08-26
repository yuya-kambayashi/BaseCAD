﻿using System.ComponentModel;
using System.Globalization;

namespace BaseCAD.Graphics
{
    public class ColorConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            string str = value as string;

            if (str == null) return false;

            if (str.StartsWith("#"))
                str = str.Substring(1);

            if (uint.TryParse(str, out uint _))
                return true;

            if (Enum.TryParse(str, out KnownColor _))
                return true;

            return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string str = value as string;

            if (str != null)
            {
                if (str.StartsWith("#"))
                    str = str.Substring(1);

                if (uint.TryParse(str, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out uint argb))
                    return Color.FromArgb(argb);

                if (Enum.TryParse(str, out KnownColor knownColor))
                    return Color.FromKnownColor(knownColor);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Color)
            {
                Color col = (Color)value;
                if (col.IsKnownColor())
                    return col.ToKnownColor().ToString();
                else
                    return col.ToHex();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}