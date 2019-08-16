using System;

namespace Reusable.Translucent
{
    public abstract class ResourceActionAttribute : Attribute
    {
        protected ResourceActionAttribute(RequestMethod method) => Method = method;

        public RequestMethod Method { get; }
    }

    public class ResourceGetAttribute : ResourceActionAttribute
    {
        public ResourceGetAttribute() : base(RequestMethod.Get) { }
    }

    public class ResourcePostAttribute : ResourceActionAttribute
    {
        public ResourcePostAttribute() : base(RequestMethod.Post) { }
    }

    public class ResourcePutAttribute : ResourceActionAttribute
    {
        public ResourcePutAttribute() : base(RequestMethod.Put) { }
    }

    public class ResourceDeleteAttribute : ResourceActionAttribute
    {
        public ResourceDeleteAttribute() : base(RequestMethod.Delete) { }
    }
}