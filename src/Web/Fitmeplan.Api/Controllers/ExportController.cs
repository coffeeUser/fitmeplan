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
    public class ExportController : ControllerBase
    {
        public MessageHandlerProvider MessageHandlerProvider { get; }

        public ExportController(MessageHandlerProvider messageHandlerProvider)
        {
            MessageHandlerProvider = messageHandlerProvider;
        }

        [HttpPost("export/{commandType}")]
        public async Task<IActionResult> Export([FromRoute]string commandType, [FromBody]JContainer model)
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

            if (!result.Success)
            {
                responseModel.Status = StatusCodes.ValidationError;
                responseModel.Errors = result.Errors;
            }

            responseModel.Data = result.Data;
            return Ok(responseModel);
        }
    }
}