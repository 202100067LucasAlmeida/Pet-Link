using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace PetLink.Models
{
    /// <summary>
    /// Representa uma notificação enviada a um utilizador,
    /// podendo estar associada a um anúncio de animal específico.
    /// </summary>
    public class ListingsNotification
    {
        //as notificações vão ter a estrutura:
        //ID, User, Título, Aviso, Listing, Lido, Data

        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public int? AnimalListingId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // propriedades para navegar
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("AnimalListingId")]
        public virtual AnimalListing AnimalListing { get; set; }

    }
}