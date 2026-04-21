using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;
using PetLink.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PetLink.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService; // Adicionado para não dar erro no Complete

        public ApplicationsController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // POST: /Applications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int AnimalListingId, string Message)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Challenge();

            int currentUserId = int.Parse(userIdString);

            // 1. Evitar que a pessoa se candidate duas vezes ao mesmo animal
            var existingApp = await _context.Applications
                .FirstOrDefaultAsync(a => a.UserId == currentUserId && a.AnimalListingId == AnimalListingId);

            if (existingApp != null)
            {
                TempData["Error"] = "You have already applied for this pet.";
                return RedirectToAction("Details", "AnimalListings", new { id = AnimalListingId });
            }

            // 2. Criar a nova candidatura
            var application = new Application
            {
                UserId = currentUserId,
                AnimalListingId = AnimalListingId,
                Message = Message,
                Status = ApplicationStatus.Pending,
                SubmittedAt = DateTime.Now
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            // 3. Mensagem de sucesso verde no ecrã
            TempData["Success"] = "Your adoption request has been sent to the tutor!";

            // 4. Redireciona de volta para a página do animal
            return RedirectToAction("Details", "AnimalListings", new { id = AnimalListingId });
        }

        // GET: Applications/Manage
        [Authorize(Roles = "Admin,Shelter")]
        public async Task<IActionResult> Manage()
        {
            var applications = await _context.Applications
                .Include(a => a.User)
                .Include(a => a.AnimalListing)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            return View(applications);
        }

        // POST: Applications/Approve/5
        [HttpPost]
        [Authorize(Roles = "Admin,Shelter")]
        public async Task<IActionResult> Approve(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null) return NotFound();

            application.Status = ApplicationStatus.Approved;
            application.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Application approved!";
            return RedirectToAction(nameof(Manage));
        }

        // POST: Applications/Complete/5
        [HttpPost]
        [Authorize(Roles = "Admin,Shelter")]
        public async Task<IActionResult> Complete(int id)
        {
            var application = await _context.Applications
                .Include(a => a.User)
                .Include(a => a.AnimalListing)
                .ThenInclude(a => a.Tutor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();

            application.Status = ApplicationStatus.Completed;
            application.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Enviar email de confirmação de adoção
            await _emailService.SendAdoptionConfirmationAsync(
                application.User.Email,
                application.User.Name,
                application.AnimalListing.Name,
                application.AnimalListing.Tutor?.Name ?? "Pet Shelter"
            );

            TempData["Success"] = "Adoption completed! Email sent to the adopter.";
            return RedirectToAction(nameof(Manage));
        }

        // POST: Applications/Reject/5
        [HttpPost]
        [Authorize(Roles = "Admin,Shelter")]
        public async Task<IActionResult> Reject(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null) return NotFound();

            application.Status = ApplicationStatus.Rejected;
            application.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Application rejected.";
            return RedirectToAction(nameof(Manage));
        }
    }
}