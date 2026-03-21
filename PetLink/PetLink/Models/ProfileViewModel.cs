using PetLink.Models;
using PetLink.Models.Enums;
using System.Collections.Generic;

namespace PetLink.Models.ViewModels
{
    public class ProfileViewModel
    {
        // Dados do utilizador
        public User User { get; set; }
        
        // Lista de animais favoritos (Saved Pets)
        public List<AnimalListing> SavedPets { get; set; }
        
        // Lista de applications ativas
        public List<Application> ActiveApplications { get; set; }
        
        // Lista de mensagens recentes
        //public List<Message> RecentMessages { get; set; }
        
        // Estatísticas
        public int TotalApplications { get; set; }
        public int UnreadMessages { get; set; }
    }
}