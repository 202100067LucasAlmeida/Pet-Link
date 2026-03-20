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
        public async Task<IActionResult> Details(int id)
        {
            var sitter = await _context.Petsitters
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sitter == null) return NotFound();

            // Carregar histórico de mensagens
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                int currentUserId = int.Parse(userIdClaim);

                ViewBag.ChatHistory = await _context.Messages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == sitter.UserId) ||
                                (m.SenderId == sitter.UserId && m.ReceiverId == currentUserId))
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }

            // Carregar outros sitters para a ViewBag (como já deves ter)
            ViewBag.OtherSitters = await _context.Petsitters.Include(p => p.User).Where(p => p.Id != id).Take(4).ToListAsync();

            return View(sitter);
        }
    }
}