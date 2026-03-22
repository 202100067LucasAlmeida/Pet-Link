using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using Microsoft.AspNetCore.Authorization;

namespace PetLink
{
    public class AnimalListingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnimalListingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AnimalListings
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AnimalListings.Include(a => a.Tutor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AnimalListings/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var listing = await _context.AnimalListings
                .Include(a => a.Tutor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (listing == null) return NotFound();

            // Carregar histórico de mensagens entre o User atual e o Tutor deste animal
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                int currentUserId = int.Parse(userIdClaim);

                ViewBag.ChatHistory = await _context.Messages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == listing.TutorId) ||
                                (m.SenderId == listing.TutorId && m.ReceiverId == currentUserId))
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }

            // carregar OtherPets
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
