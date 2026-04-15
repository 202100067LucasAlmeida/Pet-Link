using PetLink.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using Microsoft.AspNetCore.Authorization;

public class ApplicationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public ApplicationController(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // Método para marcar adoção como concluída
    [HttpPost]
    [Authorize(Roles = "Admin,Shelter")]
    public async Task<IActionResult> CompleteAdoption(int applicationId)
    {
        var application = await _context.Applications
            .Include(a => a.User)
            .Include(a => a.AnimalListing)
            .ThenInclude(a => a.Tutor)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null) return NotFound();

        application.Status = ApplicationStatus.Completed;
        application.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        // Enviar email de confirmação
        await _emailService.SendAdoptionConfirmationAsync(
            application.User.Email,
            application.User.Name,
            application.AnimalListing.Name,
            application.AnimalListing.Tutor?.Name ?? "Pet Shelter"
        );

        TempData["Success"] = "Adoption completed successfully!";
        return RedirectToAction("Manage", "Applications");
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }
}