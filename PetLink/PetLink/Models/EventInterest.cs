using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetLink.Models
{
    /// <summary>
    /// Representa o interesse manifestado por um utilizador num evento.
    /// Permite registar a participação prevista e o respetivo estado de confirmação.
    /// </summary>
    public class EventInterest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        public bool IsConfirmed { get; set; } = false; 

        // Navigation properties
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}