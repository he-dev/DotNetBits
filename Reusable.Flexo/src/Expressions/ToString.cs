using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    /// <summary>
    /// Converts Input to string. Uses InvariantCulture by default.
    /// </summary>
    public class ToString : ScalarExtension<string>
    {
        public ToString(ILogger<ToString> logger) : base(logger, nameof(ToString)) { }

        public IExpression Value { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        public IExpression Format { get; set; }

        protected override Constant<string> InvokeCore(IImmutableContainer context)
        {
            // Scope.This
            // Scope.Find("in") --> global object
            var format = Format?.Invoke(TODO).ValueOrDefault<string>() ?? "{0}";
            return (Name, string.Format(CultureInfo.InvariantCulture, format, Value.Invoke(TODO).Value));
        }
    }
}