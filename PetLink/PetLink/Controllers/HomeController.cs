using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PetLink.Models;

namespace PetLink.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        //temporário para questão de teste
        public IActionResult SignUp()
        {
            return View("~/Views/Profile/SignUpForm.cshtml");
        }


        public IActionResult ViewPet()
        {
            return View("~/Views/Home/AnimalPage.cshtml");
        }

    }
}
