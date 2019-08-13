using System.Collections.Generic;
using System.Linq;

namespace Reusable.IOnymous
{
    public delegate IEnumerable<IResourceProvider> ResourceProviderFilterCallback(IEnumerable<IResourceProvider> providers, Request request);

    public static class ResourceProviderFilters
    {
        public static IEnumerable<IResourceProvider> FilterByProviderTags(this IEnumerable<IResourceProvider> providers, Request request)
        {
            if (!request.Context.Tags().Any())
            {
                return providers;
            }
            
            return
                from p in providers
                where p.Properties.Tags().Overlaps(request.Context.Tags())
                select p;
        }

        public static IEnumerable<IResourceProvider> FilterByUriScheme(this IEnumerable<IResourceProvider> providers, Request request)
        {
            var canFilter = !(request.Uri.IsRelative || (request.Uri.IsAbsolute && request.Uri.Scheme == UriSchemes.Custom.IOnymous));
            return
                from p in providers
                where !canFilter || p.Properties.GetSchemes().Overlaps(new[] { UriSchemes.Custom.IOnymous, request.Uri.Scheme })
                select p;
        }

        public static IEnumerable<IResourceProvider> FilterByUriPath(this IEnumerable<IResourceProvider> providers, Request request)
        {
            if (request.Uri.IsAbsolute)
            {
                return providers;
            }

            return
                from p in providers
                where p.SupportsRelativeUri()
                select p;
        }

//        public static IEnumerable<IResourceProvider> FilterByScheme(this IEnumerable<IResourceProvider> providers, Request request)
//        {
//            var canFilter = !(request.Uri.IsRelative || (request.Uri.IsAbsolute && request.Uri.Scheme == UriSchemes.Custom.IOnymous));
//            return providers.Where(p => !canFilter || p.Properties.GetSchemes().Overlaps(new[] { UriSchemes.Custom.IOnymous, request.Uri.Scheme }));
//        }
    }
}