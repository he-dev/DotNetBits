using System;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class CorrelationNode : LoggerNode, ILoggerScope<CorrelationNode.Scope, (object CorrelationId, object CorrelationHandle)>
    {
        public CorrelationNode() : base(false) { }

        /// <summary>
        /// Gets or sets the factory for the default correlation-id. By default it's a Guid.
        /// </summary>
        public Func<object> NextCorrelationId { get; set; } = () => Guid.NewGuid().ToString("N");

        public override bool Enabled => LoggerScope<Scope>.Any;

        public Scope Current => LoggerScope<Scope>.Current?.Value;
        
        public Scope Push((object CorrelationId, object CorrelationHandle) parameter)
        {
            return LoggerScope<Scope>.Push(new Scope
            {
                CorrelationId = parameter.CorrelationId ?? NextCorrelationId(),
                CorrelationHandle = parameter.CorrelationHandle
            }).Value;
        }

        protected override void InvokeCore(LogEntry request)
        {
            request.SetItem(LogEntry.Names.Scope, LogEntry.Tags.Serializable, LoggerScope<Scope>.Current.Enumerate().Select(x => x.Value).ToList());
            Next?.Invoke(request);
        }

        public class Scope : IDisposable
        {
            public object CorrelationId { get; set; }

            public object CorrelationHandle { get; set; }

            public void Dispose() => LoggerScope<Scope>.Current.Dispose();
        }
    }

    public static class LoggerCorrelationHelper
    {
        public static CorrelationNode.Scope UseScope(this ILogger logger, object correlationId = default, object correlationHandle = default)
        {
            return
                logger
                    .Node<CorrelationNode>()
                    .Push((correlationId, correlationHandle));
        }
        
        /// <summary>
        /// Gets the current correlation scope.
        /// </summary>
        public static CorrelationNode.Scope Scope(this ILogger logger)
        {
            return
                logger
                    .Node<CorrelationNode>()
                    .Current;
        }
    }
}