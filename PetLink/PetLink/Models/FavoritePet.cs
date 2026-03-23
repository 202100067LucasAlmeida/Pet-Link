using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetLink.Models
{
    public class FavoritePet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } 

        [Required]
        public int AnimalListingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("AnimalListingId")]
        public virtual AnimalListing AnimalListing { get; set; }
    }
}