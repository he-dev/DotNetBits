using System.Linq;
using System.Linq.Custom;
using Reusable.IOnymous;

namespace Reusable.Commander.Services
{
    internal static class ItemRequestFactory
    {
        // arg:///file/f?&position=1
        public static (UriString Uri, Metadata Metadata) CreateItemRequest(CommandParameterProperty item)
        {
            var queryParameters = new (SoftString Key, SoftString Value)[]
            {
                (CommandArgumentQueryStringKeys.Position, item.Position.ToString()),
                (CommandArgumentQueryStringKeys.IsCollection, item.IsCollection.ToString()),
            };
            var path = item.Id.Join("/").ToLower();
            var query =
                queryParameters
                    .Where(x => x.Value)
                    .Select(x => $"{x.Key.ToString()}={x.Value.ToString()}")
                    .Join("&");
            query = (SoftString)query ? $"?{query}" : string.Empty;
            return
            (
                $"arg:///{path}{query}",
                Metadata
                    .Empty
                    .Provider(s => s.DefaultName(nameof(CommandArgumentProvider)))
            );
        }
    }
}