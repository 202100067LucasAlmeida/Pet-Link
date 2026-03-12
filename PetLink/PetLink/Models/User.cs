using System.ComponentModel.DataAnnotations;
using PetLink.Models.Enums;

namespace PetLink.Models
{
    public class User
    {
        [Key] // Define que é a primary key
        public int Id { get; set; }

        [RegularExpression(@"^[\p{L}\s\-']+$", ErrorMessage = "O nome não pode conter simbolos ou números!")]
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public UserRole Role { get; set; }

        // Indica se a Associação/PetSitter já foi verificada pelo Admin 
        public bool IsVerified { get; set; } = false; 

        
        public ICollection<AnimalListing> Listings { get; set; }
    }
}