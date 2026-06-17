using System.ComponentModel.DataAnnotations;
using PetLink.Models.Enums;
using System.Collections.Generic;
using PetLink.Models;
using System.ComponentModel.DataAnnotations.Schema; 

namespace PetLink.Models
{
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

        public UserRole Role { get; set; }

        // Indica se a Associação/PetSitter já foi verificada pelo Admin 
        public bool IsVerified { get; set; } = false; 
        public bool IsExternalLogin { get; set; }

        
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? Lat { get; set; }
        public string? Lon { get; set; }

        public void UpdateCoordinates(string lat, string lon)
        {
            Lat = lat;
            Lon = lon;
        }
        public string GetLatitude() => Lat;
        public string GetLongitude() => Lon;

        public string? Bio { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<AnimalListing> Listings { get; set; }
        public ICollection<FavoritePet> FavoritePets {get; set;}
        public ICollection<Application> Applications { get; set; }
        public ICollection<Message> SentMessages { get; set; }
        public ICollection<Message> ReceivedMessages { get; set; }
        public virtual ICollection<Review> ReviewsReceived { get; set; } // Avaliações recebidas
        public virtual ICollection<Review> ReviewsGiven { get; set; } // Avaliações feitas
        public ICollection<FavoritePetsitter> FavoritePetsitters { get; set; }

// Propriedades calculadas (não mapeadas na BD)
[NotMapped]
public double AverageRating => ReviewsReceived != null && ReviewsReceived.Any() 
    ? ReviewsReceived.Average(r => r.Rating) 
    : 0;

[NotMapped]
public int TotalReviews => ReviewsReceived?.Count ?? 0;
    }
}