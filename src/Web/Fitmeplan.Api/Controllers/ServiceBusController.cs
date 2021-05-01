using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Fitmeplan.Api.Core;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.Api.Controllers
{
    [Authorize]
    [Route("api/sb")]
    [ApiController]
    public class ServiceBusController : ControllerBase
    {
        public MessageHandlerProvider MessageHandlerProvider { get; }
        public DictionaryHandlerProvider DictionaryHandlerProvider { get; set; }

        public ServiceBusController(MessageHandlerProvider messageHandlerProvider, DictionaryHandlerProvider dictionaryHandlerProvider)
        {
            MessageHandlerProvider = messageHandlerProvider;
            DictionaryHandlerProvider = dictionaryHandlerProvider;
        }

        [HttpPost("query/{queryType}")]
        public async Task<ActionResult> Query([FromRoute]string queryType, [FromBody]JContainer model)
        {
            return await ExecuteAsync(queryType, model);
        }

        [HttpPost("command/{commandType}")]
        public async Task<ActionResult> Post([FromRoute]string commandType, [FromBody]JContainer model)
        {
            return await ExecuteAsync(commandType, model);
        }

        [HttpPut("file/{commandType}"), DisableRequestSizeLimit]
        public async Task<ActionResult> File([FromRoute]string commandType)
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

            byte[] fileContent;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileContent = memoryStream.ToArray();
            }

            return await ProcessFileAsync(commandType, fileContent);
        }

        [HttpPost("file/{commandType}")]
        public async Task<ActionResult> File([FromRoute]string commandType, [FromBody]JContainer model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var responseModel = new ResponseModel { Status = StatusCodes.Success };

            var result = await MessageHandlerProvider.GetHandler(commandType).InvokeAsync(model);

            if (result.Exception != null)
            {
                throw new ServiceBusException(result.Exception);
            }

            if (result.Success)
            {
                responseModel.Data = result.Data;
                responseModel.Status = StatusCodes.Redirect;
                return Redirect((string)responseModel.Data);
            }

            responseModel.Status = StatusCodes.ValidationError;
            responseModel.Errors = result.Errors;

            return Ok(responseModel);
        }

        [HttpPost("lookup/{lookupName}")]
        public async Task<ActionResult> Dict([FromRoute]string lookupName, [FromBody]JObject model)
        {
            var responseModel = new ResponseModel();
            responseModel.Data = await DictionaryHandlerProvider.InvokeAsync(lookupName, model);
            return Ok(responseModel);
        }

        private async Task<ActionResult> ProcessFileAsync(string requestType, byte[] fileContent)
        {
            var responseModel = new ResponseModel{Status = StatusCodes.Success};

            var result = await MessageHandlerProvider.GetHandler(requestType).InvokeAsync(fileContent);

            if (result.Exception != null)
            {
                throw new ServiceBusException(result.Exception);
            }

            if (!result.Success)
            {
                responseModel.Status = StatusCodes.ValidationError;
                responseModel.Errors = result.Errors;
                //foreach (var error in result.Errors)
                //{
                //    LocalizedString localizedString = _localizer[error.ErrorMessage];
                //    error.Message = error.MessageParameters != null ? string.Format(CultureInfo.CurrentCulture, localizedString, error.MessageParameters) : localizedString;
                //    responseModel.Errors.Add(error);
                //}

                //responseModel.Status = result.Status == StatusCodes.Gone
                //                       || result.Status == StatusCodes.Forbidden
                //                       || result.Status == StatusCodes.Conflict
                //    ? result.Status
                //    : StatusCodes.ValidationError;
            }
            
            responseModel.Data = result.Data;

            return Ok(responseModel);
        }

        private async Task<ActionResult> ExecuteAsync(string requestType, JContainer model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var responseModel = new ResponseModel{Status = StatusCodes.Success};

            var result = await MessageHandlerProvider.GetHandler(requestType).InvokeAsync(model);

            if (result.Exception != null)
            {
                throw new ServiceBusException(result.Exception);
            }

            if (!result.Success)
            {
                responseModel.Status = StatusCodes.ValidationError;
                responseModel.Errors = result.Errors;
                //foreach (var error in result.Errors)
                //{
                //    LocalizedString localizedString = _localizer[error.ErrorMessage];
                //    error.Message = error.MessageParameters != null ? string.Format(CultureInfo.CurrentCulture, localizedString, error.MessageParameters) : localizedString;
                //    responseModel.Errors.Add(error);
                //}

                //responseModel.Status = result.Status == StatusCodes.Gone
                //                       || result.Status == StatusCodes.Forbidden
                //                       || result.Status == StatusCodes.Conflict
                //    ? result.Status
                //    : StatusCodes.ValidationError;
            }

            responseModel.Data = result.Data;

            return Ok(responseModel);
        }
    }

    public static class StatusCodes
    {
        public const string Success = "200";
        public const string Redirect = "302";
        public const string BadRequest = "400";
        public const string ValidationError = "400.1";
        public const string Forbidden = "403.1";
        public const string NotFound = "404";
        public const string NotFoundRedirect = "404.1";
        public const string Conflict = "409.1";
        public const string Gone = "410.1";
    }
}
