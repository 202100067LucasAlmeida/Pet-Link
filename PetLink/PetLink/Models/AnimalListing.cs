using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetLink.Models.Enums;

namespace PetLink.Models
{
    public class AnimalListing
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public Species Species { get; set; }

        [Required]
        public string Location { get; set; } // Pode ser Distrito/Concelho

        public int AgeMonths { get; set; }

        [Required]
        public string Description { get; set; }

        // Checklist de Saúde (Requisito R24)
        public bool IsVaccinated { get; set; }
        public bool IsDewormed { get; set; }
        public bool IsSterilized { get; set; }

        public ListingStatus Status { get; set; } = ListingStatus.Pendent;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key para o Tutor (Utilizador que criou o anúncio)
        public int TutorId { get; set; }
        
        [ForeignKey("TutorId")]
        public User Tutor { get; set; } // Propriedade de Navegação
    }
}