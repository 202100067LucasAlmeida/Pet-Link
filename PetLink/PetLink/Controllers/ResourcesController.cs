using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;

namespace PetLink.Controllers
{
    public class ResourcesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResourcesController(ApplicationDbContext context)
        {
            _context = context;
        }

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

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) return NotFound();
            return View(resource);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var resources = await _context.Resources
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(resources);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) return NotFound();
            return View(resource);
        }

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
