using System.ComponentModel.DataAnnotations;
using PetLink.Models.Enums;
using System.Collections.Generic;
using PetLink.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetLink.Models
{
    /// <summary>
    /// Representa um utilizador da plataforma PetLink.
    /// Pode assumir diferentes papéis (User, PetSitter ou Shelter) e contém toda a informação
    /// necessária para autenticação, perfil e interação na plataforma.
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [RegularExpression(@"^[\p{L}\s\-']+$", ErrorMessage = "O nome não pode conter simbolos ou números!")]
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; }

        public string? PasswordHash { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public UserRole Role { get; set; }

        // Indica se a Associação/PetSitter já foi verificada pelo Admin 
        public bool IsVerified { get; set; } = false;
        public bool IsExternalLogin { get; set; }

        public string? Phone { get; set; }
        public string? City { get; set; }

        public string? Bio { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Coleções inicializadas para evitar erros de validação do ModelState
        public ICollection<AnimalListing> Listings { get; set; } = new List<AnimalListing>();
        public ICollection<FavoritePet> FavoritePets { get; set; } = new List<FavoritePet>();
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public virtual ICollection<Review> ReviewsReceived { get; set; } = new List<Review>(); // Avaliações recebidas
        public virtual ICollection<Review> ReviewsGiven { get; set; } = new List<Review>(); // Avaliações feitas
        public ICollection<FavoritePetsitter> FavoritePetsitters { get; set; } = new List<FavoritePetsitter>();

        // Propriedades calculadas (não mapeadas na BD)
        [NotMapped]
        public double AverageRating => ReviewsReceived != null && ReviewsReceived.Any()
            ? ReviewsReceived.Average(r => r.Rating)
            : 0;

        [NotMapped]
        public int TotalReviews => ReviewsReceived?.Count ?? 0;
    }
}