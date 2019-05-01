﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder.Extensions;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Flexo;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{
    internal static class ExpressionAssert
    {
        private static readonly ITreeRenderer<string> DebugViewRenderer = new PlainTextTreeRenderer();

        public static IConstant Equal<TValue>
        (
            TValue expected,
            IExpression expression,
            Func<IImmutableSession, IImmutableSession> configureContext = null,
            ITestOutputHelper output = default,
            bool throws = false
        )
        {
            var context = (configureContext ?? (ctx => ctx))(Expression.DefaultSession);

            if (throws)
            {
                Assert.ThrowsAny<DynamicException>(() => expression.Invoke(context));
                return default;
            }
            
            var actual = expression.Invoke(context);

            var debugViewString = DebugViewRenderer.Render(context.DebugView(), ExpressionDebugView.DefaultRender);

            try
            {
                switch (expected)
                {
                    case IEnumerable collection when !(typeof(TValue) == typeof(string)):
                        Assert.IsAssignableFrom<IEnumerable>(actual.Value);
                        Assert.Equal(collection.Cast<object>(), actual.Value<IEnumerable<IConstant>>().Values<object>());
                        break;
                    default:
                        Assert.Equal(expected, actual.Value);
                        break;
                }
            }
            catch (Exception)
            {
                output?.WriteLine(debugViewString);
                throw;
            }

            return actual;
        }
    }
}