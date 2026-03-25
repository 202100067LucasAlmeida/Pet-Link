using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using Microsoft.AspNetCore.Authorization;
using PetLink.Models.Enums;

using PetLink.Controllers;
namespace PetLink
{
    public class AnimalListingsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AnimalListingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AnimalListings
        public async Task<IActionResult> Index()
        {
            return await Search(null, null, null, null, null);
        }

        // GET: AnimalListings/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var listing = await _context.AnimalListings
                .Include(a => a.Tutor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (listing == null) return NotFound();

            // 1. Verificar se o utilizador logado é o dono do anúncio
            bool isOwner = false;
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim))
            {
                int currentUserId = int.Parse(userIdClaim);
                isOwner = (listing.TutorId == currentUserId);

                // Só carrega o histórico de mensagens se NÃO for o dono
                if (!isOwner)
                {
                    ViewBag.ChatHistory = await _context.Messages
                        .Where(m => (m.SenderId == currentUserId && m.ReceiverId == listing.TutorId) ||
                                    (m.SenderId == listing.TutorId && m.ReceiverId == currentUserId))
                        .OrderBy(m => m.Timestamp)
                        .ToListAsync();
                }
            }

            // 2. Passar a variável limpa para a View
            ViewBag.IsOwner = isOwner;

            ViewBag.OtherPets = _context.AnimalListings.Where(a => a.Id != id).Take(4).ToList();

            return View(listing);
        }


        // GET: AnimalListings/Create
        public IActionResult Create()
        {
            ViewData["TutorId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: AnimalListings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Species,Location,AgeMonths,Description,IsVaccinated,IsDewormed,IsSterilized,Status,CreatedAt,TutorId")] AnimalListing animalListing)
        {
            if (ModelState.IsValid)
            {
                _context.Add(animalListing);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Manage));
            }
            ViewData["TutorId"] = new SelectList(_context.Users, "Id", "Email", animalListing.TutorId);
            return View(animalListing);
        }

        // GET: AnimalListings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalListing = await _context.AnimalListings.FindAsync(id);
            if (animalListing == null)
            {
                return NotFound();
            }
            ViewData["TutorId"] = new SelectList(_context.Users, "Id", "Email", animalListing.TutorId);
            return View(animalListing);
        }

        // POST: AnimalListings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Species,Location,AgeMonths,Description,IsVaccinated,IsDewormed,IsSterilized,Status,CreatedAt,TutorId")] AnimalListing animalListing)
        {
            if (id != animalListing.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(animalListing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalListingExists(animalListing.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Manage));
            }
            ViewData["TutorId"] = new SelectList(_context.Users, "Id", "Email", animalListing.TutorId);
            return View(animalListing);
        }

        // GET: AnimalListings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalListing = await _context.AnimalListings
                .Include(a => a.Tutor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (animalListing == null)
            {
                return NotFound();
            }

            return View(animalListing);
        }

        public async Task<IActionResult> Search(Species? species, 
                                                string? location, 
                                                Age? age,
                                                string? sort,
                                                string? range)
        {
            var query = _context.AnimalListings
                        .Include(t => t.Tutor)
                        .AsQueryable();

            // Apenas animais publicados, autorizados pelo administrador
            query = query.Where(p => p.Status == ListingStatus.Published);
            
            // Store active filters in ViewBag for the view
            ViewBag.ActiveFilters = new Dictionary<string, object>();

            if (species.HasValue)
            {
                query = query.Where(p => p.Species == species.Value);
                ViewBag.ActiveFilters["Species"] = species.Value;
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(p => p.Location.Contains(location));
                ViewBag.ActiveFilters["Location"] = location;
            }

            if (age.HasValue)
            {
                query = query.Where(p => p.Age == age.Value);
                ViewBag.ActiveFilters["Age"] = age.Value;
            }
            
            // Store range if needed for distance filtering
            if (!string.IsNullOrWhiteSpace(range) && int.TryParse(range, out int rangeValue))
            {
                ViewBag.ActiveFilters["Range"] = rangeValue;
            }

            query = sort switch
            {
                "oldest" => query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var results = await query.ToListAsync();
            
            // Store current filter values to repopulate the form
            ViewBag.CurrentSpecies = species;
            ViewBag.CurrentLocation = location;
            ViewBag.CurrentAge = age;
            ViewBag.CurrentRange = range;

            return View("Index", results);
        }

        // POST: AnimalListings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var animalListing = await _context.AnimalListings.FindAsync(id);
            if (animalListing != null)
            {
                _context.AnimalListings.Remove(animalListing);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }

        private bool AnimalListingExists(int id)
        {
            return _context.AnimalListings.Any(e => e.Id == id);
        }

        // GET: AnimalListings/Manage
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            // Vai buscar todos os anúncios e inclui a informação do Tutor (para sabermos o email dele)
            var allListings = await _context.AnimalListings
                .Include(a => a.Tutor)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(allListings);
        }
    }
}
