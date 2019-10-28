using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Temporarily holding log-entries while it's waiting to be transferred to another location. 
    /// </summary>
    public class BufferNode : LoggerNode, ILoggerNodeScope<BufferNode.Scope, object>
    {
        public BufferNode() : base(false) { }

        public override bool Enabled => AsyncScope<Scope>.Any;

        protected override void InvokeCore(LogEntry request)
        {
            AsyncScope<Scope>.Current.Value.Buffer.Enqueue(request);
            // Don't call Next until Commit.
        }
        
        public Scope Current => AsyncScope<Scope>.Current?.Value;

        public Scope Push(object parameter)
        {
            return AsyncScope<Scope>.Push(new Scope { Next = Next }).Value;
        }

        public Scope Push() => Push(default);

        public class Scope : IDisposable
        {
            internal Queue<LogEntry> Buffer { get; } = new Queue<LogEntry>();

            internal LoggerNode Next { get; set; }

            public void Flush()
            {
                while (Buffer.Any())
                {
                    Next?.Invoke(Buffer.Dequeue());
                }
            }

            public void Clear()
            {
                Buffer.Clear();
            }

            public void Dispose()
            {
                Buffer.Clear();
                AsyncScope<Scope>.Current.Dispose();
            }
        }
    }

    public static class BufferNodeHelper
    {
        // public static BufferNode.Scope UseBuffer(this ILogger logger)
        // {
        //     return
        //         logger
        //             .Node<BufferNode>()
        //             .Push(default);
        // }
        
        public static ILoggerScope UseBuffer(this ILogger logger)
        {
            return new LoggerScope<BufferNode>(logger, node => node.Push(default));
        }
        
        public static BufferNode.Scope Buffer(this ILogger logger)
        {
            return
                logger
                    .Node<BufferNode>()
                    .Current;
        }
    }
}