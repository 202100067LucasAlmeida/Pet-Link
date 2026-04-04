using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Hubs;
using PetLink.Models;
using Microsoft.AspNetCore.Authorization;
using PetLink.Models.Enums;

using PetLink.Controllers;
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

            // Verificar se o utilizador logado é o dono do anúncio
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

            // Passar a variável limpa para a View
            ViewBag.IsOwner = isOwner;

            // Atenção aqui: também mudei para usar o realId
            ViewBag.OtherPets = _context.AnimalListings.Where(a => a.Id != id).Take(4).ToList();

            return View(listing);
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
        public async Task<IActionResult> Create([Bind("Name,Species,Location,AgeMonths,Description,IsVaccinated,IsDewormed,IsSterilized")] AnimalListing animalListing, IFormFile? mainPhoto)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();

            // validação Server-side
            bool hasValidationErrors = false;

            // idade
            if (animalListing.AgeMonths < 0)
            {
                ModelState.AddModelError("AgeMonths", "Age must be 0 or greater");
                hasValidationErrors = true;
            }

            // vacinas
            if (!animalListing.IsVaccinated && !animalListing.IsDewormed && !animalListing.IsSterilized)
            {
                ModelState.AddModelError("IsVaccinated", "At least one of the three (Vaccinated, Dewormed, or Sterilized) must be selected");
                hasValidationErrors = true;
            }

            // foto
            if (mainPhoto == null || mainPhoto.Length == 0)
            {
                ModelState.AddModelError("mainPhoto", "At least one photo, for the main display, is required");
                hasValidationErrors = true;
            }

            if (ModelState.IsValid && !hasValidationErrors)
            {
                // Forçar dados automáticos
                animalListing.TutorId = int.Parse(userIdClaim);
                animalListing.Status = ListingStatus.Pendent;
                animalListing.CreatedAt = DateTime.Now;

                // Upload da Imagem Principal
                if (mainPhoto != null && mainPhoto.Length > 0)
                {
                    animalListing.ImageUrl = await UploadImage(mainPhoto, "animals");
                }

                _context.Add(animalListing);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyListings));
            }
            return View(animalListing);
        }

        // GET: Responsável por abrir a página de edição
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var animalListing = await _context.AnimalListings.FindAsync(id);
            if (animalListing == null) return NotFound();

            // Segurança: Só o dono ou Admin edita
            var userId = int.Parse(User.FindFirst("UserId").Value);
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

            // 1. Procurar o animal, e tutor, original na base de dados
            var existingListing = await _context.AnimalListings
                .Include(a => a.Tutor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingListing == null) return NotFound();

            // 2. Segurança: Só o Dono ou Admin podem editar
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();
            int userId = int.Parse(userIdClaim);

            if (existingListing.TutorId != userId && !User.IsInRole("Admin")) return Forbid();

            // 3. Limpar validações de propriedades de navegação que não vêm do Form
            ModelState.Remove("Tutor");
            ModelState.Remove("Favorites");
            ModelState.Remove("Photos");
            ModelState.Remove("mainPhoto");

            // guardar o status original
            var oldStatus = existingListing.Status.ToString();

            if (ModelState.IsValid)
            {
                try
                {
                    // 4. Mapear manualmente os campos para garantir que o EF rastreia as mudanças
                    existingListing.Name = animalListing.Name;
                    existingListing.Species = animalListing.Species;
                    existingListing.AgeMonths = animalListing.AgeMonths;
                    existingListing.Location = animalListing.Location;
                    existingListing.Description = animalListing.Description;
                    existingListing.IsVaccinated = animalListing.IsVaccinated;
                    existingListing.IsDewormed = animalListing.IsDewormed;
                    existingListing.IsSterilized = animalListing.IsSterilized;

                    // 5. Lógica de ADMIN: Apenas Admin altera Status e Tutor
                    if (User.IsInRole("Admin") && existingListing.Status != animalListing.Status)
                    {
                        existingListing.Status = animalListing.Status;

                        // Create notification for the tutor about status change
                        var newStatus = animalListing.Status.ToString();
                        await _notificationService.CreateListingStatusNotificationAsync(
                            existingListing.TutorId,
                            existingListing.Name,
                            existingListing.Id,
                            oldStatus,
                            newStatus
                        );
                    }

                    if (User.IsInRole("Admin") && existingListing.TutorId != animalListing.TutorId)
                    {
                        existingListing.TutorId = animalListing.TutorId;
                    }

                    // 6. Lógica de Imagem
                    if (mainPhoto != null && mainPhoto.Length > 0)
                    {
                        // Opcional: Apagar a imagem antiga do servidor antes de salvar a nova
                        existingListing.ImageUrl = await UploadImage(mainPhoto, "animals");
                    }

                    // 7. Salvar as alterações
                    _context.Update(existingListing);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Changes saved successfully!";
                    return User.IsInRole("Admin") ? RedirectToAction(nameof(Manage)) : RedirectToAction(nameof(MyListings));
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

            // Se falhar, repopula os dados necessários para a View
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

        private async Task<string> UploadImage(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0) return null;

            // Criar um nome único para evitar ficheiros repetidos
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // Caminho físico: wwwroot/images/subFolder/nome.jpg
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", subFolder, fileName);

            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Caminho para guardar na BD (URL relativa)
            return $"/images/{subFolder}/{fileName}";
        }
    }
}
