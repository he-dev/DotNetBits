﻿using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToGuidConverter : TypeConverter<String, Guid>
    {
        protected override Guid Convert(IConversionContext<string> context)
        {
            return Guid.Parse(context.Value);
        }
    }

    public class GuidToStringConverter : TypeConverter<Guid, String>
    {
        protected override string Convert(IConversionContext<Guid> context)
        {
            return
                string.IsNullOrEmpty(context.Format)
                    ? context.Value.ToString()
                    : context.Value.ToString(context.Format, context.FormatProvider);
        }
    }
}
