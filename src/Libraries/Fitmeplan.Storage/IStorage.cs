using System.Threading.Tasks;

namespace Fitmeplan.Storage
{
    public interface IStorage
    {
        Task<(string, string)> GenerateUploadUrl(string fileExtension, string route = null, string container = null, string fileName = null);
        Task<string> CommitUpload(string route, string blobName);
        Task<string> DeleteItem(string blobName, string container = null, string path = null);
        Task<string> GenerateDownloadUrl(string blobName, string route = null, string container = null, string fileName = null);
        Task<byte[]> GetBlobContent(string blobName, string container = null, string path = null);
    }
}
