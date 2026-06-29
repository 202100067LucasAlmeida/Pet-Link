using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;

namespace PetLink.Controllers
{
    /// <summary>
    /// Controlador responsável pelas páginas gerais da aplicação.
    /// Gere a página inicial, páginas informativas e de suporte.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador da página inicial.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Apresenta a página inicial da aplicação
        /// com os quatro anúncios de animais publicados mais recentemente.
        /// </summary>
        /// <returns>Vista da página inicial com os anúncios mais recentes.</returns>
        public async Task<IActionResult> Index()
        {
            var recentPets = await _context.AnimalListings
                .OrderByDescending(a => a.CreatedAt)
                .Take(4)
                .ToListAsync();

            return View(recentPets);
        }

        /// <summary>
        /// Apresenta a página de política de privacidade.
        /// </summary>
        /// <returns>Vista da política de privacidade.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Apresenta a página de erro da aplicação.
        /// O resultado não é armazenado em cache para garantir
        /// que o identificador do pedido é sempre atual.
        /// </summary>
        /// <returns>Vista de erro com o identificador do pedido atual.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Apresenta a página de políticas da plataforma.
        /// </summary>
        /// <returns>Vista das políticas.</returns>
        public IActionResult Policies()
        {
            return View();
        }

        /// <summary>
        /// Apresenta a página explicativa sobre o funcionamento da plataforma.
        /// </summary>
        /// <returns>Vista de como funciona.</returns>
        public IActionResult HowItWorks()
        {
            return View();
        }

        /// <summary>
        /// Apresenta a página de histórias de sucesso de adoções realizadas através da plataforma.
        /// </summary>
        /// <returns>Vista das histórias de sucesso.</returns>
        public IActionResult SuccessStories()
        {
            return View();
        }
    }
}