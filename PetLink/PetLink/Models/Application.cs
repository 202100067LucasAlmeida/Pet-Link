using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetLink.Models.Enums;

namespace PetLink.Models
{
    /// <summary>
    /// Representa uma candidatura de adoção submetida por um utilizador
    /// para um anúncio de animal específico.
    /// Regista o progresso do processo de adoção e o respetivo estado.
    /// </summary>
    public class Application
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int AnimalListingId { get; set; }

        public string Message { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public int CurrentStep { get; set; } = 1;
        public int TotalSteps { get; set; } = 4;

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // propiedade de navegação
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("AnimalListingId")]
        public virtual AnimalListing AnimalListing { get; set; }
    }
}