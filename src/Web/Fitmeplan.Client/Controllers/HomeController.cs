using Microsoft.AspNetCore.Mvc;

namespace Fitmeplan.Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        public IActionResult Index()
        {
            //return View();
            return File("~/dist/index.html", "text/html");
        }
    }
}