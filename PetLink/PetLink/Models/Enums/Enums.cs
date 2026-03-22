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

    public enum Breed
    {

    }

    public enum ListingStatus
    {
        Pendent,   // Aguarda aprovação do Admin 
        Published,  // Visível na pesquisa
        Rejected,  // Chumbado pelo Admin
        Adopted     // Adoção concluída
    }

    public enum UserRole
    {
        User,
        Shelter,
        PetSitter,
        Admin
    
    }
}