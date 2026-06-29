using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;
using PetLink.Models.ViewModels;

namespace PetLink.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão dos favoritos do utilizador autenticado.
    /// Permite adicionar e remover anúncios de animais e pet sitters dos favoritos,
    /// bem como consultar e verificar o estado de cada favorito.
    /// Acessível apenas a utilizadores com os papéis User ou PetSitter.
    /// </summary>
    [Authorize(Roles = "User, PetSitter")]
    public class FavoritesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FavoritesController> _logger;

        /// <summary>
        /// Inicializa uma nova instância do controlador de favoritos.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        /// <param name="logger">Serviço de logging.</param>

        public FavoritesController(ApplicationDbContext context, ILogger<FavoritesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Apresenta os favoritos do utilizador autenticado,
        /// incluindo tanto anúncios de animais como pet sitters guardados.
        /// </summary>
        /// <returns>Vista com os favoritos do utilizador.</returns>
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
        .ThenInclude(p => p.User)
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

        /// <summary>
        /// Adiciona ou remove um anúncio de animal dos favoritos do utilizador (comportamento de toggle).
        /// Caso o anúncio já esteja nos favoritos, é removido; caso contrário, é adicionado.
        /// Devolve o resultado em formato JSON.
        /// </summary>
        /// <param name="animalListingId">Identificador do anúncio de animal.</param>
        /// <returns>Resposta JSON indicando o resultado da operação e o novo estado do favorito.</returns>
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

        /// <summary>
        /// Verifica se um anúncio de animal se encontra nos favoritos do utilizador autenticado.
        /// Devolve o resultado em formato JSON.
        /// </summary>
        /// <param name="animalListingId">Identificador do anúncio de animal.</param>
        /// <returns>
        /// Resposta JSON com <c>true</c> se o anúncio estiver nos favoritos;
        /// caso contrário, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Apresenta os favoritos do utilizador autenticado,
        /// incluindo anúncios de animais e pet sitters guardados.
        /// </summary>
        /// <returns>Vista com os favoritos do utilizador.</returns>
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
                .ThenInclude(p => p.User)
            .Select(f => f.Petsitter)
            .ToListAsync();

            var viewModel = new FavoritesViewModel
            {
                FavoritePets = favoritePets,
                FavoritePetsitters = favoritePetsitters
             };

            return View(viewModel);
        }

        /// <summary>
        /// Adiciona ou remove um pet sitter dos favoritos do utilizador (comportamento de toggle).
        /// Caso o pet sitter já esteja nos favoritos, é removido; caso contrário, é adicionado.
        /// Devolve o resultado em formato JSON.
        /// </summary>
        /// <param name="petsitterId">Identificador do pet sitter.</param>
        /// <returns>Resposta JSON indicando o resultado da operação e o novo estado do favorito.</returns>
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
                var petsitter = await _context.Petsitters
                    .FirstOrDefaultAsync(p => p.Id == petsitterId);

                if (petsitter == null)
                {
                     return Json(new { success = false, message = "Petsitter not available" });
                }

                var existingFavorite = await _context.FavoritePetsitters
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.PetsitterId == petsitter.Id);

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
                        PetsitterId = petsitter.Id,
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

        /// <summary>
        /// Verifica se um pet sitter se encontra nos favoritos do utilizador autenticado.
        /// Devolve o resultado em formato JSON.
        /// </summary>
        /// <param name="petsitterId">Identificador do pet sitter.</param>
        /// <returns>
        /// Resposta JSON com <c>true</c> se o pet sitter estiver nos favoritos;
        /// caso contrário, <c>false</c>.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> CheckPetsitter(int petsitterId)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Json(false);

            var isFavorited = await _context.FavoritePetsitters
                .AnyAsync(f => f.UserId == userId && f.PetsitterId == petsitterId);

            return Json(isFavorited);
        }


    }
}