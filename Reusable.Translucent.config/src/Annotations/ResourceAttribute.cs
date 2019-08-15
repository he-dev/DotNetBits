using System;
using JetBrains.Annotations;

namespace Reusable.Translucent.Annotations
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public class ResourceAttribute : Attribute
    {
        public string Scheme { get; set; }

        public string Provider { get; set; }
    }
}