using System.Net.Http;
using System.Threading.Tasks;

namespace Fitmeplan.Account.Service
{
    public interface IWebClientProvider
    {
        Task<HttpResponseMessage> UploadData(string url, byte[] bytes, string fileName);
    }
}
