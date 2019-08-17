using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Matches : ValueExtension<bool>
    {
        public Matches(ILogger<Matches> logger) : base(logger, nameof(Matches)) { }

        public bool IgnoreCase { get; set; } = true;
        
        public IExpression Value { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        public IExpression Pattern { get; set; }

        protected override Constant<bool> InvokeCore()
        {
            var pattern = Pattern.Invoke().Value<string>();
            var options = IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            return (Name, Regex.IsMatch(Value.Invoke().Value<string>(), pattern, options));
        }
    }
}