using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetLink.Models.Enums;

namespace PetLink.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; }

        public EventType Type { get; set; }

        public EventStatus Status { get; set; } = EventStatus.Pending;

        public string ImageUrl { get; set; }

        [Required]
        public int OrganizerId { get; set; } // UserId da Associação/Abrigo

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; } // AdminId que aprovou
        
        // Navigation properties
        [ForeignKey("OrganizerId")]
        public virtual User Organizer { get; set; }

        public bool AcceptsDonations { get; set; } = false;
        public bool AcceptsVolunteers { get; set; } = false;
    }

    public enum EventType
    {
        Adoption,
        Fundraising,
        Education,
        Volunteer,
        Other
    }

    public enum EventStatus
    {
        Pending,    // Aguarda aprovação do Admin
        Approved,   // Visível na listagem pública
        Rejected,   // Rejeitado
        Completed,  // Evento já realizado
        Cancelled   // Cancelado
    }
}