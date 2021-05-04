using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fitmeplan.Client.Controllers
{
    [Route("app")]
    [AllowAnonymous]
    public class AppController : ControllerBase
    {
        [Route("info")]
        public IActionResult GetUser()
        {
            var user = new
            {
                version = typeof(AppController).Assembly.GetName().Version.ToString()
            };

            return new JsonResult(user);
        }
    }
}