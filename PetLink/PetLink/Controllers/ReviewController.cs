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

    /// <summary>
    /// Controlador responsável pela gestão de avaliações entre utilizadores.
    /// Permite criar avaliações de adoção (entre adotante e abrigo) e de pet sitting,
    /// bem como consultar as avaliações recebidas por um utilizador.
    /// </summary>
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador de avaliações.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Apresenta o formulário de criação de uma avaliação.
        /// Determina o utilizador a avaliar e valida se o utilizador autenticado
        /// tem permissão para o fazer, de acordo com o tipo de avaliação (adoção ou pet sitting).
        /// </summary>
        /// <param name="animalListingId">Identificador do anúncio de animal associado à avaliação.</param>
        /// <param name="reviewedId">Identificador do utilizador a avaliar (obrigatório para avaliações de pet sitting).</param>
        /// <param name="reviewType">Tipo de avaliação: "Adoption" ou "Petsitter".</param>
        /// <returns>Vista de criação de avaliação ou redirecionamento caso a operação não seja permitida.</returns>
        [HttpGet]
        public async Task<IActionResult> Create(int animalListingId, int? reviewedId = null, string reviewType = "Adoption")
        {
            Console.WriteLine($"=== CREATE REVIEW ===");
            Console.WriteLine($"animalListingId: {animalListingId}");
            Console.WriteLine($"reviewedId: {reviewedId}");
            Console.WriteLine($"reviewType: {reviewType}");
            
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (currentUserId == 0) 
            {
                Console.WriteLine("User not authenticated - returning challenge");
                return Challenge();
            }

            var animal = await _context.AnimalListings
                .Include(a => a.Tutor)
                .FirstOrDefaultAsync(a => a.Id == animalListingId);

            if (animal == null) return NotFound();

            User reviewedUser = null;
            bool canReview = false;

            switch (reviewType)
            {
                case "Adoption":
                    // Se o utilizador atual é o Tutor (Shelter), vai avaliar o adotante
                    if (animal.TutorId == currentUserId)
                    {
                        // Buscar o adotante (quem completou a aplicação)
                        var application = await _context.Applications
                            .FirstOrDefaultAsync(a => a.AnimalListingId == animalListingId && 
                                                    (a.Status == ApplicationStatus.Completed ||
                              a.Status == ApplicationStatus.Approved));
                        
                        if (application != null)
                        {
                            reviewedUser = await _context.Users.FindAsync(application.UserId);
                            canReview = reviewedUser != null;
                        }
                    }
                    
                    break;

                case "Petsitter":
                    // User avalia Petsitter após serviço concluído
                    if (!reviewedId.HasValue)
                    {
                        TempData["Error"] = "Invalid petsitter review request.";
                        return RedirectToAction("Index", "AnimalListings");
                    }
                    
                    reviewedUser = await _context.Users.FindAsync(reviewedId.Value);
                    if (reviewedUser != null && reviewedUser.Role == UserRole.PetSitter)
                    {
                        // Verificar se há um serviço de petsitting concluído
                        var booking = await _context.Bookings
                            .FirstOrDefaultAsync(b => b.UserId == currentUserId && 
                                                     b.PetsitterId == reviewedId.Value &&
                                                     b.Status == BookingStatus.Completed);
                        
                        canReview = booking != null;
                        
                        if (!canReview)
                        {
                            TempData["Error"] = "You can only review a petsitter after a completed service.";
                            return RedirectToAction("MyBookings", "PetsittingBookings");
                        }
                    }
                    break;
            }

            if (!canReview || reviewedUser == null)
            {
                TempData["Error"] = "You cannot review this user.";
                return RedirectToAction("Details", "AnimalListings", new { id = animalListingId });
            }

            // Verificar se já fez review
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewerId == currentUserId && 
                                        r.ReviewedId == reviewedUser.Id &&
                                        r.AnimalListingId == animalListingId);

            if (existingReview != null)
            {
                TempData["Error"] = "You have already reviewed this user.";
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

        /// <summary>
        /// Submete uma nova avaliação após validar permissões e duplicações.
        /// A validação de permissões é repetida no servidor de acordo com o tipo de avaliação,
        /// garantindo que o utilizador autenticado tem efetivamente direito a avaliar.
        /// </summary>
        /// <param name="model">Dados da avaliação submetida.</param>
        /// <returns>Redireciona para os detalhes do anúncio ou para as reservas, dependendo do tipo de avaliação.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {

             Console.WriteLine($"Rating recebido: {model.Rating}");
            Console.WriteLine($"ModelState válido: {ModelState.IsValid}");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Erro: {error.ErrorMessage}");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (currentUserId == 0) return Challenge();

            // Verificar se já fez review
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewerId == currentUserId && 
                                        r.ReviewedId == model.ReviewedId &&
                                        r.AnimalListingId == model.AnimalListingId);

            if (existingReview != null)
            {
                TempData["Error"] = "You have already reviewed this user.";
                return RedirectToAction("Details", "AnimalListings", new { id = model.AnimalListingId });
            }

            // Verificar permissões baseado no tipo
            bool canReview = false;
            
            if (model.ReviewType == "Adoption")
            {
                var application = await _context.Applications
                    .FirstOrDefaultAsync(a => a.AnimalListingId == model.AnimalListingId && 
                                            (a.Status == ApplicationStatus.Completed ||
                              a.Status == ApplicationStatus.Approved));
                
                if (application != null)
                {
                    var animal = await _context.AnimalListings.FindAsync(model.AnimalListingId);
                    canReview = (animal.TutorId == currentUserId && application.UserId == model.ReviewedId) ||
                                (application.UserId == currentUserId && animal.TutorId == model.ReviewedId);
                }
            }
            else if (model.ReviewType == "Petsitter")
            {
                // Verificar se há um serviço de petsitting concluído
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.UserId == currentUserId && 
                                             b.PetsitterId == model.ReviewedId &&
                                             b.Status == BookingStatus.Completed);
                
                canReview = booking != null;
                
                if (!canReview)
                {
                    TempData["Error"] = "You can only review a petsitter after a completed service.";
                    return RedirectToAction("MyBookings", "PetsittingBookings");
                }
            }

            if (!canReview)
            {
                TempData["Error"] = "You are not authorized to review this user.";
                return RedirectToAction("Index", "AnimalListings");
            }

            var review = new Review
            {
                ReviewerId = currentUserId,
                ReviewedId = model.ReviewedId,
                AnimalListingId = model.AnimalListingId,
                Rating = model.Rating,
                CreatedAt = DateTime.Now,
                IsApproved = true
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you for your review!";
            
            if (model.ReviewType == "Petsitter")
            {
                return RedirectToAction("MyBookings", "PetsittingBookings");
            }
            
            return RedirectToAction("Details", "AnimalListings", new { id = model.AnimalListingId });
        }

        /// <summary>
        /// Apresenta as avaliações recebidas por um utilizador,
        /// incluindo a classificação média e o total de avaliações.
        /// Apenas utilizadores e pet sitters podem receber avaliações.
        /// </summary>
        /// <param name="userId">Identificador do utilizador a consultar.</param>
        /// <returns>Vista com as avaliações do utilizador e respetivas estatísticas.</returns>
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