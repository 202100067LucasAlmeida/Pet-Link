using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;
using PetLink.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PetLink.Controllers
{
    [Authorize(Roles = "Admin,Shelter")]
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public ApplicationsController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Applications/Manage
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