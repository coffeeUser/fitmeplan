using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Fitmeplan.Identity.Security.Jwt;
using Fitmeplan.Storage.Local;

namespace Fitmeplan.Api.Controllers
{
    //todo: zhi: fake controller for local storage, should be deleted
    [Route("api/sb")]
    [ApiController]
    public class LocalStorageController : ControllerBase
    {
        private readonly TokenProvider _tokenProvider;
        private readonly LocalStorageConfiguration _localStorageConfiguration;

        public LocalStorageController(TokenProvider tokenProvider, LocalStorageConfiguration localStorageConfiguration)
        {
            _tokenProvider = tokenProvider;
            _localStorageConfiguration = localStorageConfiguration;
        }

        [HttpPut("upload/{token}"), DisableRequestSizeLimit]
        public async Task<ActionResult> UploadFile([FromRoute]string token)
        {
            if (Request.Form.Files.Count == 0)
            {
                return BadRequest();
            }

            var file = Request.Form.Files[0];

            if (!(file.Length > 0))
            {
                return BadRequest();
            }

            return await WriteFileAsync(token, file);
        }

        private async Task<ActionResult> WriteFileAsync(string token, IFormFile file)
        {
            var responseModel = new ResponseModel { Status = StatusCodes.Success };
            var claims = _tokenProvider.GetClaimsFromToken(token);

            var blobName = claims.TryGetValue("blobName", out object blob) ? (string)blob : null;
            var path = claims.TryGetValue("route", out object route) ? (string)route : null;

            if (blobName == null || path == null)
            {
                return BadRequest();
            }

            var dirPath = Path.Combine(_localStorageConfiguration.StoragePath, path);
            Directory.CreateDirectory(dirPath);
            var fullPath = Path.Combine(dirPath, blobName);
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Ok(responseModel);
        }

        [HttpGet("download/{token}")]
        public IActionResult DownloadFile([FromRoute]string token)
        {
            return ReadFile(token);
        }

        private IActionResult ReadFile(string token)
        {
            var claims = _tokenProvider.GetClaimsFromToken(token);

            var blobName = claims.TryGetValue("blobName", out object blob) ? (string)blob : null;
            var fileName = claims.TryGetValue("filename", out object file) ? (string)file : null;

            if (blobName == null)
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_localStorageConfiguration.StoragePath, blobName);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var net = new System.Net.WebClient();
            var data = net.DownloadData(fullPath);
            var content = new MemoryStream(data);
            var contentType = "APPLICATION/octet-stream";
            return File(content, contentType, fileName ?? blobName);
        }
    }
}