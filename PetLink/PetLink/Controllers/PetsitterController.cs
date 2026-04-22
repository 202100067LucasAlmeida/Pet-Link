using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;

namespace PetLink.Controllers
{
    public class PetsitterController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public PetsitterController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return await Search(null, null, null);
        }

        // GET: Petsitter/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var sitter = await _context.Petsitters
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sitter == null) return NotFound();

            // 1. Verificar se o utilizador logado está a ver o seu próprio perfil
            bool isSelf = false;
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim))
            {
                int currentUserId = int.Parse(userIdClaim);
                isSelf = (sitter.UserId == currentUserId);

                // 2. Só carrega o histórico de mensagens se NÃO for o próprio
                if (!isSelf)
                {
                    ViewBag.ChatHistory = await _context.Messages
                        .Where(m => (m.SenderId == currentUserId && m.ReceiverId == sitter.UserId) ||
                                    (m.SenderId == sitter.UserId && m.ReceiverId == currentUserId))
                        .OrderBy(m => m.Timestamp)
                        .ToListAsync();
                }
            }

            ViewBag.IsSelf = isSelf;

            // Carregar outros sitters para a secção de baixo (excluindo o atual)
            ViewBag.OtherSitters = await _context.Petsitters
                .Include(p => p.User)
                .Where(p => p.Id != id)
                .Take(4)
                .ToListAsync();

            return View(sitter);
        }

        public async Task<IActionResult> Search(ServiceType? serviceType,
                                                decimal? maxRate,
                                                PetPreferences? petPreferences)
        {

            var query = _context.Petsitters
                        .Include(p => p.User)
                        .AsQueryable();

            if (serviceType.HasValue)
            {
                query = query.Where(p => p.serviceType == serviceType.Value);
            }

            if (maxRate.HasValue)
            {
                query = query.Where(p => p.HourlyRate <= maxRate.Value);
            }

            if (petPreferences.HasValue)
            {
                query = query.Where(p => p.petPreferences == petPreferences.Value);
            }

            var results = await query.ToListAsync();

            return View("Index", results);
        }
    }
}