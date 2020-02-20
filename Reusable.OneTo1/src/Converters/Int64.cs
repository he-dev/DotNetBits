﻿using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToInt64Converter : TypeConverter<String, Int64>
    {
        protected override long Convert(IConversionContext<string> context)
        {
            return Int64.Parse(context.Value, NumberStyles.Integer, context.FormatProvider);
        }
    }

    public class Int64ToStringConverter : TypeConverter<long, string>
    {
        protected override string Convert(IConversionContext<long> context)
        {
            return context.Value.ToString(context.FormatProvider);
        }
    }
}
