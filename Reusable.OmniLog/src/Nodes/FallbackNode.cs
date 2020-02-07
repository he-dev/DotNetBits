using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    public class FallbackNode : LoggerNode
    {
        public override bool Enabled => base.Enabled && Defaults?.Any() == true;

        public Dictionary<SoftString, object> Defaults { get; set; } = new Dictionary<SoftString, object>();

        protected override void invoke(ILogEntry request)
        {
            foreach (var (key, value) in Defaults.Select(x => (x.Key, x.Value)))
            {
                if (!request.TryGetProperty(key, out var property) && property.CanProcessWith<EchoNode>())
                {
                    request.Add(key, value, m => m.ProcessWith<EchoNode>());
                }
            }

            invokeNext(request);
        }
    }
}