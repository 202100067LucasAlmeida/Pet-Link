using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;

namespace PetLink.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de recursos informativos da plataforma.
    /// Permite consultar, pesquisar, criar, editar e eliminar recursos,
    /// estando estas últimas operações reservadas a administradores.
    /// </summary>
    public class ResourcesController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador de recursos.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        public ResourcesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Apresenta a listagem pública de recursos disponíveis,
        /// permitindo filtrar por texto de pesquisa, espécie e categoria.
        /// </summary>
        /// <param name="search">Texto a pesquisar no título ou conteúdo do recurso (opcional).</param>
        /// <param name="species">Espécie para filtrar os recursos (opcional).</param>
        /// <param name="category">Categoria para filtrar os recursos (opcional).</param>
        /// <returns>Vista com os recursos que correspondem aos filtros aplicados.</returns>
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? search, Species? species, ResourceCategory? category)
        {
            var query = _context.Resources.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r => r.Title.Contains(search) || r.Content.Contains(search));
                ViewBag.CurrentSearch = search;
            }

            if (species.HasValue)
            {
                query = query.Where(r => r.Species == species.Value);
                ViewBag.CurrentSpecies = species.Value;
            }

            if (category.HasValue)
            {
                query = query.Where(r => r.Category == category.Value);
                ViewBag.CurrentCategory = category.Value;
            }

            var resources = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.SpeciesList = Enum.GetValues(typeof(Species));
            ViewBag.CategoryList = Enum.GetValues(typeof(ResourceCategory));

            return View(resources);
        }

        /// <summary>
        /// Apresenta os detalhes de um recurso específico.
        /// </summary>
        /// <param name="id">Identificador do recurso.</param>
        /// <returns>Vista com os detalhes do recurso.</returns>
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) return NotFound();
            return View(resource);
        }

        /// <summary>
        /// Apresenta a área de gestão de recursos para administradores.
        /// </summary>
        /// <returns>Vista com a lista de todos os recursos.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var resources = await _context.Resources
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(resources);
        }

        /// <summary>
        /// Apresenta o formulário de criação de um novo recurso.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <returns>Vista de criação de recurso.</returns>
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Cria um novo recurso informativo.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <param name="model">Dados do recurso a criar.</param>
        /// <returns>Redireciona para a gestão de recursos caso a criação seja bem-sucedida.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Resource model)
        {
            ModelState.Remove("CreatedAt");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CreatedAt = DateTime.Now;
            _context.Resources.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Resource created successfully!";
            return RedirectToAction(nameof(Manage));
        }

        /// <summary>
        /// Apresenta o formulário de edição de um recurso existente.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <param name="id">Identificador do recurso.</param>
        /// <returns>Vista de edição do recurso.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) return NotFound();
            return View(resource);
        }

        /// <summary>
        /// Atualiza as informações de um recurso existente.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <param name="id">Identificador do recurso.</param>
        /// <param name="model">Dados atualizados do recurso.</param>
        /// <returns>Redireciona para a gestão de recursos após guardar.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Resource model)
        {
            if (id != model.Id) return NotFound();

            var existing = await _context.Resources.FindAsync(id);
            if (existing == null) return NotFound();

            ModelState.Remove("CreatedAt");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            existing.Title = model.Title;
            existing.Content = model.Content;
            existing.MediaUrl = model.MediaUrl;
            existing.Type = model.Type;
            existing.Species = model.Species;
            existing.Category = model.Category;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Resource updated successfully!";
            return RedirectToAction(nameof(Manage));
        }

        /// <summary>
        /// Remove definitivamente um recurso.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <param name="id">Identificador do recurso.</param>
        /// <returns>Redireciona para a gestão de recursos.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) return NotFound();

            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Resource deleted successfully!";
            return RedirectToAction(nameof(Manage));
        }
    }
}
