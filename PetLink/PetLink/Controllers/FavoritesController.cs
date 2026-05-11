using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;
using PetLink.Models.ViewModels;

namespace PetLink.Controllers
{
    [Authorize(Roles = "User, PetSitter")]
    public class FavoritesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FavoritesController> _logger;

        /// <summary>
        /// Extracts authenticated user ID from claims with null safety
        /// </summary>

        public FavoritesController(ApplicationDbContext context, ILogger<FavoritesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET: Favorites/Index - Shows user's favorited listings
        /// </summary>
        public async Task<IActionResult> Index()
        {
            if (!GetCurrentUserId(out int userId))
            {
                return Challenge();
            }

            var favoritePets = await _context.FavoritePets
        .Where(f => f.UserId == userId)
        .Include(f => f.AnimalListing)
        .Select(f => f.AnimalListing)
        .Where(a => a.Status == ListingStatus.Published)
        .ToListAsync();

    // Buscar petsitters favoritos
    var favoritePetsitters = await _context.FavoritePetsitters
        .Where(f => f.UserId == userId)
        .Include(f => f.Petsitter)
        .Select(f => f.Petsitter)
        .ToListAsync();

    // Criar o ViewModel com ambas as listas
    var viewModel = new FavoritesViewModel
    {
        FavoritePets = favoritePets,
        FavoritePetsitters = favoritePetsitters
    };

    return View("MyFavorites", viewModel);
        }

        // POST: Favorites/Toggle/5
        [HttpPost]
        public async Task<IActionResult> Toggle(int animalListingId)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var animalListing = await _context.AnimalListings
                    .FirstOrDefaultAsync(a => a.Id == animalListingId && a.Status == ListingStatus.Published);

                if (animalListing == null)
                {
                    return Json(new { success = false, message = "Animal listing not available" });
                }

                var existingFavorite = await _context.FavoritePets
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.AnimalListingId == animalListingId);

                if (existingFavorite != null)
                {
                    _context.FavoritePets.Remove(existingFavorite);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, isFavorited = false, message = "Removed from favorites" });
                }
                else
                {
                    var favorite = new FavoritePet
                    {
                        UserId = userId,
                        AnimalListingId = animalListingId,
                        CreatedAt = DateTime.Now
                    };
                    _context.FavoritePets.Add(favorite);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, isFavorited = true, message = "Added to favorites" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling favorite for animal {AnimalListingId}", animalListingId);
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        // GET: Favorites/Check/5
        [HttpGet]
        public async Task<IActionResult> Check(int animalListingId)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(false);
            }

            var isFavorited = await _context.FavoritePets
                .AnyAsync(f => f.UserId == userId && f.AnimalListingId == animalListingId);

            return Json(isFavorited);
        }

        // GET: Favorites/MyFavorites 
        public async Task<IActionResult> MyFavorites()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Challenge();
            }

            var favoritePets = await _context.FavoritePets
            .Where(f => f.UserId == userId)
            .Include(f => f.AnimalListing)
            .Select(f => f.AnimalListing)
            .Where(a => a.Status == ListingStatus.Published)
            .ToListAsync();

    
            var favoritePetsitters = await _context.FavoritePetsitters
            .Where(f => f.UserId == userId)
            .Include(f => f.Petsitter)
            .Select(f => f.Petsitter)
            .ToListAsync();

            var viewModel = new FavoritesViewModel
            {
                FavoritePets = favoritePets,
                FavoritePetsitters = favoritePetsitters
             };

            return View(viewModel);
        }

        // POST: Favorites/TogglePetsitter/5
        [HttpPost]
        public async Task<IActionResult> TogglePetsitter(int petsitterId)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

        // Verificar se o petsitter existe e é realmente um PetSitter
                var petsitter = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == petsitterId && u.Role == UserRole.PetSitter);

                if (petsitter == null)
                {
                     return Json(new { success = false, message = "Petsitter not available" });
                }

                var existingFavorite = await _context.FavoritePetsitters
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.PetsitterId == petsitterId);

                if (existingFavorite != null)
                {
                    _context.FavoritePetsitters.Remove(existingFavorite);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, isFavorited = false, message = "Removed from favorites" });
                }
                else
                {
                    var favorite = new FavoritePetsitter
                    {
                        UserId = userId,
                        PetsitterId = petsitterId,
                        CreatedAt = DateTime.Now
                    };
                    _context.FavoritePetsitters.Add(favorite);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, isFavorited = true, message = "Added to favorites" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        // GET: Favorites/CheckPetsitter/5
        [HttpGet]
        public async Task<IActionResult> CheckPetsitter(int petsitterId)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                 return Json(false);
            }

            var isFavorited = await _context.FavoritePetsitters
            .AnyAsync(f => f.UserId == userId && f.PetsitterId == petsitterId);

            return Json(isFavorited);
        }

        
    }
}