using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetLink.Models.Enums;
using System.Collections.Generic;

namespace PetLink.Models
{

    public class ChatMessage
    {
        public int Id { get; set; }

        // Quem envia a mensagem
        public int SenderId { get; set; }
        public User Sender { get; set; }

        // Quem recebe a mensagem 
        public int ReceiverId { get; set; }
        public User Receiver { get; set; }

        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;

        public int? AnimalListingId { get; set; }
    }
}