﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class All : CollectionExtension<bool>, IFilter
    {
        public All() : base(default)
        {
            Matcher = new IsEqual
            {
                Value = Constant.True,
                Matcher = Constant.FromValue("Comparer", "Default")
            };
        }

        public IEnumerable<IExpression> Values
        {
            get => ThisInner;
            set => ThisInner = value;
        }

        [JsonProperty("Predicate")]
        public IExpression Matcher { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            return
                This(context)
                    .Enabled()
                    .Select(e => e.Invoke(context))
                    .All(c => this.Equal(context.BeginScopeWithThisOuter(c)));
        }
    }
}