﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class PhysicalDirectoryProvider : ResourceProvider
    {
        public PhysicalDirectoryProvider(ResourceMetadata metadata = null)
            : base(
                (metadata ?? ResourceMetadata.Empty)
                    .Add(ResourceMetadataKeys.CanGet, true)
                    .Add(ResourceMetadataKeys.CanPut, true)
                    .Add(ResourceMetadataKeys.CanDelete, true)
            )
        { }

        public override Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            ValidateScheme(uri, "file");

            return Task.FromResult<IResourceInfo>(new PhysicalDirectoryInfo(uri));
        }

        public override async Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            ValidateScheme(uri, "file");

            using (var streamReader = new StreamReader(value))
            {
                var fullName = Path.Combine(uri.Path, await streamReader.ReadToEndAsync());
                try
                {
                    Directory.CreateDirectory(fullName);
                    return await this.GetAsync(fullName, metadata);
                }
                catch (Exception inner)
                {
                    throw CreateException(this, fullName, metadata, inner);
                }
            }
        }

        public override async Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null)
        {
            ValidateScheme(uri, "file");

            try
            {
                Directory.Delete(uri.Path, true);
                return await GetAsync(uri, metadata);
            }
            catch (Exception inner)
            {
                throw CreateException(this, uri.Path, metadata, inner);
            }
        }
    }
    
    [PublicAPI]
    internal class PhysicalDirectoryInfo : ResourceInfo
    {
        public PhysicalDirectoryInfo([NotNull] UriString uri) : base(uri) { }

        public override UriString Uri { get; }

        public override bool Exists => Directory.Exists(Uri.Path);

        public override long? Length { get; }

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn => Exists ? Directory.GetLastWriteTimeUtc(Uri.Path) : default;

        public override Task CopyToAsync(Stream stream) => throw new NotSupportedException();

        public override Task<object> DeserializeAsync(Type targetType) => throw new NotSupportedException();
    }
}