namespace PetLink.Models.ViewModels
{
    /// <summary>
    /// ViewModel utilizado na página de mensagens.
    /// Contém a lista de conversas do utilizador e a conversa atualmente selecionada.
    /// </summary>
    public class MessagesViewModel
    {
        public List<ConversationSummary> Conversations { get; set; } = new List<ConversationSummary>();
        public ConversationDetail? ActiveConversation { get; set; }
    }

    /// <summary>
    /// Representa um resumo de uma conversa apresentada na lista lateral.
    /// Inclui informações sobre o outro utilizador, o anúncio associado e a última mensagem.
    /// </summary>
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

    /// <summary>
    /// Representa os detalhes completos de uma conversa,
    /// incluindo todas as mensagens trocadas entre os utilizadores.
    /// </summary>
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