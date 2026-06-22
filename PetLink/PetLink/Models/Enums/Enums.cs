namespace PetLink.Models.Enums
{
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

    public enum Age
    {

        Puppy,
        Adult,
        Senior
    }

    public enum ServiceType
    {
        Boarding,
        Walking,
        HouseSitting
    }

    public enum PetPreferences
    {
        SmallDogs,
        Cats,
        Exotic
    }

    public enum ListingStatus
    {
        Pending = 1,  // Aguarda aprovação do Administrador
        Published = 2,     // Visível para todos na pesquisa
        Rejected = 3,      // Rejeitado pelo Administrador
        Adopted = 4        // Processo de adoção concluído
    }

    public enum ApplicationStatus
    {
        Pending = 1,       // Utilizador enviou a mensagem ao Tutor
        Approved = 2,      // Tutor aceitou iniciar o processo
        Rejected = 3,      // Tutor rejeitou o candidato
        Completed = 4      // Adoção finalizada
    }


    public enum UserRole
    {
        User,
        Shelter,
        PetSitter,
        Admin

    }

    public enum HealthDocumentType
    {
        Vaccine,      // Vacina
        Deworming,    // Desparasitação 
        Sterilization // Esterilização
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Rejected,
        Completed,
        Cancelled
    }
}