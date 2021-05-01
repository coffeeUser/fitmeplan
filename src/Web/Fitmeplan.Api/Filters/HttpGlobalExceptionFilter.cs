using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Fitmeplan.Api.Core.Security;

namespace Fitmeplan.Api.Filters
{
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HttpGlobalExceptionFilter> _logger;

        public HttpGlobalExceptionFilter(IWebHostEnvironment env, ILogger<HttpGlobalExceptionFilter> logger)
        {
            _env = env;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(new EventId(context.Exception.HResult),
                context.Exception,
                context.Exception.Message);

            if (context.Exception is NotAuthorizedException)
            {
                context.Result = new ForbidResult();
                context.HttpContext.Response.StatusCode = (int) HttpStatusCode.Forbidden;
            }
            else
            {
                var model = new ResponseModel()
                {
                    Status = "500",
                    Message = "An error occurred. Try it again."
                };

                if (!_env.IsProduction())
                {
                    model.Message = context.Exception.ToString();
                }

                context.Result = new JsonResult(model);
                context.HttpContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            }

            context.ExceptionHandled = true;
        }
    }
}