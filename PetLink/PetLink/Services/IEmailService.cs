using System.Threading.Tasks;

namespace PetLink.Services
{
    /// <summary>
    /// Contrato para o serviço de envio de emails da plataforma PetLink.
    /// Define os diferentes tipos de emails transacionais suportados pelo sistema,
    /// como notificações de mensagens e confirmações de adoção.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envia um email genérico em formato HTML.
        /// Método base utilizado pelos restantes tipos de email.
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string body);
        /// <summary>
        /// Envia email de confirmação de adoção após conclusão do processo.
        /// Informa o utilizador sobre a conclusão e próximos passos com o abrigo.
        /// </summary>
        Task SendAdoptionConfirmationAsync(string toEmail, string userName, string animalName, string shelterName);
        /// <summary>
        /// Envia notificação de novo message no sistema de chat.
        /// Inclui preview da mensagem e link direto para o chat.
        /// </summary>
        Task SendNewMessageNotificationAsync(string toEmail, string receiverName, string senderName, string messagePreview);
    }
}