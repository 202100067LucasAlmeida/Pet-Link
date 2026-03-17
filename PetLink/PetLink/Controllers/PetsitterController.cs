using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;

namespace PetLink.Controllers
{
    public class PetsitterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PetsitterController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // O Include(p => p.User) é vital para conseguirmos aceder ao Nome e IsVerified do User
            var sitters = await _context.Petsitters
                                        .Include(p => p.User)
                                        .ToListAsync();
            return View(sitters);
        }

        // GET: Petsitter/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // 1. Vai buscar o Sitter Principal
            var petsitter = await _context.Petsitters
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (petsitter == null) return NotFound();

            // 2. Vai buscar outros 4 Sitters (excluindo o atual)
            var otherSitters = await _context.Petsitters
                .Include(p => p.User)
                .Where(p => p.Id != id) // Não mostrar o próprio Sitter na secção "Outros"
                .Take(4) // Limitar a 4 cartões para não desformatar a página
                .ToListAsync();

            // 3. Coloca os outros Sitters na "Mochila" (ViewBag)
            ViewBag.OtherSitters = otherSitters;

            return View(petsitter);
        }
    }
}