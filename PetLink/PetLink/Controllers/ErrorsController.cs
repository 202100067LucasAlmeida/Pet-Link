using Microsoft.AspNetCore.Mvc;
using PetLink.Data;

namespace PetLink.Controllers
{
    public class ErrorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ErrorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> FileMissing()
        {
            return View();
        }

        public async Task<IActionResult> ServerFault()
        {
            return View();
        }
    }
}
