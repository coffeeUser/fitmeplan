using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fitmeplan.Identity.Security.Jwt;

namespace Fitmeplan.Storage.Local
{
    public class LocalStorage : IStorage
    {
        private readonly TokenProvider _tokenProvider;
        private readonly LocalStorageConfiguration _localStorageConfiguration;

        public LocalStorage(TokenProvider tokenProvider, LocalStorageConfiguration localStorageConfiguration)
        {
            _tokenProvider = tokenProvider;
            _localStorageConfiguration = localStorageConfiguration;
        }

        public Task<(string, string)> GenerateUploadUrl(string fileExtension, string route = null, string container = null, string fileName = null)
        {
            var blobName = string.IsNullOrEmpty(fileName) ? $"{Guid.NewGuid()}.{fileExtension}" : 
                Path.HasExtension(fileName) ? $"{fileName}" : $"{fileName}.{fileExtension}";
            var token = GenerateToken(blobName, route, fileName);
            var url = string.Concat(_localStorageConfiguration.SpaClientUrl, "/api/sb/upload/", token);
            return Task.FromResult((url, blobName));
        }

        private string GenerateToken(string blobName, string route, string fileName)
        {
            var claims = new Dictionary<string, object>
            {
                { "blobName", blobName },
                { "route", route },
                { "filename", fileName }
            };
            return _tokenProvider.GenerateSecurityToken(claims);
        }

        public Task<string> CommitUpload(string route, string blobName)
        {
            var path = $@"{route}/{blobName}";
            return Task.FromResult(path);
        }

        public Task<string> DeleteItem(string blobName, string container = null, string path = null)
        {
            var folderPath = string.IsNullOrEmpty(path)
                ? _localStorageConfiguration.StoragePath
                : Path.Combine(_localStorageConfiguration.StoragePath, path);
            var fullPath = Path.Combine(folderPath, blobName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            return Task.FromResult(blobName);
        }

        public Task<string> GenerateDownloadUrl(string blobName, string route = null, string container = null, string fileName = null)
        {
            var token = GenerateToken(blobName, route, fileName);
            var url = string.Concat(_localStorageConfiguration.SpaClientUrl, "/api/sb/download/", token);
            return Task.FromResult(url);
        }

        public Task<byte[]> GetBlobContent(string blobName, string container = null, string path = null)
        {
            var folderPath = string.IsNullOrEmpty(path)
                ? _localStorageConfiguration.StoragePath
                : Path.Combine(_localStorageConfiguration.StoragePath, path);
            var fullPath = Path.Combine(folderPath, blobName);
            var bytes = File.Exists(fullPath) ? File.ReadAllBytes(fullPath) : new byte[0];

            return Task.FromResult(bytes);
        }
    }
}
