﻿using System;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    // ReSharper disable once InconsistentNaming - we want this name!
    public class IIf : Expression
    {
        public IIf() : base(nameof(IIf)) { }

        [JsonRequired]
        public IExpression Predicate { get; set; }

        public IExpression True { get; set; }

        public IExpression False { get; set; }

        public override IExpression Invoke(IExpressionContext context)
        {
            if (True is null && False is null) throw new InvalidOperationException($"You need to specify at least one result ({nameof(True)}/{nameof(False)}).");

            using (context.Scope(this))
            {
                var result =
                    Predicate.InvokeWithValidation(context).Value<bool>()
                        ? (True ?? Constant.Null)
                        : (False ?? Constant.Null);


                return result.InvokeWithValidation(context);
            }
        }
    }
}