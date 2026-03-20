using PetLink.Models;
using System.Collections.Generic;

namespace PetLink.ViewModels
{
    public class MessagesViewModel
    {
        // Lista de todas as conversas do utilizador (coluna da esquerda)
        public List<ConversationSummary> Conversations { get; set; } = new List<ConversationSummary>();

        // A conversa que está atualmente aberta (coluna da direita)
        public ConversationDetail ActiveConversation { get; set; }
    }

    // Resumo de uma conversa para a lista
    public class ConversationSummary
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string OtherUserImageUrl { get; set; } // Opcional
        public string LastMessagePreview { get; set; }
        public DateTime LastMessageTimestamp { get; set; }
        public bool IsActive { get; set; } // Para destacar a conversa selecionada
        public bool IsOnline { get; set; } // Opcional (bolinha verde)
    }

    // Detalhes da conversa ativa (para os balões de chat)
    public class ConversationDetail
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string OtherUserImageUrl { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}