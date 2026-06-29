using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using Microsoft.AspNetCore.Authorization;
using PetLink.Models;

namespace PetLink.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão administrativa de utilizadores.
    /// Permite listar, consultar, criar, editar e eliminar utilizadores,
    /// estando a maioria das operações reservadas a administradores.
    /// </summary>
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador de utilizadores.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Apresenta a lista de todos os utilizadores registados.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <returns>Vista com a lista de utilizadores.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        /// <summary>
        /// Apresenta os detalhes de um utilizador específico.
        /// Caso o utilizador autenticado esteja identificado, carrega também
        /// o histórico de mensagens entre ambos.
        /// </summary>
        /// <param name="id">Identificador do utilizador.</param>
        /// <returns>Vista com os detalhes do utilizador.</returns>
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

        /// <summary>
        /// Apresenta o formulário de edição de um utilizador existente.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <param name="id">Identificador do utilizador.</param>
        /// <returns>Vista de edição do utilizador.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        /// <summary>
        /// Atualiza os dados permitidos de um utilizador (nome, email, papel e estado de verificação),
        /// preservando os restantes campos. Apenas acessível a administradores.
        /// </summary>
        /// <param name="id">Identificador do utilizador.</param>
        /// <param name="user">Dados atualizados do utilizador.</param>
        /// <returns>Redireciona para a lista de utilizadores após guardar.</returns>
        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Role,IsVerified")] User user)
        {
            if (id != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var rows = await _context.Users
                    .Where(u => u.Id == id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(u => u.Name, user.Name)
                        .SetProperty(u => u.Email, user.Email)
                        .SetProperty(u => u.Role, user.Role)
                        .SetProperty(u => u.IsVerified, user.IsVerified)
                    );

                Console.WriteLine($"Linhas afetadas: {rows}");
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        /// <summary>
        /// Apresenta a página de confirmação antes de eliminar um utilizador.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <param name="id">Identificador do utilizador.</param>
        /// <returns>Vista de confirmação.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null) return NotFound();

            return View(user);
        }

        /// <summary>
        /// Remove definitivamente um utilizador da base de dados.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <param name="id">Identificador do utilizador.</param>
        /// <returns>Redireciona para a lista de utilizadores.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

        /// <summary>
        /// Apresenta o formulário de criação de um novo utilizador.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <returns>Vista de criação de utilizador.</returns>
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Cria um novo utilizador, verificando previamente se o email
        /// já se encontra registado na base de dados. Apenas acessível a administradores.
        /// </summary>
        /// <param name="user">Dados do utilizador a criar.</param>
        /// <returns>Redireciona para a lista de utilizadores caso a criação seja bem-sucedida.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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