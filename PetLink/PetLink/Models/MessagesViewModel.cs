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
        public string? OtherUserImagePath { get; set; }

        // NOVOS CAMPOS: Identificação do contexto da conversa
        public int? AnimalListingId { get; set; }
        public string? AnimalName { get; set; }
        public string? AnimalImagePath { get; set; }

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
        public string? OtherUserImagePath { get; set; }

        // NOVOS CAMPOS: Identificação do contexto da conversa
        public int? AnimalListingId { get; set; }
        public string? AnimalName { get; set; }

        public List<Message> Messages { get; set; } = new List<Message>();
    }
}