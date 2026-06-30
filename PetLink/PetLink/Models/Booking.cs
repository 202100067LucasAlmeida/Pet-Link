using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetLink.Models.Enums;

namespace PetLink.Models
{
    /// <summary>
    /// Representa uma reserva de um serviço de pet sitting,
    /// efetuada por um utilizador junto de um pet sitter.
    /// Regista as datas, o preço calculado e o estado da reserva ao longo do seu ciclo de vida.
    /// </summary>
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PetsitterId { get; set; }

        public ServiceType ServiceType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string? PetName { get; set; }

        public string? PetSpecies { get; set; }

        public string? Message { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("PetsitterId")]
        public virtual Petsitter Petsitter { get; set; }
    }
}
