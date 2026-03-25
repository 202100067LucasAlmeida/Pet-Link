using PetLink.Models;
using System.Collections.Generic;

namespace PetLink.Models.ViewModels
{
    public class MessagesViewModel
    {
        public List<ConversationSummary> Conversations { get; set; } = new List<ConversationSummary>();
        public ConversationDetail? ActiveConversation { get; set; }
    }

    public class ConversationSummary
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string LastMessagePreview { get; set; } = string.Empty;
        public DateTime LastMessageTimestamp { get; set; }
        public bool IsActive { get; set; }
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; }
    }

    public class ConversationDetail
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}

