using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetLink.Models.Enums;
using System.Collections.Generic;

namespace PetLink.Models
{
    /// <summary>
    /// Representa um anúncio de adoção de um animal.
    /// Contém os dados do animal, o seu estado de saúde, fotografias,
    /// o estado de publicação e o tutor (abrigo) responsável pelo anúncio.
    /// </summary>
    public class AnimalListing
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public Species Species { get; set; }

        [Required]
        public string Location { get; set; }

        public int AgeMonths { get; set; }
        public Age Age { get; set; }

        [Required]
        public string Description { get; set; }

        // Informação de saúde do animal
        public ICollection<HealthDocument> HealthDocuments { get; set; } = new List<HealthDocument>();


        public ListingStatus Status { get; set; } = ListingStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int TutorId { get; set; }

        [ForeignKey("TutorId")]
        public User Tutor { get; set; }

        public ICollection<FavoritePet> Favorites { get; set; }

        public String? ImageUrl { get; set; }
        public ICollection<AnimalPhoto> Photos { get; set; } = new List<AnimalPhoto>();



        //Uns extras da parte da saúde, para não mudar muito a lógica de verificação que tinhamos

        //nota: o NotMapped significa que não é criada uma coluna na base de dados para este "atributo"
        [NotMapped]
        public bool IsVaccinated => HealthDocuments?.Any(d => d.Type == HealthDocumentType.Vaccine) ?? false;

        [NotMapped]
        public bool IsDewormed => HealthDocuments?.Any(d => d.Type == HealthDocumentType.Deworming) ?? false;

        [NotMapped]
        public bool IsSterilized => HealthDocuments?.Any(d => d.Type == HealthDocumentType.Sterilization) ?? false;

        [NotMapped]
        public string? VaccinationProofUrl => HealthDocuments?.FirstOrDefault(d => d.Type == HealthDocumentType.Vaccine)?.FilePath;

        [NotMapped]
        public string? DewormingProofUrl => HealthDocuments?.FirstOrDefault(d => d.Type == HealthDocumentType.Deworming)?.FilePath;

        [NotMapped]
        public string? SterilizationProofUrl => HealthDocuments?.FirstOrDefault(d => d.Type == HealthDocumentType.Sterilization)?.FilePath;
    }

    /// <summary>
    /// Representa uma fotografia da galeria de um anúncio de animal.
    /// </summary>
    public class AnimalPhoto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Url { get; set; }

        public int AnimalListingId { get; set; }

        [ForeignKey("AnimalListingId")]
        public AnimalListing AnimalListing { get; set; }
    }
}