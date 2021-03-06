﻿using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToUInt16Converter : TypeConverter<String, UInt16>
    {
        protected override UInt16 ConvertCore(IConversionContext<String> context)
        {
            return UInt16.Parse(context.Value, context.FormatProvider);
        }
    }

    public class UInt16ToStringConverter : TypeConverter<ushort, string>
    {
        protected override string ConvertCore(IConversionContext<UInt16> context)
        {
            return context.Value.ToString(context.FormatProvider);
        }
    }
}
