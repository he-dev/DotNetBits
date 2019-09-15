using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Flawless.ExpressionVisitors;
using Reusable.Flawless.Helpers;

namespace Reusable.Flawless
{
    using exprfac = ValidationExpressionFactory;

    public delegate TResult ValidationFunc<in T, out TResult>(T obj, IImmutableContainer context);

    public delegate TResult AggregateDelegate<TElement, TResult>(IEnumerable<TElement> source, Func<TElement, TResult> selector);

    public interface IValidatorBuilder<T> : ICollection<IValidator<T>>
    {
        //IValidationRuleBuilder<T, TElement> When(Expression<ValidationFunc<TElement, bool>> when);
        IValidatorBuilder<T> Not();
        IValidatorBuilder<T> Predicate(Expression<Func<T, bool>> predicate);
        IValidatorBuilder<T> Error();
        IValidatorBuilder<T> Message(Expression<ValidationFunc<T, string>> message);
        IValidatorBuilder<T> Tags(params string[] tags);
        IValidator<T> Build();
    }

    public interface IValidatorBuilder<T, TSource, TElement> : IValidatorBuilder<TElement>
    {
        //IValidatorBuilder<TElement> Predicate(Expression<Func<TElement, bool>> predicate);
        //IValidator<T> Build();
    }

    public class ValidatorBuilder<T, TSource, TElement> : List<IValidator<TElement>>, IValidatorBuilder<T, TSource, TElement>
    {
        private readonly Func<IEnumerable<IValidator<TElement>>, IValidator<T>> _validatorFactory;

        //private static readonly Expression<ValidationFunc<TRight, bool>> DefaultWhen = (x, ctx) => true;
        private static readonly Expression<ValidationFunc<TElement, string>> DefaultMessage = (x, ctx) => default;

        private readonly IList<ValidationRuleTemplate> _validationRuleTemplates;

        //private Expression<ValidationFunc<TRight, bool>> _when;
        private bool _negate;

        public ValidatorBuilder
        (
            Func<IEnumerable<IValidator<TElement>>, IValidator<T>> validatorFactory
        )
        {
            _validatorFactory = validatorFactory;
            _validationRuleTemplates = new List<ValidationRuleTemplate>();
            //_when = DefaultWhen;
            _negate = false;
        }

        protected IEnumerable<ValidationRuleTemplate> ValidationRuleTemplates => _validationRuleTemplates;

//        public IValidationRuleBuilder<T, TElement> When(Expression<ValidationFunc<TElement, bool>> when)
//        {
//            _when = when;
//            return this;
//        }

        public IValidatorBuilder<TElement> Not()
        {
            _negate = true;
            return this;
        }

        public IValidatorBuilder<TElement> Predicate(Expression<Func<TElement, bool>> predicate)
        {
            _validationRuleTemplates.Add(new ValidationRuleTemplate
            {
                //When = _when,
                Predicate = _negate ? exprfac.Not(predicate) : predicate,
                Message = DefaultMessage,
            });

            _negate = false;
            // _when = DefaultWhen;

            return this;
        }

        public IValidatorBuilder<TElement> Error()
        {
            _validationRuleTemplates.Last().Required = true;
            return this;
        }

        public IValidatorBuilder<TElement> Message(Expression<ValidationFunc<TElement, string>> message)
        {
            //_validationRuleTemplates.Last().Message = message;
            return this;
        }

        public IValidatorBuilder<TElement> Tags(params string[] tags)
        {
            _validationRuleTemplates.Last().Tags = tags.ToImmutableSortedSet(SoftString.Comparer);
            return this;
        }

        [NotNull]
        public IValidator<TElement> Build()
        {
            /*
             
             There are two expressions:
                 selector: (x, ctx) => x.FirstName
                 predicate: x => bool;
             
             that need to be called in chain like that:
                 var value = selector(obj, context); 
                 var result = predicate(value);
             
             so I build this expression:
                 return predicate(selector(obj, context));
                 
             */
//            var rules =
//                from item in ValidationRuleTemplates
//                // Need to recreate all expressions so that they use the same parameters the Selector uses.
//                //let when = Expression.Lambda<ValidationFunc<T, bool>>(item.When.Body, Selector.Parameters)
//                // predicate(selector(obj, context))
//                let predicate =
//                    Expression.Lambda<ValidationFunc<TLeft, bool>>(
//                        Expression.Invoke(item.Predicate,
//                            Expression.Invoke(Selector, Selector.Parameters)),
//                        Selector.Parameters)
//                let message = Expression.Lambda<ValidationFunc<TLeft, string>>(item.Message.Body, Selector.Parameters)
//                select (IValidator<TLeft>)new ValidationRule<TLeft, TValue>(_selector, item.When, predicate, message, item.Required, item.Tags);

            //rules = rules.ToList();

//            foreach (var rule in rules.Concat(this.SelectMany(b => b.Build())))
//            {
//                yield return rule;
//            }

            return _validatorFactory(Enumerable.Empty<IValidator<TElement>>());
        }

        protected class ValidationRuleTemplate
        {
            //public Expression<ValidationFunc<TElement, bool>> When { get; set; }

