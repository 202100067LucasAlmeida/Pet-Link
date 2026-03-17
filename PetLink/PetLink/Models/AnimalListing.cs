using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetLink.Models.Enums;
using System.Collections.Generic;

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
        public Breed Breed { get; set; }

        [Required]
        public string Location { get; set; } 

        public int AgeMonths { get; set; }

        [Required]
        public string Description { get; set; }

        // Informação de saúde do animal
        public bool IsVaccinated { get; set; }
        public bool IsDewormed { get; set; }
        public bool IsSterilized { get; set; }

        public ListingStatus Status { get; set; } = ListingStatus.Pendent;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Chave estrangeira para o utilizador que criou o anúncio 
        public int TutorId { get; set; }
        
        [ForeignKey("TutorId")]
        public User Tutor { get; set; } 

         public ICollection<FavoritePet> Favorites { get; set; }
    }
}