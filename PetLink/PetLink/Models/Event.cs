using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetLink.Models.Enums;

namespace PetLink.Models
{
    /// <summary>
    /// Representa um evento criado por uma associação ou abrigo,
    /// como ações de adoção, angariação de fundos, sensibilização ou voluntariado.
    /// Está sujeito a aprovação por parte de um administrador antes de ser publicado.
    /// </summary>
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

    /// <summary>
    /// Tipos de evento disponíveis na plataforma.
    /// </summary>
    public enum EventType
    {
        Adoption,
        Fundraising,
        Education,
        Volunteer,
        Other
    }

    /// <summary>
    /// Estado de um evento ao longo do seu ciclo de aprovação e realização.
    /// </summary>
    public enum EventStatus
    {
        /// <summary>Aguarda aprovação do Admin.</summary>
        Pending,
        /// <summary>Visível na listagem pública.</summary>
        Approved,
        /// <summary>Rejeitado.</summary>
        Rejected,
        /// <summary>Evento já realizado.</summary>
        Completed,
        /// <summary>Cancelado.</summary>
        Cancelled
    }
}