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
        public async Task<IActionResult> Create(int animalListingId)
        {
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (currentUserId == 0) return Challenge();

            // Verificar se o utilizador adotou este animal
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.UserId == currentUserId && 
                                         a.AnimalListingId == animalListingId &&
                                         a.Status == ApplicationStatus.Completed);

            if (application == null)
            {
                TempData["Error"] = "You can only review pets you have adopted.";
                return RedirectToAction("Index", "AnimalListings");
            }

            // Verificar se já fez review
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewerId == currentUserId && 
                                         r.AnimalListingId == animalListingId);

            if (existingReview != null)
            {
                TempData["Error"] = "You have already reviewed this adoption.";
                return RedirectToAction("Details", "AnimalListings", new { id = animalListingId });
            }

            var animal = await _context.AnimalListings
                .Include(a => a.Tutor)
                .FirstOrDefaultAsync(a => a.Id == animalListingId);

            if (animal == null) return NotFound();

            // Verificar se o tutor pode receber avaliações
            if (animal.Tutor != null && animal.Tutor.Role != UserRole.User && animal.Tutor.Role != UserRole.PetSitter)
            {
                TempData["Error"] = "This user cannot receive reviews.";
                return RedirectToAction("Details", "AnimalListings", new { id = animalListingId });
            }

            var viewModel = new CreateReviewViewModel
            {
                AnimalListingId = animalListingId,
                AnimalName = animal.Name,
                ReviewedId = animal.TutorId,
                ReviewedName = animal.Tutor?.Name
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