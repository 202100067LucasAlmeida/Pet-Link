using PetLink.Models;
using System.Collections.Generic;

namespace PetLink.ViewModels
{
    public class MessagesViewModel
    {
        // Lista de todas as conversas do utilizador 
        public List<ConversationSummary> Conversations { get; set; } = new List<ConversationSummary>();

        // A conversa que está atualmente aberta 
        public ConversationDetail ActiveConversation { get; set; }
    }

    public class ConversationSummary
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string OtherUserImageUrl { get; set; } 
        public string LastMessagePreview { get; set; }
        public DateTime LastMessageTimestamp { get; set; }
        public bool IsActive { get; set; } 
        public bool IsOnline { get; set; } 
    }

    // Detalhes da conversa ativa 
    public class ConversationDetail
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string OtherUserImageUrl { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}