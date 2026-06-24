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
                .Include(a => a.HealthDocuments)
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
            ViewBag.OtherPets = await _context.AnimalListings
                .Include(a => a.HealthDocuments)
                .Where(a => a.Id != id && a.Status == ListingStatus.Published)
                .Take(4)
                .ToListAsync();

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
        public async Task<IActionResult> Create(
            [Bind("Name,Species,Location,AgeMonths,Description")] AnimalListing animalListing,
            IFormFile? mainPhoto,
            IFormFile[]? galleryPhotos,
            bool IsVaccinated,
            bool IsDewormed,
            bool IsSterilized,
            IFormFile[]? vaccinationDocuments,
            IFormFile[]? dewormingDocuments,
            IFormFile[]? sterilizationDocuments)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();

            // Store checkbox values in ViewBag for preserving state
            ViewBag.IsVaccinated = IsVaccinated;
            ViewBag.IsDewormed = IsDewormed;
            ViewBag.IsSterilized = IsSterilized;

            ModelState.Remove("Tutor");
            ModelState.Remove("Favorites");
            ModelState.Remove("Photos");
            ModelState.Remove("HealthDocuments");

            // ========== VALIDAÇÕES ==========
            if (animalListing.AgeMonths < 0)
                ModelState.AddModelError("AgeMonths", "Age must be 0 or greater.");

            if (!IsVaccinated && !IsDewormed && !IsSterilized)
            {
                ModelState.AddModelError("IsVaccinated", "Please confirm at least one health status (Vaccinated, Dewormed, or Sterilized).");
            }

            // Only validate main photo if this is the initial submission
            // Don't validate if we're returning from a validation error
            if (mainPhoto == null || mainPhoto.Length == 0)
            {
                ModelState.AddModelError("mainPhoto", "A main photo is required.");
            }

            if (ModelState.IsValid)
            {
                // Set basic properties
                animalListing.TutorId = int.Parse(userIdClaim);
                animalListing.Status = ListingStatus.Pending;
                animalListing.CreatedAt = DateTime.Now;

                // Upload the main photo
                if (mainPhoto != null && mainPhoto.Length > 0)
                {
                    animalListing.ImageUrl = await UploadImage(mainPhoto, "animals");
                }

                // Save the animal listing to get an ID
                _context.Add(animalListing);
                await _context.SaveChangesAsync();

                // Add Vaccination Documents
                if (IsVaccinated && vaccinationDocuments != null && vaccinationDocuments.Any())
                {
                    foreach (var doc in vaccinationDocuments.Where(d => d != null && d.Length > 0))
                    {
                        var filePath = await UploadImage(doc, "health-documents/vaccinations");
                        var healthDoc = new HealthDocument
                        {
                            Name = $"Vaccination Document - {DateTime.Now:yyyy-MM-dd HH:mm}",
                            Type = HealthDocumentType.Vaccine,
                            FilePath = filePath,
                            IsVerified = false,
                            UploadedAt = DateTime.UtcNow,
                            AnimalListingId = animalListing.Id
                        };
                        _context.HealthDocuments.Add(healthDoc);
                    }
                }

                // Add Deworming Documents
                if (IsDewormed && dewormingDocuments != null && dewormingDocuments.Any())
                {
                    foreach (var doc in dewormingDocuments.Where(d => d != null && d.Length > 0))
                    {
                        var filePath = await UploadImage(doc, "health-documents/deworming");
                        var healthDoc = new HealthDocument
                        {
                            Name = $"Deworming Document - {DateTime.Now:yyyy-MM-dd HH:mm}",
                            Type = HealthDocumentType.Deworming,
                            FilePath = filePath,
                            IsVerified = false,
                            UploadedAt = DateTime.UtcNow,
                            AnimalListingId = animalListing.Id
                        };
                        _context.HealthDocuments.Add(healthDoc);
                    }
                }

                // Add Sterilization Documents
                if (IsSterilized && sterilizationDocuments != null && sterilizationDocuments.Any())
                {
                    foreach (var doc in sterilizationDocuments.Where(d => d != null && d.Length > 0))
                    {
                        var filePath = await UploadImage(doc, "health-documents/sterilization");
                        var healthDoc = new HealthDocument
                        {
                            Name = $"Sterilization Document - {DateTime.Now:yyyy-MM-dd HH:mm}",
                            Type = HealthDocumentType.Sterilization,
                            FilePath = filePath,
                            IsVerified = false,
                            UploadedAt = DateTime.UtcNow,
                            AnimalListingId = animalListing.Id
                        };
                        _context.HealthDocuments.Add(healthDoc);
                    }
                }

                await _context.SaveChangesAsync();

                await _notificationService.CreateNewListingNotificationForAdminsAsync(
                    animalListing.Id,
                    animalListing.Name,
                    animalListing.TutorId
                );

                TempData["Success"] = "Your listing has been created successfully!";
                return RedirectToAction(nameof(MyListings));
            }

            // If validation fails, return to the form
            return View(animalListing);
        }

        // GET: AnimalListings/Edit/5
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var animalListing = await _context.AnimalListings
                .Include(a => a.HealthDocuments)
                .FirstOrDefaultAsync(a => a.Id == id);

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
        // 1. ADICIONADOS OS 3 BOOLEANOS AQUI PARA CAPTURAR AS CHECKBOXES DO FORMULÁRIO HTML
        public async Task<IActionResult> Edit(int id, AnimalListing animalListing, IFormFile? mainPhoto, bool isVaccinated, bool isDewormed, bool isSterilized, int[] verifiedDocuments)
        {
            if (id != animalListing.Id) return NotFound();

            var existingListing = await _context.AnimalListings
                .Include(a => a.Tutor)
                .Include(a => a.HealthDocuments) // 2. CRÍTICO: Carregar os documentos de saúde existentes!
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
            ModelState.Remove("HealthDocuments"); // Prevenir erros de validação

            var oldStatus = existingListing.Status.ToString();

            // ========== VALIDAÇÃO DAS CHECKBOXES DE SAÚDE ==========
            // Usamos os parâmetros booleanos do método em vez do animalListing
            if (!isVaccinated && !isDewormed && !isSterilized)
            {
                ModelState.AddModelError("IsVaccinated", "Please confirm at least one health status (Vaccinated, Dewormed, or Sterilized).");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingListing.Name = animalListing.Name;
                    existingListing.Species = animalListing.Species;
                    existingListing.AgeMonths = animalListing.AgeMonths;
                    existingListing.Location = animalListing.Location;
                    existingListing.Description = animalListing.Description;

                    // ========== ATUALIZAR AS CONDIÇÕES DE SAÚDE ==========

                    // Vacinação: Se a checkbox está ativa e não existe documento, criamos. Se está desativada e existe, apagamos.
                    var vacDoc = existingListing.HealthDocuments.FirstOrDefault(d => d.Type == HealthDocumentType.Vaccine);
                    if (isVaccinated && vacDoc == null)
                        existingListing.HealthDocuments.Add(new HealthDocument { Name = "Boletim de Vacinas", Type = HealthDocumentType.Vaccine, FilePath = "/images/placeholders/proof_vacination.png" });
                    else if (!isVaccinated && vacDoc != null)
                        existingListing.HealthDocuments.Remove(vacDoc);

                    // Desparasitação
                    var dewDoc = existingListing.HealthDocuments.FirstOrDefault(d => d.Type == HealthDocumentType.Deworming);
                    if (isDewormed && dewDoc == null)
                        existingListing.HealthDocuments.Add(new HealthDocument { Name = "Comprovativo Desparasitação", Type = HealthDocumentType.Deworming, FilePath = "/images/placeholders/proof_vacination.png" });
                    else if (!isDewormed && dewDoc != null)
                        existingListing.HealthDocuments.Remove(dewDoc);

                    // Esterilização
                    var steDoc = existingListing.HealthDocuments.FirstOrDefault(d => d.Type == HealthDocumentType.Sterilization);
                    if (isSterilized && steDoc == null)
                        existingListing.HealthDocuments.Add(new HealthDocument { Name = "Certificado Esterilização", Type = HealthDocumentType.Sterilization, FilePath = "/images/placeholders/proof_vacination.png" });
                    else if (!isSterilized && steDoc != null)
                        existingListing.HealthDocuments.Remove(steDoc);

                    if (isAdmin && verifiedDocuments != null && verifiedDocuments.Any())
                    {
                        var currentAdminId = int.Parse(userIdClaim);
                        foreach (var docId in verifiedDocuments)
                        {
                            var doc = await _context.HealthDocuments.FindAsync(docId);
                            if (doc != null && doc.AnimalListingId == existingListing.Id)
                            {
                                doc.IsVerified = true;
                                doc.VerifiedAt = DateTime.Now;
                                doc.VerifiedByAdminId = currentAdminId;
                            }
                        }
                    }

                    // Optionally, unverify documents that were not checked
                    if (isAdmin && ModelState.IsValid)
                    {
                        var allDocs = existingListing.HealthDocuments;
                        foreach (var doc in allDocs)
                        {
                            if (verifiedDocuments == null || !verifiedDocuments.Contains(doc.Id))
                            {
                                doc.IsVerified = false;
                                doc.VerifiedAt = null;
                                doc.VerifiedByAdminId = null;
                            }
                        }
                    }


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
                .Include(a => a.HealthDocuments)
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
                .Include(a => a.HealthDocuments)
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
                .Include(a => a.HealthDocuments)
                .Where(a => a.TutorId == currentUserId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(myListings);
        }

        // GET: AnimalListings/MyReceivedApplications
        [Authorize]
        public async Task<IActionResult> MyReceivedApplications()
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Challenge();
            int currentUserId = int.Parse(userIdString);

            // Buscar todos os pedidos feitos a anúncios que pertencem ao utilizador atual
            var applications = await _context.Applications
                .Include(a => a.User)
                .Include(a => a.AnimalListing)
                .Where(a => a.AnimalListing.TutorId == currentUserId)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            return View(applications);
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

            Directory.CreateDirectory(Path.GetDirectoryName(uploadPath));

            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{subFolder}/{fileName}";
        }

        // GET: AnimalListings/Map
        public async Task<IActionResult> Map(Species? species, string? location, Age? age)
        {
            var query = _context.AnimalListings
                .Where(p => p.Status == ListingStatus.Published);

            if (species.HasValue)
                query = query.Where(p => p.Species == species.Value);

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(p => p.Location.Contains(location));

            if (age.HasValue)
                query = query.Where(p => p.Age == age.Value);

            var results = await query.ToListAsync();

            // Buscar shelters e petsitters com coordenadas ou cidade
            var serviceUsers = await _context.Users
                .Where(u => (u.Role == UserRole.Shelter || u.Role == UserRole.PetSitter)
                         && !string.IsNullOrEmpty(u.City))
                .ToListAsync();

            ViewBag.Shelters = serviceUsers.Where(u => u.Role == UserRole.Shelter).ToList();
            ViewBag.PetSitters = serviceUsers.Where(u => u.Role == UserRole.PetSitter).ToList();
            ViewBag.CurrentSpecies = species;
            ViewBag.CurrentLocation = location;
            ViewBag.CurrentAge = age;

            return View(results);
        }
    }
}