            public Expression<Func<TElement, bool>> Predicate { get; set; }

            public Expression<ValidationFunc<TElement, string>> Message { get; set; }

            public IImmutableSet<string> Tags { get; set; } = ImmutableHashSet<string>.Empty;

            public bool Required { get; set; }
        }
    }

    public class ScalarValidatorBuilder<T, TValue> : ValidatorBuilder<,,>
    {
        private readonly Expression<ValidationFunc<T, TValue>> _selector;

        public ScalarValidatorBuilder(IValidatorBuilder<T> parent, Expression<ValidationFunc<T, TValue>> selector) : base(parent, selector)
        {
            _selector = selector;
        }

        public static ScalarValidatorBuilder<T, TNext> Create<TNext>(IValidatorBuilder<T> parent, Expression<Func<TValue, TNext>> selector)
        {
            var validate = selector.AddContextParameter();
            var injected = ObjectInjector.Inject(validate, parent.Selector.Body);
            return new ScalarValidatorBuilder<T, TNext>(parent, Expression.Lambda<ValidationFunc<T, TNext>>(injected, parent.Selector.Parameters));
        }

        public override IValidator<T> Build()
        {
            /*
             
             There are two expressions:
                 selector: (x, ctx) => x.FirstName
                 predicate: x => bool;
             
             that need to be called in chain like that:
                 var value = selector(obj, context); 
                 var result = predicate(value);
             
             so I build this expression:
                 return predicate(selector(obj, context));
                 
             */
            var rules =
                from item in ValidationRuleTemplates
                // Need to recreate all expressions so that they use the same parameters the Selector uses.
                //let when = Expression.Lambda<ValidationFunc<T, bool>>(item.When.Body, Selector.Parameters)
                // predicate(selector(obj, context))
                let predicate =
                    Expression.Lambda<ValidationFunc<T, bool>>(
                        Expression.Invoke(item.Predicate,
                            Expression.Invoke(Selector, Selector.Parameters)),
                        Selector.Parameters)
                let message = Expression.Lambda<ValidationFunc<T, string>>(item.Message.Body, Selector.Parameters)
                select (IValidator<T>)new ValidationRule<T, TValue>(_selector, item.When, predicate, message, item.Required, item.Tags);

            //rules = rules.ToList();

            foreach (var rule in rules.Concat(this.SelectMany(b => b.Build())))
            {
                yield return rule;
            }
        }
    }

    public class CollectionValidatorBuilder<T, TValue> : ValidatorBuilder<,,>
    {
        private readonly Expression<ValidationFunc<T, IEnumerable<TValue>>> _selector;

        private readonly Expression<AggregateDelegate<TValue, bool>> _aggregate;

        public CollectionValidatorBuilder
        (
            IValidatorBuilder<T> parent,
            Expression<ValidationFunc<T, IEnumerable<TValue>>> selector,
            AggregateDelegate<TValue, bool> aggregateDelegate
        ) : base(parent, selector)
        {
            _selector = selector;

            // The aggregateDelegate needs to be converted into a lambda-expression.
            var aggregateParameters = new[]
            {
                Expression.Parameter(typeof(IEnumerable<TValue>), "source"),
                Expression.Parameter(typeof(Func<TValue, bool>), "predicate")
            };
            _aggregate =
                Expression.Lambda<AggregateDelegate<TValue, bool>>(
                    Expression.Call(
                        aggregateDelegate.Method,
                        aggregateParameters.Cast<Expression>()),
                    aggregateParameters);
        }

        public static IValidatorBuilder<> Create<TNext>
        (
            IValidatorBuilder<T> parent,
            Expression<Func<TValue, IEnumerable<TNext>>> selector,
            AggregateDelegate<TNext, bool> aggregateDelegate
        )
        {
            var collectionSelectorWithContext = selector.AddContextParameter();
            var collectionSelectorFull = ObjectInjector.Inject(collectionSelectorWithContext, parent.Selector.Body);
            var lambda = Expression.Lambda<ValidationFunc<T, IEnumerable<TNext>>>(collectionSelectorFull, parent.Selector.Parameters);

            return new CollectionValidatorBuilder<T, TNext>(parent, lambda, aggregateDelegate);
        }

        public override IValidator<T> Build()
        {
            /*
                Building
                
                (T obj, TContext context) =>
                {
                    var collection = getCollection(obj, context);
                    return aggregate(collection, x => predicate(x, context));
                };
                
                as
                
                (T obj, TContext context) => aggregate(getCollection(obj, context), x => predicate(x, context));
             
             */

            var rules =
                from item in ValidationRuleTemplates
                // Need to recreate all expressions so that they use the same parameters the Selector uses.
                let predicate =
                    Expression.Lambda<ValidationFunc<T, bool>>(
                        Expression.Invoke(
                            _aggregate,
                            Expression.Invoke(Selector, Selector.Parameters), item.Predicate),
                        Selector.Parameters)
                //let when = Expression.Lambda<ValidationFunc<T, bool>>(item.When.Body, Selector.Parameters)
                let message = Expression.Lambda<ValidationFunc<T, string>>(item.Message.Body, Selector.Parameters)
                select (IValidator<T>)new ValidationRule<T, IEnumerable<TValue>>(_selector, item.When, predicate, message, item.Required, item.Tags);

            //rules = rules.ToList();

            foreach (var rule in rules.Concat(this.SelectMany(b => b.Build())))
            {
                yield return rule;
            }
        }
    }

