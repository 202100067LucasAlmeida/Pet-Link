using System.ComponentModel.DataAnnotations;
using PetLink.Models.Enums;

namespace PetLink.Models
{
    public class User
    {
        [Key] // Define que é a Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public UserRole Role { get; set; }

        // Indica se a Associação/PetSitter já foi verificada pelo Admin (Requisito R10)
        public bool IsVerified { get; set; } = false; 

        // Relação 1 para Muitos: Um utilizador pode ter vários anúncios
        public ICollection<AnimalListing> Listings { get; set; }
    }
}