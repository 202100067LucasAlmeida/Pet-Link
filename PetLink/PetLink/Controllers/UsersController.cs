using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;

namespace PetLink.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Injetar a base de dados no controller
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ação que carrega a página principal (Index)
        public async Task<IActionResult> Index()
        {
            // Vai à base de dados e traz todos os utilizadores
            var users = await _context.Users.ToListAsync();
            
            // Envia a lista para a página HTML
            return View(users);
        }
    }
}