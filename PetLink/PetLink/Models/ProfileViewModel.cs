using PetLink.Models;
using PetLink.Models.Enums;
using System.Collections.Generic;

namespace PetLink.Models.ViewModels
{
    public class ProfileViewModel
    {
        public User User { get; set; }
        
        public List<AnimalListing> SavedPets { get; set; }
        
        public List<Application> ActiveApplications { get; set; }
        
        public List<Message> RecentConversations { get; set; }
        
        public int TotalApplications { get; set; }
        public int UnreadMessages { get; set; }
        public int DaysSinceJoined { get; set; }
    }
}