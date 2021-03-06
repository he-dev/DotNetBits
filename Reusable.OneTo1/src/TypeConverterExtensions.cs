﻿using System;
using System.Diagnostics;

namespace Reusable.OneTo1
{
    public static class TypeConverterExtensions
    {
        [DebuggerStepThrough]
        public static object Convert(this ITypeConverter converter, object value, Type toType, string format, IFormatProvider formatProvider)
        {
            return converter.Convert(new ConversionContext<object>(value, toType, converter)
            {
                Format = format,
                FormatProvider = formatProvider,
            });
        }

        [DebuggerStepThrough]
        public static object Convert(this ITypeConverter converter, object value, Type toType)
        {
            return converter.Convert(new ConversionContext<object>(value, toType, converter));
        }

        public static T Convert<T>(this ITypeConverter converter, object value)
        {
            return (T)converter.Convert(value, typeof(T));
        }
    }
}