using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetLink.Models
{
    /// <summary>
    /// Representa a associação entre um utilizador e um pet sitter
    /// que este marcou como favorito.
    /// </summary>
    public class FavoritePetsitter
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } 

        [Required]
        public int PetsitterId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("PetsitterId")]
        public virtual Petsitter Petsitter { get; set; }
    }
}