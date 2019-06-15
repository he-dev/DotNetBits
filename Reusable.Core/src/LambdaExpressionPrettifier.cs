using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Flawless;

namespace Reusable
{
    // We don't want to show the exact same expression as the condition
    // because there are variables and closures that don't look pretty.
    // We replace them with more friendly names.
    internal class LambdaExpressionPrettifier : ExpressionVisitor
    {
        private readonly ParameterExpression _originalParameter;

        private readonly ParameterExpression _replacementParameter;

        private LambdaExpressionPrettifier(ParameterExpression originalParameter, ParameterExpression replacementParameter)
        {
            _originalParameter = originalParameter;
            _replacementParameter = replacementParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node.Equals(_originalParameter) ? _replacementParameter : base.VisitParameter(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // Extract member name from closures.
            if (node.Expression is ConstantExpression)
            {
                return Expression.Parameter(node.Type, node.Member.Name);
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            // Remove type conversion, this is change (Convert(<T>) != null) to (<T> != null)
            if (node.Operand.Type == _originalParameter.Type)
            {
                return Expression.Parameter(node.Operand.Type, _replacementParameter.Name);
            }

            return base.VisitUnary(node);
        }

        public static Expression Prettify<T>([NotNull] Expression<Func<T, bool>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var replacementParameter = Expression.Parameter(typeof(T), $"<{typeof(T).ToPrettyString()}>");
            return new LambdaExpressionPrettifier(expression.Parameters[0], replacementParameter).Visit(expression.Body);
        }
        
        public static Expression Prettify([NotNull] LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var parameterType = expression.Parameters.Single().Type;
            var replacementParameter = Expression.Parameter(parameterType, $"<{parameterType.ToPrettyString()}>");
            return new LambdaExpressionPrettifier(expression.Parameters[0], replacementParameter).Visit(expression.Body);
        }
    }

    public static class ExpressionExtension
    {
        public static string ToPrettyString(this LambdaExpression expression) => LambdaExpressionPrettifier.Prettify(expression).ToString();
    }
}