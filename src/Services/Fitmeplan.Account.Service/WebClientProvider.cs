using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Fitmeplan.Account.Service
{
    public class WebClientProvider : IWebClientProvider
    {
        private readonly bool _isLocalStorage;
        public WebClientProvider(bool isLocalStorage)
        {
            _isLocalStorage = isLocalStorage;
        }

        public async Task<HttpResponseMessage> UploadData(string url, byte[] bytes, string fileName)
        {
            var client = new HttpClient();

            if (_isLocalStorage)
            {
                using var form = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                form.Add(fileContent, "file", fileName);

                return await client.PutAsync(url, form);
            }
            else
            {
                using var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fileContent.Headers.Add("x-ms-blob-type", "BlockBlob");

                return await client.PutAsync(url, fileContent);
            }
        }
    }
}
