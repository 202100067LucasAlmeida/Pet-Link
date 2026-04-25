using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetLink.Controllers;
using PetLink.Data;
using PetLink.Hubs;
using PetLink.Models;
using PetLink.Models.Enums;

namespace PetLink
{
    public class AnimalListingsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public AnimalListingsController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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

            var userIdClaim = User.FindFirst("UserId")?.Value;
            bool isAuthenticated = int.TryParse(userIdClaim, out int currentUserId);

            // Segurança: Ocultar anúncios não publicados, exceto para o dono ou Admin
            if (listing.Status != ListingStatus.Published)
            {
                bool isAuthorized = isAuthenticated && (listing.TutorId == currentUserId || User.IsInRole("Admin"));
                if (!isAuthorized) return Forbid();
            }

            bool isOwner = isAuthenticated && (listing.TutorId == currentUserId);
            bool hasApplied = false;
            ApplicationStatus? myApplicationStatus = null;
            bool canReview = false;

            if (isAuthenticated && !isOwner)
            {
                // Carregar o histórico de mensagens
                ViewBag.ChatHistory = await _context.Messages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == listing.TutorId) ||
                                (m.SenderId == listing.TutorId && m.ReceiverId == currentUserId))
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();

                // Verificar se já existe candidatura
                var application = await _context.Applications
                    .FirstOrDefaultAsync(a => a.UserId == currentUserId && a.AnimalListingId == id);

                if (application != null)
                {
                    hasApplied = true;
                    myApplicationStatus = application.Status;

                    // Lógica para verificar se pode avaliar (apenas se a adoção foi concluída)
                    if (application.Status == ApplicationStatus.Completed)
                    {
                        var existingReview = await _context.Reviews
                            .FirstOrDefaultAsync(r => r.ReviewerId == currentUserId && r.AnimalListingId == id);

                        bool canReceiveReview = listing.Tutor != null &&
                                                (listing.Tutor.Role == UserRole.User || listing.Tutor.Role == UserRole.PetSitter);

                        canReview = existingReview == null && canReceiveReview;
                    }
                }
            }

            ViewBag.IsOwner = isOwner;
            ViewBag.HasApplied = hasApplied;
            ViewBag.MyApplicationStatus = myApplicationStatus;
            ViewBag.CanReview = canReview;
            ViewBag.OtherPets = await _context.AnimalListings.Where(a => a.Id != id).Take(4).ToListAsync();

            return View(listing);
        }

        // GET: Lista as candidaturas enviadas pelo Adotante
        [Authorize]
        public async Task<IActionResult> MyApplications()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();

            int currentUserId = int.Parse(userIdClaim);

            var myApplications = await _context.Applications
                .Include(a => a.AnimalListing)
                    .ThenInclude(l => l.Tutor)
                .Where(a => a.UserId == currentUserId)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            return View(myApplications);
        }

        // GET: AnimalListings/Create
        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: AnimalListings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Name,Species,Location,AgeMonths,Description,IsVaccinated,IsDewormed,IsSterilized")] AnimalListing animalListing,
            IFormFile? mainPhoto, IFormFile? vaccinationProof, IFormFile? dewormingProof, IFormFile? sterilizationProof)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();

            // Validação customizada
            if (animalListing.AgeMonths < 0)
                ModelState.AddModelError("AgeMonths", "Age must be 0 or greater.");

            if (!animalListing.IsVaccinated && !animalListing.IsDewormed && !animalListing.IsSterilized)
                ModelState.AddModelError("IsVaccinated", "At least one health status (Vaccinated, Dewormed, or Sterilized) must be selected.");

            if (mainPhoto == null || mainPhoto.Length == 0)
                ModelState.AddModelError("mainPhoto", "A main photo is required.");

            // Validação dos documentos
            if (animalListing.IsVaccinated && (vaccinationProof == null || vaccinationProof.Length == 0))
                ModelState.AddModelError("vaccinationProof", "You must provide proof of vaccination.");

            if (animalListing.IsDewormed && (dewormingProof == null || dewormingProof.Length == 0))
                ModelState.AddModelError("dewormingProof", "You must provide proof of deworming.");

            if (animalListing.IsSterilized && (sterilizationProof == null || sterilizationProof.Length == 0))
                ModelState.AddModelError("sterilizationProof", "You must provide proof of sterilization.");

            if (ModelState.IsValid)
            {
                animalListing.TutorId = int.Parse(userIdClaim);
                animalListing.Status = ListingStatus.Pending;
                animalListing.CreatedAt = DateTime.Now;
                animalListing.ImageUrl = await UploadImage(mainPhoto, "animals");

                // Upload dos documentos
                if (animalListing.IsVaccinated && vaccinationProof != null)
                    animalListing.VaccinationProofUrl = await UploadImage(vaccinationProof, "proofs");

                if (animalListing.IsDewormed && dewormingProof != null)
                    animalListing.DewormingProofUrl = await UploadImage(dewormingProof, "proofs");

                if (animalListing.IsSterilized && sterilizationProof != null)
                    animalListing.SterilizationProofUrl = await UploadImage(sterilizationProof, "proofs");

                _context.Add(animalListing);
                await _context.SaveChangesAsync();

                await _notificationService.CreateNewListingNotificationForAdminsAsync(
                    animalListing.Id,
                    animalListing.Name,
                    animalListing.TutorId
                );

                return RedirectToAction(nameof(MyListings));
            }

            return View(animalListing);
        }

        // GET: AnimalListings/Edit/5
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var animalListing = await _context.AnimalListings.FindAsync(id);
            if (animalListing == null) return NotFound();

            int userId = int.Parse(User.FindFirst("UserId").Value);
            if (animalListing.TutorId != userId && !User.IsInRole("Admin")) return Forbid();

            ViewData["TutorId"] = new SelectList(_context.Users, "Id", "Email", animalListing.TutorId);
            return View(animalListing);
        }

        // POST: AnimalListings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, AnimalListing animalListing, IFormFile? mainPhoto)
        {
            if (id != animalListing.Id) return NotFound();

            var existingListing = await _context.AnimalListings
                .Include(a => a.Tutor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingListing == null) return NotFound();

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();

            int userId = int.Parse(userIdClaim);
            bool isAdmin = User.IsInRole("Admin");

            if (existingListing.TutorId != userId && !isAdmin) return Forbid();

            ModelState.Remove("Tutor");
            ModelState.Remove("Favorites");
            ModelState.Remove("Photos");
            ModelState.Remove("mainPhoto");

            var oldStatus = existingListing.Status.ToString();

            if (ModelState.IsValid)
            {
                try
                {
                    existingListing.Name = animalListing.Name;
                    existingListing.Species = animalListing.Species;
                    existingListing.AgeMonths = animalListing.AgeMonths;
                    existingListing.Location = animalListing.Location;
                    existingListing.Description = animalListing.Description;
                    existingListing.IsVaccinated = animalListing.IsVaccinated;
                    existingListing.IsDewormed = animalListing.IsDewormed;
                    existingListing.IsSterilized = animalListing.IsSterilized;

                    if (isAdmin)
                    {
                        if (existingListing.Status != animalListing.Status)
                        {
                            existingListing.Status = animalListing.Status;
                            await _notificationService.CreateListingStatusNotificationAsync(
                                existingListing.TutorId,
                                existingListing.Name,
                                existingListing.Id,
                                oldStatus,
                                animalListing.Status.ToString()
                            );
                        }

                        if (existingListing.TutorId != animalListing.TutorId)
                        {
                            existingListing.TutorId = animalListing.TutorId;
                        }
                    }

                    if (mainPhoto != null && mainPhoto.Length > 0)
                    {
                        existingListing.ImageUrl = await UploadImage(mainPhoto, "animals");
                    }

                    _context.Update(existingListing);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Changes saved successfully!";
                    return isAdmin ? RedirectToAction(nameof(Manage)) : RedirectToAction(nameof(MyListings));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalListingExists(animalListing.Id)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving: " + ex.Message);
                }
            }

            ViewData["TutorId"] = new SelectList(_context.Users, "Id", "Email", animalListing.TutorId);
            return View(animalListing);
        }

        // GET: AnimalListings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var animalListing = await _context.AnimalListings
                .Include(a => a.Tutor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (animalListing == null) return NotFound();

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
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Manage));
        }

        public async Task<IActionResult> Search(Species? species, string? location, Age? age, string? sort, string? range)
        {
            var query = _context.AnimalListings
                .Include(t => t.Tutor)
                .Where(p => p.Status == ListingStatus.Published);

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

            ViewBag.CurrentSpecies = species;
            ViewBag.CurrentLocation = location;
            ViewBag.CurrentAge = age;
            ViewBag.CurrentRange = range;

            return View("Index", results);
        }

        // GET: AnimalListings/Manage (Admin apenas)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var allListings = await _context.AnimalListings
                .Include(a => a.Tutor)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(allListings);
        }

        // GET: Meus anúncios
        [Authorize]
        public async Task<IActionResult> MyListings()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("LoginForm", "Profile");

            int currentUserId = int.Parse(userIdClaim);

            var myListings = await _context.AnimalListings
                .Where(a => a.TutorId == currentUserId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(myListings);
        }

        private bool AnimalListingExists(int id)
        {
            return _context.AnimalListings.Any(e => e.Id == id);
        }

        private async Task<string> UploadImage(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0) return null;

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", subFolder, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(uploadPath)); // Garantir que a pasta existe

            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{subFolder}/{fileName}";
        }
        // GET: AnimalListings/Map
        public async Task<IActionResult> Map(Species? species, string? location, Age? age)
        {
            // Vai buscar apenas os animais publicados e os seus tutores (para ter as coordenadas)
            var query = _context.AnimalListings
                .Include(t => t.Tutor)
                .Where(p => p.Status == ListingStatus.Published);

            // Aplicar Filtros se existirem
            if (species.HasValue)
                query = query.Where(p => p.Species == species.Value);

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(p => p.Location.Contains(location));

            if (age.HasValue)
                query = query.Where(p => p.Age == age.Value);

            var results = await query.ToListAsync();

            // Guardar os filtros atuais para manter a seleção visível na página
            ViewBag.CurrentSpecies = species;
            ViewBag.CurrentLocation = location;
            ViewBag.CurrentAge = age;

            return View(results);
        }
    }
}