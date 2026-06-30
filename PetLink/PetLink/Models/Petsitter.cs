using PetLink.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetLink.Models
{
    /// <summary>
    /// Representa o perfil de um Pet Sitter registado na plataforma.
    /// Contém informações sobre os serviços prestados, preferências,
    /// localização, preço e classificação.
    /// </summary>
    public class Petsitter
    {
        [Key]
        public int Id { get; set; }

        // Ligação ao Utilizador 
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }

        public int Age { get; set; }

        public ServiceType serviceType { get; set; }

        public PetPreferences petPreferences { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        public string Bio { get; set; }
        public double Rating { get; set; }
        public string LocationZone { get; set; }
        public double DistanceKm { get; set; }

        // Tags separadas por vírgula 
        public string SpecialtyTags { get; set; }
    }
}