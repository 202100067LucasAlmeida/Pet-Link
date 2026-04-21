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

        [Required]
        public string Location { get; set; }

        public int AgeMonths { get; set; }
        public Age Age { get; set; }

        [Required]
        public string Description { get; set; }

        // Informação de saúde do animal
        public bool IsVaccinated { get; set; }
        public bool IsDewormed { get; set; }
        public bool IsSterilized { get; set; }

        public ListingStatus Status { get; set; } = ListingStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int TutorId { get; set; }

        [ForeignKey("TutorId")]
        public User Tutor { get; set; }

        public ICollection<FavoritePet> Favorites { get; set; }

        public String? ImageUrl { get; set; }
        public ICollection<AnimalPhoto> Photos { get; set; } = new List<AnimalPhoto>();
    }
    public class AnimalPhoto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Url { get; set; }

        public int AnimalListingId { get; set; }

        [ForeignKey("AnimalListingId")]
        public AnimalListing AnimalListing { get; set; }
    }
}