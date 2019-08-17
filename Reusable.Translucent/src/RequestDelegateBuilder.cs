using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public delegate Task RequestDelegate<in TContext>(TContext context);

    public class RequestDelegateBuilder
    {
        private readonly IServiceProvider _services;

        private readonly Stack<(Type MiddlewareType, object[] Parameters)> _middleware = new Stack<(Type MiddlewareType, object[] Parameters)>();

        public RequestDelegateBuilder(IServiceProvider services)
        {
            _services = services;
        }

        public RequestDelegateBuilder UseMiddleware<T>(params object[] args)
        {
            _middleware.Push((typeof(T), args));
            return this;
        }

        public RequestDelegate<TContext> Build<TContext>()
        {
            if (!_middleware.Any())
            {
                throw new InvalidOperationException($"Cannot build {nameof(RequestDelegate<TContext>)} because there are no middleware.");
            }

            var previous = default(object);
            while (_middleware.Any())
            {
                var current = _middleware.Pop();
                var nextCallback = CreateNext<TContext>(previous);
                var parameters = new object[] { nextCallback };
                if (current.Parameters.Any())
                {
                    parameters = parameters.Concat(current.Parameters).ToArray();
                }

                var middlewareCtor = current.MiddlewareType.GetConstructor(parameters.Select(p => p.GetType()).ToArray());
                if (middlewareCtor is null)
                {
                    throw DynamicException.Create
                    (
                        "ConstructorNotFound",
                        $"Type '{current.MiddlewareType.ToPrettyString()}' does not have a constructor with these parameters: [{parameters.Select(p => p.GetType().ToPrettyString()).Join(", ")}]"
                    );
                }

                previous = middlewareCtor.Invoke(parameters);
            }

            return CreateNext<TContext>(previous);
        }

        // Using this helper to "catch" the "previous" middleware before it goes out of scope and is overwritten by the loop.
        private RequestDelegate<TContext> CreateNext<TContext>(object middleware)
        {
            // This is the last last middleware and there is nowhere to go from here.
            if (middleware is null)
            {
                return _ => Task.CompletedTask;
            }

            var invokeMethods = new[]
            {
                middleware.GetType().GetMethod("InvokeAsync"),
                middleware.GetType().GetMethod("Invoke")
            };

            var nextInvokeMethod = invokeMethods.Where(Conditional.IsNotNull).SingleOrThrow
            (
                onEmpty: () => DynamicException.Create("InvokeNotFound", $"{middleware.GetType().ToPrettyString()} must implement either 'InvokeAsync' or 'Invoke'."),
                onMany: () => DynamicException.Create("AmbiguousInvoke", $"{middleware.GetType().ToPrettyString()} must implement either 'InvokeAsync' or 'Invoke' but not both.")
            );

            var parameters = nextInvokeMethod.GetParameters();

            if (parameters.First().ParameterType != typeof(TContext))
            {
                throw DynamicException.Create
                (
                    "InvokeSignature",
                    $"{middleware.GetType().ToPrettyString()} Invoke(Async)'s first parameters must be of type '{typeof(RequestDelegate<TContext>).ToPrettyString()}'."
                );
            }

            return context =>
            {
                var parameterValues =
                    parameters
                        .Skip(1) // TContext is always there.
                        .Select(parameter => _services.Resolve(parameter.ParameterType)) // Resolve other Invoke(Async) parameters.
                        .Prepend(context);

                // Call the actual invoke with its parameters.
                return (Task)nextInvokeMethod.Invoke(middleware, parameterValues.ToArray());
            };
            //return next.CreateDelegate<RequestCallback<TContext>>(middleware);
        }
    }

    public interface IServiceProvider
    {
        bool IsRegistered(Type type);

        object Resolve(Type type);
    }

    public class LambdaServiceProvider : IServiceProvider
    {
        private readonly IImmutableDictionary<Type, object> _services;

        public LambdaServiceProvider(Action<IDictionary<Type, object>> configure)
        {
            var services = new Dictionary<Type, object>();
            configure(services);
            _services = services.ToImmutableDictionary();
        }

        public bool IsRegistered(Type type) => _services.ContainsKey(type);

        public object Resolve(Type type)
        {
            return
                _services.TryGetValue(type, out var service)
                    ? service
                    : throw DynamicException.Create("ServiceNotFound", $"There is no service of type '{type.ToPrettyString()}'.");
        }
    }

    public class AutofacServiceProvider : IServiceProvider
    {
        private readonly IComponentContext _componentContext;

        public AutofacServiceProvider(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        public bool IsRegistered(Type type) => _componentContext.IsRegistered(type);

        public object Resolve(Type type) => _componentContext.Resolve(type);
    }

    public static class MethodInfoExtensions
    {
        public static T CreateDelegate<T>(this MethodInfo method, object target) where T : Delegate
        {
            return (T)method.CreateDelegate(typeof(T), target);
        }
    }
}