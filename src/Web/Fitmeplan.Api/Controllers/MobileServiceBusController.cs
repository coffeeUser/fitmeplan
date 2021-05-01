using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Fitmeplan.Api.Core;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.Api.Controllers
{
    [Authorize]
    [Route("api/sbm")]
    [ApiController]
    public class MobileServiceBusController : ControllerBase
    {
        public MobileMessageHandlerProvider MessageHandlerProvider { get; }

        public IConfiguration Configuration { get; set; }

        public MobileServiceBusController(MobileMessageHandlerProvider messageHandlerProvider, IConfiguration configuration)
        {
            MessageHandlerProvider = messageHandlerProvider;
            Configuration = configuration;
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

        private async Task<ActionResult> ExecuteAsync(string requestType, JContainer model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var responseModel = new ResponseModel { Status = StatusCodes.Success };

            var result = await MessageHandlerProvider.GetHandler(requestType).InvokeAsync(model);

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

        [AllowAnonymous]
        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            var info = new
            {
                email = Configuration["Support:Email"],
                subject = Configuration["Support:Subject"],
                body = Configuration["Support:Body"]
            };

            return new JsonResult(info);
        }
    }
}
