using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace Fitmeplan.Storage.Azure
{
    public class AzureStorage : IStorage
    {
        private readonly double _expirePeriod;
        private readonly CloudBlobClient _client;
        private readonly CloudBlobContainer _tempContainer;
        private readonly CloudBlobContainer _mainContainer;
        private readonly CloudStorageAccount _account;

        public AzureStorage(AzureStorageConfiguration configuration)
        {
            _expirePeriod = configuration.TokenExpirePeriod;
            _account = CloudStorageAccount.Parse(configuration.ConnectionString);
            _client = _account.CreateCloudBlobClient();
            _tempContainer = _client.GetContainerReference(configuration.TempContainerName);
            _mainContainer = _client.GetContainerReference(configuration.MainContainerName);
        }

        public async Task<(string, string)> GenerateUploadUrl(string fileExtension, string route = null, string container = null, string fileName = null)
        {
            var currentContainer = string.IsNullOrEmpty(container) ? _tempContainer : _client.GetContainerReference(container);
            await currentContainer.CreateIfNotExistsAsync();
            var blobName = $"{Guid.NewGuid()}.{fileExtension}";
            var url = GenerateUrl(blobName, SharedAccessBlobPermissions.Write, currentContainer, fileName);
            return (url, blobName);
        }

        private string GenerateUrl(string blobName, SharedAccessBlobPermissions permission, CloudBlobContainer container, string fileName = null)
        {
            var adHocPolicy = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(_expirePeriod),
                Permissions = permission
            };
            string sasToken;
            if (!string.IsNullOrEmpty(fileName))
            {
                var blob = container.GetBlockBlobReference(blobName);
                sasToken = blob.GetSharedAccessSignature(adHocPolicy, new SharedAccessBlobHeaders
                {
                    ContentDisposition = "attachment; filename=" + fileName
                });
            }
            else
            {
                sasToken = container.GetSharedAccessSignature(adHocPolicy, null);
            }
            
            return $"{container.Uri}/{blobName}{sasToken}";
        }

        public async Task<string> CommitUpload(string route, string blobName)
        {
            await _mainContainer.CreateIfNotExistsAsync();
            var path = $"{route}/{blobName}";
            var tempBlob = _tempContainer.GetBlockBlobReference(blobName);
            var destinationBlob = _mainContainer.GetBlockBlobReference(path);
            using (var memoryStream = new MemoryStream())
            {
                await tempBlob.DownloadToStreamAsync(memoryStream);
                await destinationBlob.StartCopyAsync(tempBlob);
                await tempBlob.DeleteAsync();
            }

            return path;
        }

        public async Task<string> DeleteItem(string blobName, string container = null, string path = null)
        {
            var currentContainer = string.IsNullOrEmpty(container) ? _mainContainer : _client.GetContainerReference(container);
            await currentContainer.CreateIfNotExistsAsync();
            var deletedBlob = currentContainer.GetBlockBlobReference(blobName);
            await deletedBlob.DeleteAsync();
            return blobName;
        }

        public Task<string> GenerateDownloadUrl(string blobName, string route = null, string container = null, string fileName = null)
        {
            var currentContainer = string.IsNullOrEmpty(container) ? _mainContainer : _client.GetContainerReference(container);
            var url = GenerateUrl(blobName, SharedAccessBlobPermissions.Read, currentContainer, fileName);
            return Task.FromResult(url);
        }

        public async Task<byte[]> GetBlobContent(string blobName, string container = null, string path = null)
        {
            var currentContainer = string.IsNullOrEmpty(container) ? _mainContainer : _client.GetContainerReference(container);
            await currentContainer.CreateIfNotExistsAsync();
            var blockBlob = currentContainer.GetBlockBlobReference(blobName);

            using (var stream = new MemoryStream())
            {
                if (blockBlob.Exists())
                {
                    blockBlob.DownloadToStream(stream);
                }

                return stream.ToArray();
            }
        }
    }
}
