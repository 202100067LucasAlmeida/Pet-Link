using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetLink.Models
{
    /// <summary>
    /// Representa uma mensagem trocada entre dois utilizadores através do sistema de chat.
    /// A mensagem pode estar associada a um anúncio de adoção específico, permitindo
    /// manter conversas independentes para cada animal.
    /// </summary>
    public class Message
    {
        public int? AnimalListingId { get; set; } 
        public virtual AnimalListing AnimalListing { get; set; }
        public int Id { get; set; }

        // Quem enviou
        public int SenderId { get; set; }
        public virtual User Sender { get; set; }

        // Quem recebeu
        public int ReceiverId { get; set; }
        public virtual User Receiver { get; set; }

        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}