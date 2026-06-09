namespace PetLink.Services
{
    public interface IChatbotService
    {
        Task<string> GetBotResponseAsync(string userMessage, int? currentUserId);
    }
}
