﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class All : PredicateExpression
    {
        public All() : base(nameof(All)) { }

        [JsonRequired]
        public IEnumerable<IExpression> Predicates { get; set; }

        protected override InvokeResult<bool> Calculate(IExpressionContext context)
        {
            var last = default(IExpression);
            foreach (var predicate in Predicates.Enabled())
            {
                last = predicate.Invoke(last?.Context ?? context);
                if (!last.Value<bool>())
                {
                    return InvokeResult.From(false, context);
                }
            }

            return InvokeResult.From(true, last?.Context ?? context);
        }
    }
}