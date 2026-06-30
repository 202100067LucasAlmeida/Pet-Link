using System.ComponentModel.DataAnnotations;
using PetLink.Models.Enums;

namespace PetLink.Models
{
    /// <summary>
    /// Representa um recurso educativo da plataforma PetLink.
    /// Estes recursos podem incluir artigos, vídeos ou outros conteúdos informativos
    /// direcionados a diferentes espécies e categorias de cuidado animal.
    /// </summary>
    public class Resource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Content { get; set; }

        [MaxLength(500)]
        public string? MediaUrl { get; set; }

        [Required]
        public ResourceType Type { get; set; }

        [Required]
        public Species Species { get; set; }

        [Required]
        public ResourceCategory Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
