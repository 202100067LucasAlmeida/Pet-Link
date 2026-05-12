using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;
using PetLink.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PetLink.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Review/Create/5
        [HttpGet]
public async Task<IActionResult> Create(int animalListingId, int? reviewedId = null)
{
    var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
    if (currentUserId == 0) return Challenge();

    var animal = await _context.AnimalListings
        .Include(a => a.Tutor)
        .FirstOrDefaultAsync(a => a.Id == animalListingId);

    if (animal == null) return NotFound();

    // Determinar quem está a ser avaliado
    // Se reviewedId for fornecido (ex: petsitter), usa esse, senão usa o Tutor
    var reviewedUser = reviewedId.HasValue 
        ? await _context.Users.FindAsync(reviewedId.Value)
        : animal.Tutor;

    if (reviewedUser == null) return NotFound();

    // Verificar permissão: só pode avaliar quem está envolvido na transação
    bool canReview = false;
    string reviewType = "Adoption";

    if (reviewedId.HasValue)
    {
        // Avaliação de Petsitter - verificar se existe serviço concluído
        // (tirar os comentarios quando for possivel contratar um petssiter)
        /*
        var booking = await _context.PetsittingBookings
            .FirstOrDefaultAsync(b => b.UserId == currentUserId && 
                                     b.PetsitterId == reviewedId.Value &&
                                     b.AnimalListingId == animalListingId &&
                                     b.Status == BookingStatus.Completed);
        canReview = booking != null;
        reviewType = "Petsitter";
        */
        
        canReview = true;
    }
    else
    {
        // Avaliação de Tutor (adoção)
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.UserId == currentUserId && 
                                     a.AnimalListingId == animalListingId &&
                                     a.Status == ApplicationStatus.Completed);
        canReview = application != null;
        reviewType = "Adoption";
    }

    if (!canReview)
    {
        TempData["Error"] = $"You can only review {reviewType.ToLower()} after the process is completed.";
        return RedirectToAction("Details", "AnimalListings", new { id = animalListingId });
    }

    // Verificar se já fez review
    var existingReview = await _context.Reviews
        .FirstOrDefaultAsync(r => r.ReviewerId == currentUserId && 
                                 r.ReviewedId == reviewedUser.Id &&
                                 r.AnimalListingId == animalListingId);

    if (existingReview != null)
    {
        TempData["Error"] = $"You have already reviewed this {reviewType.ToLower()}.";
        return RedirectToAction("Details", "AnimalListings", new { id = animalListingId });
    }

    // Verificar se o avaliado pode receber avaliações (User ou PetSitter)
    if (reviewedUser.Role != UserRole.User && reviewedUser.Role != UserRole.PetSitter)
    {
        TempData["Error"] = "This user cannot receive reviews.";
        return RedirectToAction("Details", "AnimalListings", new { id = animalListingId });
    }

    var viewModel = new CreateReviewViewModel
    {
        AnimalListingId = animalListingId,
        AnimalName = animal.Name,
        ReviewedId = reviewedUser.Id,
        ReviewedName = reviewedUser.Name,
        ReviewType = reviewType
    };

    return View(viewModel);
}

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (currentUserId == 0) return Challenge();

            // Verificar novamente se pode avaliar
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.UserId == currentUserId && 
                                         a.AnimalListingId == model.AnimalListingId &&
                                         a.Status == ApplicationStatus.Completed);

            if (application == null)
            {
                TempData["Error"] = "You can only review pets you have adopted.";
                return RedirectToAction("Index", "AnimalListings");
            }

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewerId == currentUserId && 
                                         r.AnimalListingId == model.AnimalListingId);

            if (existingReview != null)
            {
                TempData["Error"] = "You have already reviewed this adoption.";
                return RedirectToAction("Details", "AnimalListings", new { id = model.AnimalListingId });
            }

            // Verificar se o tutor pode receber avaliações
            var tutor = await _context.Users.FindAsync(model.ReviewedId);
            if (tutor != null && tutor.Role != UserRole.User && tutor.Role != UserRole.PetSitter)
            {
                TempData["Error"] = "This user cannot receive reviews.";
                return RedirectToAction("Details", "AnimalListings", new { id = model.AnimalListingId });
            }

            var review = new Review
            {
                ReviewerId = currentUserId,
                ReviewedId = model.ReviewedId,
                AnimalListingId = model.AnimalListingId,
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedAt = DateTime.Now,
                IsApproved = true
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you for your review!";
            return RedirectToAction("Details", "AnimalListings", new { id = model.AnimalListingId });
        }

        // GET: Review/UserReviews/5
        [HttpGet]
        public async Task<IActionResult> UserReviews(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            // Verificar se este user pode receber avaliações
            var canReceiveReviews = (user.Role == UserRole.User || user.Role == UserRole.PetSitter);
            
            var reviews = new List<Review>();
            var averageRating = 0.0;
            var totalReviews = 0;
            
            if (canReceiveReviews)
            {
                reviews = await _context.Reviews
                    .Where(r => r.ReviewedId == userId && r.IsApproved)
                    .Include(r => r.Reviewer)
                    .Include(r => r.AnimalListing)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
                
                totalReviews = reviews.Count;
                averageRating = totalReviews > 0 ? reviews.Average(r => r.Rating) : 0;
            }

            var viewModel = new UserReviewsViewModel
            {
                User = user,
                Reviews = reviews,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                CanReceiveReviews = canReceiveReviews
            };

            return View(viewModel);
        }
    }
}