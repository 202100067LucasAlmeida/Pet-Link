using System.Threading.Tasks;

namespace PetLink.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendAdoptionConfirmationAsync(string toEmail, string userName, string animalName, string shelterName);
        Task SendNewMessageNotificationAsync(string toEmail, string receiverName, string senderName, string messagePreview);
    }
}