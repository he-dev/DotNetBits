using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous.Mail
{
    public abstract class MailProvider : ResourceProvider
    {
        protected MailProvider(IImmutableContainer metadata) : base(metadata.SetScheme(UriSchemes.Known.MailTo)) { }

        protected async Task<string> ReadBodyAsync(Stream value, IImmutableContainer metadata)
        {
            using (var bodyReader = new StreamReader(value, metadata.GetItemOrDefault(MailRequestContext.BodyEncoding, Encoding.UTF8)))
            {
                return await bodyReader.ReadToEndAsync();
            }
        }
    }
    
    [Rename(nameof(MailRequestContext))]
    public class MailRequestContext : SelectorBuilder<MailRequestContext>
    {
        public static Selector<string> From = Select(() => From);

        public static Selector<IList<string>> To = Select(() => To);

        public static Selector<IList<string>> CC = Select(() => CC);

        public static Selector<string> Subject = Select(() => Subject);

        public static Selector<IDictionary<string, byte[]>> Attachments = Select(() => Attachments);

        public static Selector<Encoding> BodyEncoding = Select(() => BodyEncoding);

        public static Selector<bool> IsHtml = Select(() => IsHtml);

        public static Selector<bool> IsHighPriority = Select(() => IsHighPriority);
    }

    internal class MailResource : Resource
    {
        private readonly Stream _response;

        internal MailResource(IImmutableContainer properties, Stream response)
            : base(properties.SetExists(response != Stream.Null))
        {
            _response = response;
        }

        //public override bool Exists => !(_response is null);

        //public override long? Length => _response?.Length;
        
        public override async Task CopyToAsync(Stream stream)
        {
            await _response.Rewind().CopyToAsync(stream);
        }

        public override void Dispose()
        {
            _response.Dispose();
        }
    }
}