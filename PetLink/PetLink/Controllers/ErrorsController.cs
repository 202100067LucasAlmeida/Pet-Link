using Microsoft.AspNetCore.Mvc;

namespace PetLink.Controllers
{
    public class ErrorsController : Controller
    {
        [Route("Errors/ServerFault")]
        public IActionResult ServerFault()
        {
            return View();
        }

        [Route("Errors/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Route("Errors/Status")]
        public IActionResult Status(int code)
        {
            if (code == 404)
            {
                return View("FileMissing");
            }

            return View("ServerFault");
        }
    }
}