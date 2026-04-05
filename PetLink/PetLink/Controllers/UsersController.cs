using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using Microsoft.AspNetCore.Authorization;
using PetLink.Models;
namespace PetLink.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users
        // Mostra a lista de todos os utilizadores registados
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        // Mostra os detalhes de um utilizador específico
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null) return NotFound();

            // Carregar histórico de mensagens
            if (GetCurrentUserId(out int currentUserId))
            {
                ViewBag.ChatHistory = await _context.Messages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == user.Id) ||
                                (m.SenderId == user.Id && m.ReceiverId == currentUserId))
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }

            return View(user);
        }

        // GET: Users/Edit/5
        // Mostra o formulário de edição de um utilizador
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Users/Edit/5
        // Recebe os dados editados do utilizador e atualiza na base de dados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,PasswordHash,Role,IsVerified")] User user)
        {
            if (id != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        // Mostra a página de confirmação para eliminar um utilizador
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Users/Delete/5
        // Remove o utilizador da base de dados após confirmação
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Verifica se um utilizador existe na base de dados
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }


        // GET: Users/Create
        // Mostra o formulário para criar um novo utilizador
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // Recebe os dados do novo utilizador e guarda na base de dados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,PasswordHash,Role,IsVerified")] User user)
        {
            // Verifica se o email já existe
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Este email já está registado na base de dados.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }
    }
}