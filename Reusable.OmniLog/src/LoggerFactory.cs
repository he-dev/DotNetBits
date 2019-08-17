using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public class LoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<SoftString, ILogger> _loggers;

        public LoggerFactory()
        {
            _loggers = new ConcurrentDictionary<SoftString, ILogger>();
        }

        public List<LoggerNode> Nodes { get; set; } = new List<LoggerNode>();

        #region ILoggerFactory

        public ILogger CreateLogger(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _loggers.GetOrAdd(name, n => new Logger(CreatePipeline(n.ToString())));
        }

        private LoggerNode CreatePipeline(string logger)
        {
            return Nodes.Aggregate<LoggerNode, LoggerNode>(new ConstantNode { Constants = { [nameof(Logger)] = logger } }, (current, next) => current.InsertNext(next)).First();
        }

        public void Dispose() { }

        #endregion
    }

    public static class LoggerFactoryExtensions
    {
        [NotNull]
        public static ILogger<T> CreateLogger<T>([NotNull] this ILoggerFactory loggerFactory, bool includeNamespace = false)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            return new Logger<T>(loggerFactory);
        }

        public static LoggerFactory Use<T>(this LoggerFactory loggerFactory, Action<T> configure = default) where T : LoggerNode, new()
        {
            var node = new T();
            configure?.Invoke(node);
            return loggerFactory.Use(node);
        }


        public static LoggerFactory Use<T>(this LoggerFactory loggerFactory, T node) where T : LoggerNode
        {
            loggerFactory.Nodes.Add(node);
            return loggerFactory;
        }
    }
}