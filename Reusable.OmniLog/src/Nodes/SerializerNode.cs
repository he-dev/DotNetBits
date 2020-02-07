using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Nodes
{
    public class SerializerNode : LoggerNode
    {
        private readonly ISerializer _serializer;

        public SerializerNode(ISerializer serializer) => _serializer = serializer;

        public SerializerNode() : this(new JsonSerializer()) { }

        /// <summary>
        /// Gets or sets serializable properties. If empty then all items are scanned.
        /// </summary>
        //public HashSet<string> SerializableProperties { get; set; } = new HashSet<string>(SoftString.Comparer);
        protected override void invoke(ILogEntry request)
        {
            foreach (var property in request.Where(LogProperty.CanProcess.With(this)).Where(LogProperty.ValueIs.NotNull()).ToList())
            {
                request.Add(property.Name, _serializer.Serialize(property.Value!), m => m.ProcessWith<EchoNode>());
            }

            invokeNext(request);
        }
    }

    public static class LoggerSerializerHelper
    {
        // public static LogEntry Serializable(this LogEntry logEntry, string propertyName, object obj)
        // {
        //     return logEntry.SetItem(propertyName, SerializationNode.LogItemTag, obj);
        // }
    }
}