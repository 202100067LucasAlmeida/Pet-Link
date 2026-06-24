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

            // Precisamos de ir buscar o animal para saber quem é o Tutor (Destinatário da Mensagem) e o Nome
            var animal = await _context.AnimalListings.FindAsync(AnimalListingId);

            if (animal.Status == ListingStatus.Adopted)
            {
                TempData["Error"] = "This animal has already been adopted and is no longer available.";
                return RedirectToAction("Details", "AnimalListings", new { id = AnimalListingId });
            }

            if (animal == null) return NotFound();

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

            // 3. NOVO: Criar a mensagem inicial no sistema de Chat!
            var chatMessage = new PetLink.Models.Message
            {
                SenderId = currentUserId,
                ReceiverId = animal.TutorId,
                AnimalListingId = AnimalListingId, // O contexto do chat (Para separar o Cooper do Rex)
                Content = $"[Adoption Application] Hello! I'm very interested in adopting {animal.Name}.\n\nMy presentation: {Message}",
                Timestamp = DateTime.Now,
                IsRead = false
            };
            _context.Messages.Add(chatMessage);

            // Grava a Candidatura e a Mensagem ao mesmo tempo
            await _context.SaveChangesAsync();

            // 4. Mensagem de sucesso verde no ecrã
            TempData["Success"] = "Your adoption request has been sent! Check your messages to talk with the tutor.";

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Challenge();
            int currentUserId = int.Parse(userIdString);

            // Carregar a candidatura aceite com o anúncio associado
            var acceptedApp = await _context.Applications
                .Include(a => a.AnimalListing)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (acceptedApp == null) return NotFound();

            // Segurança: só o dono do anúncio pode aceitar
            if (acceptedApp.AnimalListing.TutorId != currentUserId)
                return Forbid();

            // Só faz sentido aceitar candidaturas pendentes
            if (acceptedApp.Status != ApplicationStatus.Pending)
            {
                TempData["Error"] = "This application is no longer pending.";
                return RedirectToAction("MyListings", "AnimalListings");
            }

            var listingId = acceptedApp.AnimalListingId;

            // 1. Aceitar esta candidatura
            acceptedApp.Status = ApplicationStatus.Approved;
            acceptedApp.UpdatedAt = DateTime.Now;

            // 2. Marcar o anúncio como adotado
            acceptedApp.AnimalListing.Status = ListingStatus.Adopted;

            // 3. Rejeitar e notificar todos os outros candidatos pendentes
            var otherPendingApps = await _context.Applications
                .Include(a => a.User)
                .Where(a => a.AnimalListingId == listingId
                         && a.Id != id
                         && a.Status == ApplicationStatus.Pending)
                .ToListAsync();

            foreach (var app in otherPendingApps)
            {
                app.Status = ApplicationStatus.Rejected;
                app.UpdatedAt = DateTime.Now;

                // Notificação para cada candidato rejeitado
                _context.ListingsNotifications.Add(new ListingsNotification
                {
                    UserId = app.UserId,
                    Title = "Adoption Update",
                    Message = $"Unfortunately, {acceptedApp.AnimalListing.Name} has already been adopted by another family. Thank you for your interest!",
                    AnimalListingId = listingId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 4. Notificação para o candidato aceite
            _context.ListingsNotifications.Add(new ListingsNotification
            {
                UserId = acceptedApp.UserId,
                Title = "Adoption Accepted! 🎉",
                Message = $"Congratulations! Your adoption request for {acceptedApp.AnimalListing.Name} has been accepted. The tutor will be in touch soon.",
                AnimalListingId = listingId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Adoption accepted! All other pending applications for {acceptedApp.AnimalListing.Name} have been rejected and applicants notified.";
            return RedirectToAction("MyReceivedApplications", "AnimalListings");
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