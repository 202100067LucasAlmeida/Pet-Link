using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetLink.Models.Enums;


namespace PetLink.Models
{
    public class HealthDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public HealthDocumentType Type { get; set; }

        [Required]
        public string FilePath { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Chave estrangeira pra ligar ao anúncio
        public int AnimalListingId { get; set; }

        [ForeignKey("AnimalListingId")]
        public AnimalListing AnimalListing { get; set; }

        //verificações da saúde
        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedAt { get; set; }

        public int? VerifiedByAdminId { get; set; }

        [ForeignKey("VerifiedByAdminId")]
        public User VerifiedByAdmin { get; set; }
    }
}