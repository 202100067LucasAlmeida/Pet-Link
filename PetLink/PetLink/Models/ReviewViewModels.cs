using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetLink.Models.ViewModels
{
    /// <summary>
    /// ViewModel utilizado para criação de uma nova avaliação (review).
    /// Contém informação sobre o utilizador avaliado, o anúncio e a nota atribuída.
    /// </summary>
    public class CreateReviewViewModel
    {
        public int AnimalListingId { get; set; }
        public string? AnimalName { get; set; }
        public int ReviewedId { get; set; }
        public string? ReviewedName { get; set; }
        public string ReviewType { get; set; }

        [Required(ErrorMessage = "Please select a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

    }

    /// <summary>
    /// ViewModel utilizado para apresentar todas as reviews de um utilizador.
    /// Inclui estatísticas e informação agregada da reputação.
    /// </summary>
    public class UserReviewsViewModel
    {
        public User User { get; set; }
        public List<Review> Reviews { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public bool CanReceiveReviews { get; set; }  // Adicionado
    }

    /// <summary>
    /// ViewModel utilizado para a página de perfil de um Tutor.
    /// Contém dados do tutor, avaliações e anúncios associados.
    /// </summary>
    public class TutorProfileViewModel
    {
        public User Tutor { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<Review> RecentReviews { get; set; }
        public List<AnimalListing> Listings { get; set; }
    }

    /// <summary>
    /// Modelo simplificado utilizado para apresentação de uma review na UI.
    /// </summary>
    public class ReviewDisplayViewModel
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReviewerName { get; set; }
        public string AnimalName { get; set; }
    }
}