    public interface IValidatorModule<T>
    {
        void Build(IValidatorBuilder<T> builder);
    }

    public static class ValidatorBuilderExtensions
    {
        public static IValidatorBuilder<TElement, TNextSource, TNextSource> Validate<T, TSource, TElement, TNextSource>
        (
            this IValidatorBuilder<T, TSource, TElement> builder,
            Expression<Func<TElement, TNextSource>> sourceSelector
        )
        {
            var validatorFactory = Validator<TElement, TNextSource, TNextSource>.CreateFactory(x => new[] { x }, x => x, (x, ctx) => true);
            return new ValidatorBuilder<TElement, TNextSource, TNextSource>(validators => validatorFactory(validators));
            // todo - convert selector to enumerable
            return default;
        }

        public static IValidatorBuilder<TElement, TNextSource, TNextElement> Validate___<T, TSource, TElement, TNextSource, TNextElement>
        (
            this IValidatorBuilder<T, TSource, TElement> builder,
            Expression<Func<TElement, IEnumerable<TNextSource>>> sourceSelector,
            Expression<Func<TNextSource, TNextElement>> elementSelector
        )
        {
            //new ValidationRuleCollection<TCurrent, TNext, TNext>(selector, x => x, (x, ctx) => true, );
            var validatorFactory = Validator<TElement, TNextSource, TNextElement>.CreateFactory(sourceSelector, elementSelector, (x, ctx) => true);
            var next = new ValidatorBuilder<TElement, TNextSource, TNextElement>(validators => validatorFactory(validators));
            //builder.Add(next);
            return next;

            //var rules = new ValidationRuleCollection<TCurrent, TNext, TNext>(selector, x => x, (x, ctx) => true, default);
            //return new ValidationRuleBuilder<TNext>();
        }

        public static void Validate<T, TCurrent, TNext>(this IValidatorBuilder<> builder, Expression<Func<TCurrent, TNext>> selector, Action<IValidatorBuilder<>> configureBuilder)
        {
            configureBuilder(ScalarValidatorBuilder<T, TCurrent>.Create(builder, selector));
        }

        public static IValidatorBuilder<> Validate<T, TCurrent, TNext>
        (
            this IValidatorBuilder<T> builder,
            Expression<Func<TCurrent, IEnumerable<TNext>>> selector,
            AggregateDelegate<TNext, bool> aggregateDelegate
        )
        {
            return CollectionValidatorBuilder<T, TCurrent>.Create(builder, selector, aggregateDelegate);
        }

        public static IValidatorBuilder<> ValidateAll<T, TCurrent, TNext>(this IValidatorBuilder<> builder, Expression<Func<TCurrent, IEnumerable<TNext>>> selector)
        {
            return builder.Validate(selector, Enumerable.All);
        }

        public static IValidatorBuilder<> ValidateAny<T, TCurrent, TNext>(this IValidatorBuilder<> builder, Expression<Func<TCurrent, IEnumerable<TNext>>> selector)
        {
            return builder.Validate(selector, Enumerable.Any);
        }

        public static Expression<ValidationFunc<T, TValue>> AddContextParameter<T, TValue>(this Expression<Func<T, TValue>> expression)
        {
            // x => x.Member --> (x, context) => x.Member
            return
                Expression.Lambda<ValidationFunc<T, TValue>>(
                    expression.Body,
                    expression.Parameters.Single(),
                    Expression.Parameter(typeof(IImmutableContainer), "context"));
        }
    }

    // y => y.Member --> x => x.Member.Member
    public class ObjectInjector : ExpressionVisitor
    {
        private readonly Expression _firstParameter;
        private readonly Expression _inject;

        private ObjectInjector(Expression firstParameter, Expression inject)
        {
            _firstParameter = firstParameter;
            _inject = inject;
        }

        public static Expression Inject(LambdaExpression lambda, Expression inject)
        {
            return new ObjectInjector(lambda.Parameters[0], inject).Visit(lambda.Body);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return
                node.Expression == _firstParameter
                    ? Expression.MakeMemberAccess(_inject, node.Member)
                    : base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return
                node.Object == _firstParameter
                    ? Expression.Call(_inject, node.Method, node.Arguments)
                    : base.VisitMethodCall(node);
        }
    }

    public class ReplaceLambda<T, TValue> : ExpressionVisitor
    {
        public static Expression<ValidationFunc<T, TValue>> Invoke(LambdaExpression lambda)
        {
            return (Expression<ValidationFunc<T, TValue>>)new ReplaceLambda<T, TValue>().Visit(lambda);
        }

        protected override Expression VisitLambda<TCurrent>(Expression<TCurrent> node)
        {
            return Expression.Lambda<ValidationFunc<T, TValue>>(Visit(node.Body), node.Parameters);
        }
    }
}