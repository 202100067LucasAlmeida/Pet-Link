using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetLink.Models
{
    /// <summary>
    /// Representa uma avaliação deixada por um utilizador após uma adoção ou serviço de pet sitting.
    /// Permite avaliar a experiência associada a um utilizador e a um anúncio específico.
    /// </summary>
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReviewerId { get; set; } // Quem fez a avaliação (user)

        [Required]
        public int ReviewedId { get; set; } // Quem foi avaliado (Tutor/PetSitter)

        [Required]
        public int AnimalListingId { get; set; } // Animal que foi adotado ou no qual foi usado o serviço de petsitting

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } // Nota de 1 a 5 estrelas
    

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = false; // Admin precisa aprovar?

        // Navigation properties
        [ForeignKey("ReviewerId")]
        public virtual User Reviewer { get; set; }

        [ForeignKey("ReviewedId")]
        public virtual User Reviewed { get; set; }

        [ForeignKey("AnimalListingId")]
        public virtual AnimalListing AnimalListing { get; set; }
    }
}