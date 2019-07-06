using System;
using System.Collections.Immutable;
using System.Linq;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public static class ImmutableContainerExtensions
    {
        //private static readonly Selector<IImmutableSet<SoftString>> Schemes = From<IProviderProperties>.Select(x => x.Schemes);
        //private static readonly Selector<IImmutableSet<SoftString>> Names = From<IProviderProperties>.Select(x => x.Names);

        public static IImmutableContainer SetScheme(this IImmutableContainer container, SoftString scheme)
        {
            if (scheme.IsNullOrEmpty())
            {
                return container;
            }

            return
                container.SetItem(ResourceProvider.Property.Schemes, container.TryGetItem(ResourceProvider.Property.Schemes, out var schemes)
                    ? schemes.Add(scheme)
                    : ImmutableHashSet<SoftString>.Empty.Add(scheme));
        }

        public static IImmutableSet<SoftString> GetSchemes(this IImmutableContainer container)
        {
            return container.GetItemOrDefault(ResourceProvider.Property.Schemes, ImmutableHashSet<SoftString>.Empty);
        }

        public static IImmutableContainer SetName(this IImmutableContainer container, SoftString name)
        {
            if (name.IsNullOrEmpty())
            {
                return container;
            }

            return
                container.SetItem(
                    ResourceProvider.Property.Names,
                    container.TryGetItem(ResourceProvider.Property.Names, out var names)
                        ? names.Add(name)
                        : ImmutableHashSet<SoftString>.Empty.Add(name));
        }

        public static IImmutableSet<SoftString> GetNames(this IImmutableContainer container)
        {
            return container.GetItemOrDefault(ResourceProvider.Property.Names, ImmutableHashSet<SoftString>.Empty);
        }

        #region Resource

        public static IImmutableContainer SetUri(this IImmutableContainer container, UriString value) => container.SetItem(Resource.Property.Uri, value);
        public static IImmutableContainer SetExists(this IImmutableContainer container, bool value) => container.SetItem(Resource.Property.Exists, value);
        public static IImmutableContainer SetFormat(this IImmutableContainer container, MimeType value) => container.SetItem(Resource.Property.Format, value);
        
        public static IImmutableContainer SetDataType(this IImmutableContainer container, Type value) => container.SetItem(Resource.Property.DataType, value);

        public static UriString GetUri(this IImmutableContainer container) => container.GetItemOrDefault(Resource.Property.Uri);
        public static bool GetExists(this IImmutableContainer container) => container.GetItemOrDefault(Resource.Property.Exists);
        public static MimeType GetFormat(this IImmutableContainer container) => container.GetItemOrDefault(Resource.Property.Format, MimeType.None);
        public static Type GetDataType(this IImmutableContainer container) => container.GetItemOrDefault(Resource.Property.DataType);

        #endregion

        // Copies existing items from the specified session by T.
        public static IImmutableContainer Copy<T>(this IImmutableContainer container, From<T> fromType)
        {
            var copyable =
                from selector in fromType.Selectors()
                where container.ContainsKey(selector.ToString())
                select selector;

            return copyable.Aggregate(ImmutableContainer.Empty, (current, next) => current.SetItem(next.ToString(), container[next.ToString()]));
        }

        public static IImmutableContainer CopyRequestProperties(this IImmutableContainer container)
        {
            return container.Copy(Request.Property.This);
        }
        
        public static IImmutableContainer CopyResourceProperties(this IImmutableContainer container)
        {
            return container.Copy(Resource.Property.This);
        }
    }
}