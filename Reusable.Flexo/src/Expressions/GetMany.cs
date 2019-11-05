using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class GetMany : GetItem<IEnumerable<IExpression>>
    {
        public GetMany([NotNull] ILogger<GetMany> logger) : base(logger, nameof(GetMany)) { }

        protected override Constant<IEnumerable<IExpression>> InvokeCore(IImmutableContainer context)
        {
            var items = (IEnumerable<object>)FindItem();
            return Constant.FromEnumerable(Path, items); // items.Select((x, i) => Constant.FromValue<object>($"Item[{i}]", x)).ToList());
        }
    }
}