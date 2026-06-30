namespace PetLink.Models.Enums
{
    /// <summary>
    /// Espécies de animais suportadas pela plataforma.
    /// </summary>
    public enum Species
    {
        Dog,
        Cat,
        Rodent,
        Bird,
        Reptile,
        Fish,
        Other
    }

    /// <summary>
    /// Faixas etárias utilizadas para classificar os animais.
    /// </summary>
    public enum Age
    {
        Puppy,
        Adult,
        Senior
    }

    /// <summary>
    /// Tipos de serviço disponibilizados pelos pet sitters.
    /// </summary>
    public enum ServiceType
    {
        Boarding,
        Walking,
        HouseSitting
    }

    /// <summary>
    /// Preferências de tipo de animal indicadas pelos pet sitters.
    /// </summary>
    public enum PetPreferences
    {
        SmallDogs,
        Cats,
        Exotic
    }

    /// <summary>
    /// Estado de um anúncio de adoção ao longo do seu ciclo de vida.
    /// </summary>
    public enum ListingStatus
    {
        /// <summary>Aguarda aprovação do Administrador.</summary>
        Pending = 1,
        /// <summary>Visível para todos na pesquisa.</summary>
        Published = 2,
        /// <summary>Rejeitado pelo Administrador.</summary>
        Rejected = 3,
        /// <summary>Processo de adoção concluído.</summary>
        Adopted = 4
    }

    /// <summary>
    /// Estado de uma candidatura de adoção submetida por um utilizador.
    /// </summary>
    public enum ApplicationStatus
    {
        /// <summary>Utilizador enviou a candidatura ao Tutor.</summary>
        Pending = 1,
        /// <summary>Tutor aceitou iniciar o processo.</summary>
        Approved = 2,
        /// <summary>Tutor rejeitou o candidato.</summary>
        Rejected = 3,
        /// <summary>Adoção finalizada.</summary>
        Completed = 4
    }

    /// <summary>
    /// Papéis de utilizador reconhecidos pela plataforma.
    /// </summary>
    public enum UserRole
    {
        User,
        Shelter,
        PetSitter,
        Admin
    }

    /// <summary>
    /// Tipos de documento de saúde associados a um anúncio de animal.
    /// </summary>
    public enum HealthDocumentType
    {
        /// <summary>Vacina.</summary>
        Vaccine,
        /// <summary>Desparasitação.</summary>
        Deworming,
        /// <summary>Esterilização.</summary>
        Sterilization
    }

    /// <summary>
    /// Estado de uma reserva de serviço de pet sitting.
    /// </summary>
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Rejected,
        Completed,
        Cancelled
    }

    /// <summary>
    /// Formato de um recurso informativo disponibilizado na plataforma.
    /// </summary>
    public enum ResourceType
    {
        Article,
        Video
    }

    /// <summary>
    /// Categoria temática de um recurso informativo.
    /// </summary>
    public enum ResourceCategory
    {
        Health,
        Training,
        Nutrition,
        General
    }
}