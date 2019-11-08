using System.Collections.Generic;
using System.Linq;
using Reusable.Data;

namespace Reusable.Flexo
{
    public class ForEach : CollectionExtension<object>
    {
        public ForEach() : base(default) { }

        public IEnumerable<IExpression> Values { get => Arg; set => Arg = value; }

        public IEnumerable<IExpression> Body { get; set; }

        protected override object ComputeValue(IImmutableContainer context)
        {
            var query =
                from item in GetArg(context)
                from expr in Body.Enabled()
                select (item, expr);

            foreach (var (item, expr) in query)
            {
                expr.Invoke(context, ImmutableContainer.Empty.SetItem(ExpressionContext.Item, item));
            }

            return default;
        }
    }
}