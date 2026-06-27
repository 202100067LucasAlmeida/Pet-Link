using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;

namespace PetLink.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método responsável por carregar a página inicial.
        // Obtém os 4 anúncios de animais mais recentes da base de dados e envia-os para a View da homepage.
        public async Task<IActionResult> Index()
        {
            // Vai buscar os 4 animais mais recentes
            var recentPets = await _context.AnimalListings
                .OrderByDescending(a => a.CreatedAt)
                .Take(4)
                .ToListAsync();

            // Envia a lista de animais recentes para a View da homepage
            return View(recentPets);
        }

        // !! DEV !!
        public IActionResult TestError()
        {
            throw new Exception("Isto é um teste para forçar o erro 500!");
        }

        // Carrega a página de política de privacidade
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        // Método utilizado para mostrar a página de erro da aplicação
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Policies()
        {
            return View();
        }

        public IActionResult HowItWorks()
        {
            return View();
        }

        public IActionResult SuccessStories()
        {
            return View();
        }
    }
}