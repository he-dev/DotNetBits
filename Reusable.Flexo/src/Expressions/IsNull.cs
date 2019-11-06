using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsNull : ScalarExtension<bool>
    {
        public IsNull(ILogger<IsNull> logger) : base(logger, nameof(IsNull)) { }
        
        public IExpression Left { get => ThisInner; set => ThisInner = value; }

        protected override bool InvokeAsValue(IImmutableContainer context)
        {
            return This(context).Invoke(context).Value is null;
        }
    }
}