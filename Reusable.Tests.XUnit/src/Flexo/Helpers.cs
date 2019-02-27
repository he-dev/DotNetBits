﻿using System;
using JetBrains.Annotations;
using Reusable.Exceptionizer;
using Reusable.Flexo;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{
    internal static class Helpers
    {
        public static void Equal<TValue, TExpression>(TValue expectedValue, TExpression expression, IExpressionContext context = null) where TExpression : IExpression
        {
            var expected = expectedValue is IConstant constant ? constant : Constant.Create(expression.Name, expectedValue);
            var actual = expression.Invoke(context ?? new ExpressionContext());

            if (!expected.Equals(actual))
            {
                throw DynamicException.Create("AssertFailed", CreateAssertFailedMessage(expected, actual));
            }
        }

        private static string CreateAssertFailedMessage(object expected, object actual)
        {
            return
                $"{Environment.NewLine}" +
                $"» Expected:{Environment.NewLine}{expected}{Environment.NewLine}" +
                $"» Actual:{Environment.NewLine}{actual}" +
                $"{Environment.NewLine}";
        }
    }
}