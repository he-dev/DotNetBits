using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;

namespace Reusable.Flexo
{
    public class Collection : Expression<IEnumerable<IExpression>>
    {
        [JsonConstructor]
        public Collection(SoftString name) : base(name ?? nameof(Collection)) { }

        public List<IExpression> Values { get; set; }

        protected override Constant<IEnumerable<IExpression>> InvokeCore(IImmutableSession context)
        {
            return 
            (
                Name,
                Values.Enabled().Select((e, i) => Constant.FromNameAndValue($"Item-{i}", e.Invoke(context).Value)).ToList(),
                context
            );
        }
    }
}