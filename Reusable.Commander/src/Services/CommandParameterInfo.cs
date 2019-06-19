using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.IOnymous;
using Reusable.Quickey;

namespace Reusable.Commander.Services
{
    internal class CommandParameterInfo : ResourceInfo
    {
        private readonly List<string> _values;

        public CommandParameterInfo([NotNull] UriString uri, bool exists, List<string> values)
            : base(uri, ImmutableSession.Empty.SetItem(From<IResourceMeta>.Select(x => x.Format), MimeType.Binary))
        {
            Exists = exists;
            _values = values;
        }

        public override bool Exists { get; }
        public override long? Length => _values.Count;
        public override DateTime? CreatedOn { get; }
        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            // It's easier to handle arguments as json because it takes care of all conversions.
            //using (var data = await ResourceHelper.SerializeAsJsonAsync(_values))
            using (var data = await ResourceHelper.SerializeBinaryAsync(_values))
            {
                await data.Rewind().CopyToAsync(stream);
            }
        }
    }
}