﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class PhysicalFileProvider : ResourceProvider
    {
        public PhysicalFileProvider(ResourceProviderMetadata metadata = null)
            : base(
                (metadata ?? ResourceProviderMetadata.Empty)
                    .Add(ResourceProviderMetadataKeyNames.CanGet, true)
                    .Add(ResourceProviderMetadataKeyNames.CanPut, true)
                    .Add(ResourceProviderMetadataKeyNames.CanDelete, true)
            )
        { }

        public override Task<IResourceInfo> GetAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            return Task.FromResult<IResourceInfo>(new PhysicalFileInfo(uri));
        }

        public override async Task<IResourceInfo> PutAsync(SimpleUri uri, Stream value, ResourceProviderMetadata metadata = null)
        {
            try
            {
                using (var fileStream = new FileStream(uri.Path, FileMode.CreateNew, FileAccess.Write))
                {
                    await value.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }
                return await GetAsync(uri, metadata);
            }
            catch (Exception inner)
            {
                throw CreateException(this, uri.Path, metadata, inner);
            }

        }

        public override async Task<IResourceInfo> DeleteAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            try
            {
                File.Delete(uri.Path);
                return await GetAsync(uri, metadata);
            }
            catch (Exception inner)
            {
                throw CreateException(this, uri.Path, metadata, inner);
            }
        }
    }
}
