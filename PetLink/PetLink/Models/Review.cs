using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetLink.Models
{
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

        [MaxLength(500)]
        public string Comment { get; set; } // Comentário da avaliação

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